using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WakaBot.Data;

/// <summary>
/// Used by Entity Framework when updating migrations.
/// https://stackoverflow.com/a/60602620/16322117
/// </summary>
public class WakaContextFactory : IDesignTimeDbContextFactory<WakaContext>
{
    /// <summary>
    /// Create instance of WakaTime DBContext
    /// </summary>
    /// <returns>Instance of DBContext</returns>
    public WakaContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddEnvironmentVariables()
            .AddJsonFile("logconfig.json", optional: false)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string dbType = configuration["dBType"] ?? "sqlite";
        string dbPath = Path.Join(configuration!["dBPath"] ?? AppContext.BaseDirectory,
            configuration["dBFileName"] ?? "waka.db");

        var optionsBuilder = new DbContextOptionsBuilder<WakaContext>();

        switch (dbType)
        {
            case "postgres":
               optionsBuilder.UseNpgsql($"Data Source={dbPath}");
               break;
            case "sqlite":
               optionsBuilder.UseSqlite($"Data Source={dbPath}");
               break;
        }
        return new WakaContext(optionsBuilder.Options);
    }

}
