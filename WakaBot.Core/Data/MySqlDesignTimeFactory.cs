using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WakaBot.Core.Services;

namespace WakaBot.Core.Data;

public class MySqlDesignTimeFactory : IDesignTimeDbContextFactory<MySqlContext>
{
    public MySqlContext CreateDbContext(string[] args)
    {
        var config = ConfigManager.Configuration;
        return new MySqlContext(new DbContextOptionsBuilder<WakaContext>().UseMySql(config.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7))).Options);
    }
}