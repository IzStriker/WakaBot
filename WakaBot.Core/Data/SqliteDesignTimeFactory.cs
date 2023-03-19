using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WakaBot.Core.Data;

public class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<SqliteContext>
{
    public SqliteContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables("DOTNET_")
            .Build();

        return new SqliteContext(new DbContextOptionsBuilder<WakaContext>().UseSqlite(config.GetConnectionString("Sqlite")).Options);
    }
}