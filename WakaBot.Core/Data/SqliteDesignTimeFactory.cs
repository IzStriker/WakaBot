using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WakaBot.Core.Services;

namespace WakaBot.Core.Data;

public class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<SqliteContext>
{
    public SqliteContext CreateDbContext(string[] args)
    {
        var config = ConfigManager.Configuration;
        return new SqliteContext(new DbContextOptionsBuilder<WakaContext>().UseSqlite(config.GetConnectionString("Sqlite")).Options);
    }
}