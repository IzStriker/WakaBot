using CommandLine;
using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Serilog;
using WakaBot.Core.CommandLine;
using WakaBot.Core.Data;
using WakaBot.Core.Graphs;
using WakaBot.Core.Services;
using WakaBot.Core.WakaTimeAPI;
using WakaBot.Core.WebApp;
using WakaBot.Core.Commands;

namespace WakaBot.Core
{
    public class WakaBotService : IHostedService
    {
        private DiscordSocketClient? _client;
        private InteractionService? _interactionService;
        private IConfiguration? _configuration;

        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.All,
            AlwaysDownloadUsers = true,
        };

        private string[] args;

        public WakaBotService(string[] args)
        {
            this.args = args;
        }

        public WakaBotService() { }

        /// <summary>
        /// Entry point for Discord bot Component of application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {

            try
            {
                var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                var config = new ConfigurationBuilder();
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json");

                if (env != "Production")
                {
                    config.AddJsonFile("logconfig.json");
                }

                config.AddEnvironmentVariables("DOTNET_");
                _configuration = config.Build();

            }
            catch (FileNotFoundException e)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = originalColor;
                Environment.Exit(1);
            }


            var services = ConfigureServices();
            // TODO: Create new project for console
            // TODO: Only run in console app
            // and move a copy to web app
            //Parser.Default.ParseArguments<MigrationsCmd>(args)
            //    .WithParsed<MigrationsCmd>(cmd => cmd.Execute(services))
            //    .WithNotParsed(err =>
            //    {
            //        // if invalid command line args 
            //        Environment.Exit(3);
            //    });

            _client = services.GetRequiredService<DiscordSocketClient>();
            _interactionService = services.GetRequiredService<InteractionService>();

            await _client.LoginAsync(TokenType.Bot, _configuration["token"]);
            await _client.StartAsync();

            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            // Initialise WebServer if Enabled.
            if (_configuration.GetValue<bool>("runWebServer"))
            {
                await new Server().StartAsync(services);
            }

            //await Task.Delay(-1, cancellationToken);
        }

        /// <summary>
        /// Creates ServiceProvider required for dependency injection.
        /// </summary>
        /// <returns>ServiceProvider of dependency injection objects.</returns>
        private ServiceProvider ConfigureServices()
        {
            // Force Serilog to use base app directory instead of current.
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            // Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();

            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .AddSingleton(_socketConfig)
                .AddSingleton<IConfiguration>(_configuration!)
                .AddSingleton(x => new GraphGenerator(_configuration!["colourURL"]))
                .AddDbContextFactory<WakaContext>()
                .AddScoped<WakaTime>()
                .AddMemoryCache()
                .AddLogging(config => config.AddSerilog())
                .BuildServiceProvider();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
