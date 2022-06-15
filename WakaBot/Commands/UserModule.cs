using Discord;
using Discord.Interactions;
using WakaBot.Data;
using WakaBot.Models;


namespace WakaBot.Commands;

/// <summary>
/// Handles user operations in WakaBot.
/// </summary>
public class UserModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly WakaContext _wakaContext;
    private readonly WakaTime _wakaTime;


    /// <summary>
    /// Create new instance of UserModule
    /// </summary>
    /// <param name="context">Database context.</param>
    public UserModule(WakaContext context, WakaTime wakaTime)
    {
        _wakaContext = context;
        _wakaTime = wakaTime;
    }


    /// <summary>
    /// Register new server member to Wakabot.
    /// </summary>
    /// <param name="discordUser">User to be registered.</param>
    /// <param name="wakaUser">WakaTime username of user to be registered.</param>
    [SlashCommand("wakaregister", "Register new server member to WakaBot Service")]
    public async Task RegisterUser(IUser discordUser, String wakaUser)
    {
        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Just checking your profile.",
            Color = Color.Orange,
            Description = "Should only take a second."
        }.Build());

        var errors = await _wakaTime.ValidateRegistration(wakaUser);

        if (errors.HasFlag(WakaTime.RegistrationErrors.UserNotFound))
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = $"Invalid username **{wakaUser}**, ensure your username is correct."
            }
            .Build());
            await DeleteOriginalResponseAsync();
            return;
        }

        string description = string.Empty;

        if (errors.HasFlag(WakaTime.RegistrationErrors.StatsNotFound))
        {
            description += "Stats not available, ensure `Display languages, editors, os, categories publicly ` is enabled in profile.\n\n";
        }

        if (errors.HasFlag(WakaTime.RegistrationErrors.TimeNotFound))
        {
            description += "Coding time not available, ensure `Display code time publicly` is enabled in profile.";
        }

        if (!errors.Equals(WakaTime.RegistrationErrors.None))
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = description
            }.Build());
            await DeleteOriginalResponseAsync();
            return;
        }

        if (_wakaContext.Users.Where(x => x.DiscordId == discordUser.Id || x.WakaName == wakaUser).Count() > 0)
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "User already registered",
                Color = Color.Red,
                Description = $"User {discordUser.Mention} **{wakaUser}**, already registered"
            }.Build());
            await DeleteOriginalResponseAsync();
            return;
        }

        _wakaContext.Add(new User() { DiscordId = discordUser.Id, WakaName = wakaUser });
        _wakaContext.SaveChanges();

        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
        {
            Title = "User registered",
            Color = Color.Green,
            Description = $"User {discordUser.Mention} register as {wakaUser}"
        }.Build());
        await DeleteOriginalResponseAsync();
    }

    /// <summary>
    /// Remove user from WakaBot database.
    /// </summary>
    /// <param name="discordUser">User to be removed.</param>
    /// <returns></returns>
    [SlashCommand("wakaderegister", "Deregister registered wakabot user")]
    public async Task DeregisterUser(IUser discordUser)
    {
        var user = _wakaContext.Users.FirstOrDefault(user => user.DiscordId == discordUser.Id);

        if (user == null)
        {
            await RespondAsync(embed: new EmbedBuilder
            {
                Color = Color.Red,
                Title = "Error",
                Description = $"User {discordUser.Username} isn't registered to WakaBot."
            }.Build());
            return;
        }

        await RespondAsync(embed: new EmbedBuilder
        {
            Color = Color.Orange,
            Title = "Hang about",
            Description = "Removing user from database"
        }.Build());

        _wakaContext.Users.Remove(user);
        _wakaContext.SaveChanges();

        await DeleteOriginalResponseAsync();
        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
        {
            Color = Color.Green,
            Title = "User Deregistered",
            Description = $"User {discordUser.Username} Successfully deregistered."
        }.Build());
    }

    /// <summary>
    /// Get list or registered users.
    /// </summary>
    [SlashCommand("wakausers", "Get list of registered users")]
    public async Task Users()
    {
        var fields = new List<EmbedFieldBuilder>();
        var users = _wakaContext.Users.ToList();
        await Context.Guild.DownloadUsersAsync();
        foreach (User user in users)
        {
            string disUser;
            var data = Context.Guild.GetUser(user.DiscordId);
            if (Context.Guild.GetUser(user.DiscordId).Nickname != null)
                disUser = Context.Guild.GetUser(user.DiscordId).Nickname;
            else
                disUser = Context.Guild.GetUser(user.DiscordId).Username;

            fields.Add(new EmbedFieldBuilder()
            {
                Name = disUser,
                Value = $"[{user.WakaName}](https://wakatime.com/@{user.WakaName})"
            });
        }

        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Registered users",
            Color = Color.Purple,
            Fields = fields
        }.Build());

    }
}

