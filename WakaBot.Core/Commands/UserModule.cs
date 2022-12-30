using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using WakaBot.Core.Data;
using WakaBot.Core.Models;
using WakaBot.Core.WakaTimeAPI;
using WakaBot.Core.OAuth2;
using WakaBot.Core.MessageBroker;
using Discord.WebSocket;

namespace WakaBot.Core.Commands;

/// <summary>
/// Handles user operations in WakaBot.
/// </summary>
[Group("user", "user based commands")]
public class UserModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly WakaContext _wakaContext;
    private readonly WakaTime _wakaTime;
    private readonly OAuth2Client _oAuth2Client;


    /// <summary>
    /// Create new instance of UserModule
    /// </summary>
    /// <param name="context">Database context.</param>
    public UserModule(
        WakaContext context,
        WakaTime wakaTime,
        OAuth2Client oAuth2Client,
        DiscordSocketClient client,
        MessageQueue queue
    )
    {
        _wakaContext = context;
        _wakaTime = wakaTime;
        _oAuth2Client = oAuth2Client;

        RegisterSubscriptions(queue, client);
    }

    private void RegisterSubscriptions(MessageQueue queue, DiscordSocketClient client)
    {
        queue.Subscribe<TokenResponse>("auth", async (res) =>
        {
            var discordUser = _wakaContext.DiscordUsers.Include(x => x.WakaUser).FirstOrDefault(x => x.WakaUserId == res.Uid);
            if (discordUser == null)
            {
                return;
            }
            if (discordUser.WakaUser != null)
            {
                discordUser.WakaUser.AccessToken = res.AccessToken;
                discordUser.WakaUser.RefreshToken = res.RefreshToken;
                discordUser.WakaUser.ExpiresAt = res.ExpiresAt;
                discordUser.WakaUser.Scope = res.Scope;

                _wakaContext.SaveChanges();
            }

            await client.GetUser(discordUser.Id).SendMessageAsync("Your WakaTime account has been successfully linked to your Discord account.");
        });
    }

    /// <summary>
    /// Register new server member to Wakabot.
    /// </summary>
    /// <param name="discordUser">User to be registered.</param>
    /// <param name="wakaUser">WakaTime username of user to be registered.</param>
    [SlashCommand("register", "Register new server member to WakaBot Service")]
    public async Task RegisterUser(IUser discordUser, String wakaUser, bool oAuth = false)
    {
        await DeferAsync(ephemeral: true);

        if (oAuth)
        {
            await RegisterOAuthUser(discordUser, wakaUser);
        }
        else
        {
            await RegisterStandardUser(discordUser, wakaUser);
        }

    }


    /// <summary>
    /// Remove user from WakaBot database.
    /// </summary>
    /// <param name="discordUser">User to be removed.</param>
    /// <returns></returns>
    [SlashCommand("deregister", "Deregister registered wakabot user")]
    public async Task DeregisterUser(IUser discordUser)
    {
        await DeferAsync(ephemeral: true);

        var guild = _wakaContext.DiscordGuilds.Include(x => x.Users)
            .FirstOrDefault(x => x.Id == Context.Guild.Id);
        var user = guild?.Users.FirstOrDefault(x => x.Id == discordUser.Id);


        if (user == null || guild == null)
        {
            await FollowupAsync(embed: new EmbedBuilder
            {
                Color = Color.Red,
                Title = "Error",
                Description = $"User {discordUser.Mention} isn't registered to WakaBot."
            }.Build());
            return;
        }

        guild.Users.Remove(user);
        _wakaContext.SaveChanges();

        await FollowupAsync(embed: new EmbedBuilder()
        {
            Color = Color.Green,
            Title = "User Deregistered",
            Description = $"User {discordUser.Mention} Successfully deregistered."
        }.Build());
    }

    /// <summary>
    /// Get list or registered users.
    /// </summary>
    [SlashCommand("list", "Get list of registered users")]
    public async Task Users()
    {
        var fields = new List<EmbedFieldBuilder>();
        var users = _wakaContext.DiscordGuilds.Include(x => x.Users).ThenInclude(x => x.WakaUser)
            .FirstOrDefault(x => x.Id == Context.Guild.Id)?.Users.ToList();
        await Context.Guild.DownloadUsersAsync();

        if (users == null || users.Count() == 0)
        {
            await RespondAsync(embed: new EmbedBuilder()
            {
                Title = "No Registered Users",
                Color = Color.Red
            }.Build());
            return;
        }

        foreach (var user in users)
        {
            string name;
            var discordUser = Context.Guild.GetUser(user.Id);
            if (discordUser.Nickname != null)
                name = discordUser.Nickname;
            else
                name = discordUser.Username;

            if (user.WakaUser != null)
            {

                fields.Add(new EmbedFieldBuilder()
                {
                    Name = name,
                    Value = $"[{user.WakaUser.Username}](https://wakatime.com/@{user.WakaUser.Username})"
                });
            }
        }

        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Registered Users",
            Color = Color.Purple,
            Fields = fields
        }.Build());

    }

    private async Task RegisterStandardUser(IUser discordUser, String wakaUser)
    {
        var (userId, errors) = await _wakaTime.ValidateRegistration(wakaUser);
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
        if (_wakaContext.DiscordGuilds.Include(x => x.Users)
                .FirstOrDefault(x => x.Id == Context.Guild.Id)?
                .Users.FirstOrDefault(x => x.Id == discordUser.Id) != null)
        {
            await FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "User already registered",
                Color = Color.Red,
                Description = $"User {discordUser.Mention} **{wakaUser}**, already registered"
            }.Build());
            return;
        }

        var user = _wakaContext.DiscordUsers.FirstOrDefault(x => x.Id == discordUser.Id);
        if (user == null)
        {
            user = new DiscordUser()
            {
                Id = discordUser.Id,
                WakaUser = new WakaUser()
                {
                    Username = wakaUser,
                    usingOAuth = false,
                    Id = userId == null ? await _wakaTime.GetUserIdAsync(wakaUser) : userId
                }
            };
        }

        var guild = _wakaContext.DiscordGuilds.FirstOrDefault(x => x.Id == Context.Guild.Id);
        if (guild == null)
        {
            guild = new DiscordGuild()
            {
                Id = Context.Guild.Id,
            };
            guild.Users.Add(user);
            _wakaContext.DiscordGuilds.Add(guild);
        }
        else
        {
            guild.Users.Add(user);
        }

        _wakaContext.SaveChanges();

        await FollowupAsync(embed: new EmbedBuilder()
        {
            Title = "User registered",
            Color = Color.Green,
            Description = $"User {discordUser.Mention} register as {wakaUser}"
        }.Build()
        );
    }
    private async Task RegisterOAuthUser(IUser discordUser, string wakaUser)
    {
        var (userId, errors) = await _wakaTime.ValidateRegistration(wakaUser);

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

        var user = new DiscordUser()
        {
            Id = discordUser.Id,
            WakaUser = new WakaUser()
            {
                Username = wakaUser,
                usingOAuth = true,
                Id = userId == null ? await _wakaTime.GetUserIdAsync(wakaUser) : userId
            }
        };

        var guild = _wakaContext.DiscordGuilds.Include(x => x.Users).FirstOrDefault(x => x.Id == Context.Guild.Id);
        if (guild == null)
        {
            guild = new DiscordGuild()
            {
                Id = Context.Guild.Id,
            };
            guild.Users.Add(user);
            _wakaContext.DiscordGuilds.Add(guild);
        }
        else
        {
            guild.Users.Add(user);
        }
        _wakaContext.SaveChanges();

        var link = _oAuth2Client.GetRedirectUrl(new string[] { "email", "read_stats" });

        await FollowupAsync(embed: new EmbedBuilder()
        {
            Title = "OAuth Registration",
            Color = Color.Purple,
            Description = $"Please follow the link below to register your account.\n\n[Register]({link})"
        }.Build());
    }
}


