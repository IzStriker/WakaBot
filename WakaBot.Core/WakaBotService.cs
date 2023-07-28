using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using WakaBot.Core.Services;

namespace WakaBot.Core
{
    public class WakaBotService : IHostedService
    {
        private IServiceProvider _services;

        public WakaBotService(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Entry point for Discord bot Component of application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var config = ConfigManager.Configuration;

            var _client = _services.GetRequiredService<DiscordSocketClient>();
            var _interactionService = _services.GetRequiredService<InteractionService>();

            await _client.LoginAsync(TokenType.Bot, config["token"]);
            await _client.StartAsync();

            await _services.GetRequiredService<CommandHandler>().InitializeAsync();
            _services.GetRequiredService<SubscriptionHandler>().Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
