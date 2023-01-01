using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using WakaBot.Core.Data;
using WakaBot.Core.MessageBroker;
using WakaBot.Core.OAuth2;

namespace WakaBot.Core.Services;

public class SubscriptionHandler
{
    private readonly MessageQueue _messageQueue;
    private readonly DiscordSocketClient _client;
    private readonly ILogger _logger;
    private readonly IDbContextFactory<WakaContext> _contextFactory;

    public SubscriptionHandler(
        MessageQueue messageQueue,
        DiscordSocketClient client,
        ILogger<SubscriptionHandler> logger,
        IDbContextFactory<WakaContext> contextFactory)
    {
        _messageQueue = messageQueue;
        _client = client;
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public void Initialize()
    {
        _messageQueue.Subscribe<TokenResponse>("auth:success", async (res) =>
        {
            using var context = _contextFactory.CreateDbContext();
            var discordUser = context.DiscordUsers.Include(x => x.WakaUser).FirstOrDefault(x => x.WakaUserId == res.Uid);
            if (discordUser == null || discordUser.WakaUser == null)
            {
                return;
            }
            if (discordUser.WakaUser.State != res.State || discordUser.WakaUser.State == null)
            {
                return;
            }

            discordUser.WakaUser.AccessToken = res.AccessToken;
            discordUser.WakaUser.RefreshToken = res.RefreshToken;
            discordUser.WakaUser.ExpiresAt = res.ExpiresAt;
            discordUser.WakaUser.Scope = res.Scope;
            discordUser.WakaUser.State = null;

            context.SaveChanges();

            await _client.GetUser(discordUser.Id).SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "WakaBot OAuth2",
                Description = $"Your WakaTime account has been successfully linked to your Discord account until {res.ExpiresAt.ToLongDateString()}.",
                Color = Color.Green
            }.Build());
            _logger.LogInformation($"Successfully authenticated {discordUser.Id}.");
        });

        _messageQueue.Subscribe<ErrorResponse>("auth:fail", async (res) =>
        {
            using var context = _contextFactory.CreateDbContext();
            var discordUser = context.DiscordUsers.Include(x => x.WakaUser).FirstOrDefault(x => x.WakaUser!.State == res.State);
            if (discordUser == null || discordUser.WakaUser == null)
            {
                return;
            }

            discordUser.WakaUser.State = null;
            discordUser.WakaUser.usingOAuth = false;

            context.SaveChanges();

            await _client.GetUser(discordUser.Id).SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "WakaBot OAuth2",
                Description = @$"There was an error authenticating your WakaTime account:
                 `{res.Description}`
                 You will still be able to use the bot, but you will not be able to use the OAuth2 feature.",
                Color = Color.Red
            }.Build());
            _logger.LogInformation($"Failed to authenticate {discordUser.Id}.");
        });
    }
}