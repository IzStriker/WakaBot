using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WakaBot.Core.Data;

public class MySqlDesignTimeFactory : IDesignTimeDbContextFactory<MySqlContext>
{
    public MySqlContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables("DOTNET_")
            .Build();

        return new MySqlContext(new DbContextOptionsBuilder<WakaContext>().UseMySql(config.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7))).Options);
    }
}