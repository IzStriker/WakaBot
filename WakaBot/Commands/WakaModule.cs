using Discord;
using Discord.Interactions;
using WakaBot.Data;
using WakaBot.Models;

namespace WakaBot.Commands;

public class WakaModule : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("wakaping", "Recieve a pong")]
    public async Task Ping()
    {
        var embed = new EmbedBuilder()
        {
            Title = "wakapong",
            Color = Color.LightGrey,
        };
        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("wakaregister", "Register new server member to WakaTime Service")]
    public async Task RegisterUser(IUser discordUser, String wakaUser)
    {
        var errors = await WakaTime.ValidateRegistration(wakaUser);

        if (errors.HasFlag(WakaTime.RegistrationErrors.UserNotFound))
        {
            await RespondAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = $"Invalid username **{wakaUser}**, ensure your username is correct."
            }
            .Build());
            return;
        }

        string description = string.Empty;

        if (errors.HasFlag(WakaTime.RegistrationErrors.StatsNotFound))
        {
            description += "Stats not available, ensure `Display languages, editors, os, categories publicly    ` is enabled in profile.\n\n";
        }

        if (errors.HasFlag(WakaTime.RegistrationErrors.TimeNotFound))
        {
            description += "Coding time not available, ensure `Display code time publicly` is enabled in profile.";
        }

        if (!errors.Equals(WakaTime.RegistrationErrors.None))
        {
            await RespondAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = description
            }.Build());
            return;
        }

        using WakaContext context = new();

        if (context.Users.Where(x => x.DiscordId == discordUser.Id || x.WakaName == wakaUser).Count() > 0)
        {
            await RespondAsync(embed: new EmbedBuilder()
            {
                Title = "User already registered",
                Color = Color.Red,
                Description = $"User {discordUser.Mention} **{wakaUser}**, already registered"
            }.Build());
            return;
        }

        context.Add(new User() { DiscordId = discordUser.Id, WakaName = wakaUser });
        context.SaveChanges();

        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "User registered",
            Color = Color.Green,
            Description = $"User {discordUser.Mention} register as {wakaUser}"
        }.Build());
    }


    [SlashCommand("wakausers", "Get list of registered users")]
    public async Task Users()
    {
        using WakaContext context = new();

        var fields = new List<EmbedFieldBuilder>();

        foreach (User user in context.Users.ToList())
        {
            var disUser = Context.Guild.GetUser(user.DiscordId).Nickname;

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