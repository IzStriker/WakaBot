using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommandLine;
using WakaBot.Services;
using WakaBot.Graphs;
using WakaBot.Data;
using WakaBot.CommandLine;
using Serilog;

namespace WakaBot;

/// <summary>
/// Main class for WakaTime Discord bot.
/// </summary>
public class WakaBot
{
    /// <summary>
    /// Entry Point for console app.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static Task Main(string[] args) => new WakaBot().MainAsync(args);

    private DiscordSocketClient? _client;
    private InteractionService? _interactionService;
    private IConfiguration? _configuration;

    private readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.All,
        AlwaysDownloadUsers = true,
    };

    /// <summary>
    /// Entry point for Discord bot Component of application.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public async Task MainAsync(string[] args)
    {

        try
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddEnvironmentVariables()
                .AddJsonFile("logconfig.json", optional: false)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        }
        catch (FileNotFoundException e)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = originalColor;
            Environment.Exit(1);
        }


        using var services = ConfigureServices();

        Parser.Default.ParseArguments<MigrationsCmd>(args)
            .WithParsed<MigrationsCmd>(cmd => cmd.Execute(services))
            .WithNotParsed(err =>
            {
                Environment.Exit(3);
            });

        _client = services.GetRequiredService<DiscordSocketClient>();
        _interactionService = services.GetRequiredService<InteractionService>();

        await _client.LoginAsync(TokenType.Bot, _configuration["token"]);
        await _client.StartAsync();

        await services.GetRequiredService<CommandHandler>().InitializeAsync();

        await Task.Delay(-1);
    }

    /// <summary>
    /// Creates ServiceProvider required for dependency injection.
    /// </summary>
    /// <returns>ServiceProvider of dependency injection objects.</returns>
    private ServiceProvider ConfigureServices()
    {
        // Setup database connection path using default values.
        string dbPath = Path.Join(_configuration!["dBPath"] ?? AppContext.BaseDirectory,
                 _configuration["dBFileName"] ?? "waka.db");

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
            .AddSingleton(x => new GraphGenerator(_configuration["colourURL"]))
            .AddDbContextFactory<WakaContext>(opt => opt.UseSqlite($"Data Source={dbPath}"))
            .AddScoped<WakaTime>()
            .AddMemoryCache()
            .AddLogging(config => config.AddSerilog())
            .BuildServiceProvider();
    }
}
