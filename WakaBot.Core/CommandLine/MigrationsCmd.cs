using CommandLine;
using WakaBot.Core.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.CommandLine;

/// <summary>
/// Handles interactions with Entity Framework Core.
/// </summary>
/// Exists so no additional tools for Entity Framework Core 
/// need to be installed while setting up the bot.
public class MigrationsCmd
{
    [Option('m', "ef-migrate", Required = false, Default = false, HelpText = "Run database migrations")]
    public bool RunMigrations { get; set; }



    public void Execute(ServiceProvider services)
    {
        using var context = services.GetService<WakaContext>();
        var logger = services.GetService<ILogger<MigrationsCmd>>()!;
        if (RunMigrations)
        {
            logger.LogInformation("Running migrations");
            context!.Database.Migrate();
            logger.LogInformation("Migrations run successfully.");
            Environment.Exit(0);
        }
        else
        {
            // Check all database migrations have been run.
            if (context!.Database.GetAppliedMigrations().ToList().Count !=
                context!.Database.GetMigrations().ToList().Count)
            {

                logger.LogError("Database migrations must be run before running the application.");
                logger.LogError("Use flags -m or --ef-migrate");
                Environment.Exit(3);
            }
        }
    }
}