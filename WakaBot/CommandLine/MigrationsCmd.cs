using CommandLine;
using WakaBot.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WakaBot.CommandLine;

public class MigrationsCmd
{
    [Option('m', "--ef-migrate", Required = false, Default = false, HelpText = "Set to run Database Migrations")]
    public bool RunMigrations { get; set; }

    public void Execute(ServiceProvider services)
    {
        using var context = services.GetService<WakaContext>();
        if (RunMigrations)
        {
            Console.WriteLine("Running migrations");
            context!.Database.Migrate();
            Console.WriteLine("Migrations run successfully.");
            Environment.Exit(0);
        }
        else
        {
            if (context!.Database.GetAppliedMigrations().ToList().Count == 0)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database migrations must be run before running the application.");
                Console.WriteLine("Use flags -m or --ef-migrate");
                Console.ForegroundColor = originalColor;
                Environment.Exit(3);
            }
        }
    }
}