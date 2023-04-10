
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WakaBot.Core.Services;

namespace WakaBot.Core.Data;

public class PostgreSqlDesignTimeFactory : IDesignTimeDbContextFactory<PostgreSqlContext>
{

    PostgreSqlContext IDesignTimeDbContextFactory<PostgreSqlContext>.CreateDbContext(string[] args)
    {
        var config = ConfigManager.Configuration;
        return new PostgreSqlContext(new DbContextOptionsBuilder<WakaContext>().UseNpgsql(config.GetConnectionString("PostgreSql")).Options);
    }
}
