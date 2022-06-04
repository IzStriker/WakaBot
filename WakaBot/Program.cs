﻿using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CommandLine;
using WakaBot.Services;
using WakaBot.Graphs;
using WakaBot.Data;
using WakaBot.CommandLine;


namespace WakaBot;

public class WakaBot
{
    public static Task Main(string[] args) => new WakaBot().MainAsync(args);

    private DiscordSocketClient? _client;
    private InteractionService? _interactionService;
    private IConfiguration? _configuration;

    private readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.All,
        AlwaysDownloadUsers = true,
    };

    public async Task MainAsync(string[] args)
    {

        try
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddEnvironmentVariables()
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
            .WithParsed<MigrationsCmd>(cmd => cmd.Execute(services));

        _client = services.GetRequiredService<DiscordSocketClient>();
        _interactionService = services.GetRequiredService<InteractionService>();


        await _client.LoginAsync(TokenType.Bot, _configuration["token"]);
        await _client.StartAsync();

        await services.GetRequiredService<CommandHandler>().InitializeAsync();

        await Task.Delay(-1);
    }

    private ServiceProvider ConfigureServices()
    {
        string dbPath = Path.Join(_configuration!["dBPath"] ?? AppContext.BaseDirectory,
                 _configuration["dBFileName"] ?? "waka.db");

        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandler>()
            .AddSingleton(_socketConfig)
            .AddSingleton<IConfiguration>(_configuration!)
            .AddSingleton(x => new GraphGenerator())
            .AddDbContext<WakaContext>(opt => opt.UseSqlite($"Data Source={dbPath}"))
            .BuildServiceProvider();
    }
}
