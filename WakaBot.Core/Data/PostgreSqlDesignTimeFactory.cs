
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WakaBot.Core.Data;

public class PostgreSqlDesignTimeFactory : IDesignTimeDbContextFactory<PostgreSqlContext>
{

    PostgreSqlContext IDesignTimeDbContextFactory<PostgreSqlContext>.CreateDbContext(string[] args)
    {

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables("DOTNET_")
            .Build();

        return new PostgreSqlContext(new DbContextOptionsBuilder<WakaContext>().UseNpgsql(config.GetConnectionString("PostgreSql")).Options);
    }
}
