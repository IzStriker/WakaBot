using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WakaBot.Core.Data;
using WakaBot.Core.Graphs;
using WakaBot.Core.MessageBroker;
using WakaBot.Core.Services;
using WakaBot.Core.WakaTimeAPI;

namespace WakaBot.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddWakaBot(this IServiceCollection services)
    {
        var config = ConfigManager.Configuration;

        // Force Serilog to use base app directory instead of current.
        Environment.CurrentDirectory = AppContext.BaseDirectory;

        // Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        // setup services
        services.AddSingleton<Metrics>();
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton(() => new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            AlwaysDownloadUsers = true,
        });

        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<GraphGenerator, OxyPlotGenerator>();
        services.AddDbContextFactory<WakaContext>(opt =>
        {
            var provider = config["databaseProvider"];
            switch (provider?.ToLower())
            {
                case "sqlite":
                    opt.UseSqlite(config.GetConnectionString("Sqlite"));
                    break;
                case "mysql":
                    opt.UseMySql(config.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7)));
                    break;
                case "postgresql":
                    opt.UseNpgsql(config.GetConnectionString("PostgreSql"));
                    break;
                default:
                    throw new ArgumentException("Invalid database provider specified in appsettings.json");
            }
        });
        services.AddSingleton<CommandHandler>();
        services.AddTransient<WakaTime>();
        services.AddSingleton<MessageQueue>();
        services.AddSingleton<SubscriptionHandler>();
        services.AddMemoryCache();
        services.AddLogging(config => config.AddSerilog());

        // setup http clients
        services.AddHttpClient<WakaTime>(c => c.BaseAddress = new Uri("https://wakatime.com/api/v1/"))
            .AddHttpMessageHandler<WakaTimeCacheHandler>();

        services.AddTransient<WakaTimeCacheHandler>();
    }
}