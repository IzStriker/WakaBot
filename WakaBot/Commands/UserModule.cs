using Discord;
using Discord.Interactions;
using WakaBot.Data;
using WakaBot.Models;
using WakaBot.WakaTimeAPI;


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
        await DeferAsync();

        var errors = await _wakaTime.ValidateRegistration(wakaUser);
        string description = string.Empty;

        if (errors.HasFlag(WakaTime.RegistrationErrors.UserNotFound))
        {
            await FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = $"Invalid username **{wakaUser}**, ensure your username is correct."
            }.Build());
            return;
        }

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
            await FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = description
            }.Build());
            return;
        }

        // Get users in current guild with matching discordId or WakaId
        if (_wakaContext.Users.Where(x => x.GuildId == Context.Guild.Id &&
            (x.DiscordId == discordUser.Id || x.WakaName == wakaUser)).Count() > 0)
        {
            await FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "User already registered",
                Color = Color.Red,
                Description = $"User {discordUser.Mention} **{wakaUser}**, already registered"
            }.Build());
            return;
        }

        _wakaContext.Add(new User() { DiscordId = discordUser.Id, WakaName = wakaUser, GuildId = Context.Guild.Id, });
        _wakaContext.SaveChanges();

        await FollowupAsync(embed: new EmbedBuilder()
        {
            Title = "User registered",
            Color = Color.Green,
            Description = $"User {discordUser.Mention} register as {wakaUser}"
        }.Build()
        );
    }

    /// <summary>
    /// Remove user from WakaBot database.
    /// </summary>
    /// <param name="discordUser">User to be removed.</param>
    /// <returns></returns>
    [SlashCommand("wakaderegister", "Deregister registered wakabot user")]
    public async Task DeregisterUser(IUser discordUser)
    {
        await DeferAsync();

        var user = _wakaContext.Users.FirstOrDefault(user => user.DiscordId == discordUser.Id &&
             user.GuildId == Context.Guild.Id);

        if (user == null)
        {
            await FollowupAsync(embed: new EmbedBuilder
            {
                Color = Color.Red,
                Title = "Error",
                Description = $"User {discordUser.Username} isn't registered to WakaBot."
            }.Build());
            return;
        }

        _wakaContext.Users.Remove(user);
        _wakaContext.SaveChanges();

        await FollowupAsync(embed: new EmbedBuilder()
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
        var users = _wakaContext.Users.Where(user => user.GuildId == Context.Guild.Id).ToList();
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

