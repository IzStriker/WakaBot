using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using WakaBot.Core.Data;
using WakaBot.Core.Extensions;
using WakaBot.Core.Models;
using WakaBot.Core.WakaTimeAPI;

namespace WakaBot.Core.Commands;

/// <summary>
/// Handles user operations in WakaBot.
/// </summary>
[Group("user", "user based commands")]
public class UserModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly WakaContext _wakaContext;
    private readonly WakaTime _wakaTime;


    /// <summary>
    /// Create new instance of UserModule
    /// </summary>
    /// <param name="context">Database context.</param>
    public UserModule(
        WakaContext context,
        WakaTime wakaTime
    )
    {
        _wakaContext = context;
        _wakaTime = wakaTime;

    }

    /// <summary>
    /// Register new server member to Wakabot.
    /// </summary>
    /// <param name="wakaUser">WakaTime username of user to be registered.</param>
    [SlashCommand("register", "Register new server member to WakaBot Service")]
    public async Task RegisterUser(String wakaUser)
    {
        await DeferAsync(ephemeral: true);
        var discordUser = Context.User;
        await RegisterOAuthUser(discordUser, wakaUser);
    }


    /// <summary>
    /// Remove user from WakaBot database.
    /// </summary>
    /// <returns></returns>
    [SlashCommand("deregister", "Deregister registered wakabot user")]
    public async Task DeregisterUser()
    {
        var discordUser = Context.User;
        await DeferAsync(ephemeral: true);
        var guild = _wakaContext.DiscordGuilds.Find(Context.Guild.Id);
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
            if (discordUser?.Nickname != null)
            {
                name = discordUser.Nickname;
            }
            else if (discordUser?.Username != null)
            {
                name = discordUser.Username;
            }
            else
            {
                name = user?.WakaUser?.Username ?? "Unknown";
            }

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

    private async Task RegisterOAuthUser(IUser discordUser, string wakaUser)
    {
        var guild = _wakaContext.DiscordGuilds.Include(x => x.Users).FirstOrDefault(x => x.Id == Context.Guild.Id);
        var user = _wakaContext.DiscordUsers.Include(x => x.WakaUser)
        .FirstOrDefault(x => x.Id == discordUser.Id);

        if (
            user != null &&
            user.WakaUser != null &&
            user.WakaUser.usingOAuth &&
            user.WakaUser.ExpiresAt > DateTime.Now
        )
        {
            // if user already exists, is using OAuth and token is not expired
            // check if user is already registered in guild else add user to guild
            if (guild != null && guild.Users.FirstOrDefault(x => x.Id == user.Id) != null)
            {
                await FollowupAsync(embed: new EmbedBuilder()
                {
                    Title = "User already registered",
                    Color = Color.Red,
                    Description = $"User {discordUser.Mention} **{wakaUser}**, already registered"
                }.Build());
                return;
            }
            else
            {
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
                return;
            }
        }
        else if (user != null && user.WakaUser != null && user.WakaUser.usingOAuth && user.WakaUser.ExpiresAt < DateTime.Now)
        {
            // if user is registered using OAuth but refresh token has expired
            // clear old auth data and register user again
            user.WakaUser.AccessToken = null;
            user.WakaUser.RefreshToken = null;
            user.WakaUser.ExpiresAt = null;
            user.WakaUser.Scope = null;
        }
        else if (user != null && user.WakaUser != null && !user.WakaUser.usingOAuth)
        {
            // if user is registered using standard method and is trying to register 
            // using OAuth then update user to use oauth
            user.WakaUser.usingOAuth = true;
        }
        else if (user == null)
        {
            // if user is not already registered then create new user
            var (userId, errors) = await _wakaTime.ValidateRegistration(wakaUser);
            var fields = new List<EmbedFieldBuilder>();
            foreach (RegistrationErrors error in Enum.GetValues(typeof(RegistrationErrors)))
            {
                if (error != RegistrationErrors.None && errors.HasFlag(error))
                {
                    var (message, stop) = error.GetRegistrationErrorMessage();
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "\u200b",
                        Value = message,
                        IsInline = false
                    });
                    if (stop) break;
                }
            }

            if (!errors.Equals(RegistrationErrors.None))
            {
                await FollowupAsync(embed: new EmbedBuilder()
                {
                    Title = "Error",
                    Color = Color.Red,
                    Fields = fields
                }.Build());
                return;
            }

            user = new DiscordUser()
            {
                Id = discordUser.Id,
                WakaUser = new WakaUser()
                {
                    Username = wakaUser,
                    usingOAuth = true,
                    Id = userId == null ? await _wakaTime.GetUserIdAsync(wakaUser) : userId
                }
            };
        }

        var state = Guid.NewGuid().ToString("N");
        if (user.WakaUser != null)
        {
            user.WakaUser.State = state;
        }
        else
        {
            user.WakaUser = new WakaUser()
            {
                Username = wakaUser,
                usingOAuth = true,
                State = state
            };
        }

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

        var link = _wakaTime.GetRedirectUrl(new string[] { "email", "read_stats" }, state);

        await FollowupAsync(embed: new EmbedBuilder()
        {
            Title = "Registration",
            Color = Color.Purple,
            Description = @$"
                            Please follow the link below to complete your account Registration.

                            [Register]({link})

                            You will be noticed when you have successfully registered. The link above will expire within a short period of time. If you're unable to complete the registration you'll still be able to use WakaBot, but with reduced functionality. Rerun the command to complete registration."
        }.Build());
    }
}


