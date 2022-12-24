using Microsoft.EntityFrameworkCore;

namespace WakaBot.Data;

public class DbManager
{
    public static DbContextOptionsBuilder GetConnection(DbContextOptionsBuilder opt, IConfiguration config)
    {
        if (!string.IsNullOrWhiteSpace(config.GetConnectionString("MySql")))
        {
            return opt.UseMySql(config.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 7)));
        }

        string dbPath = Path.Join(
            config["dBPath"] ?? AppContext.BaseDirectory,
            config["dBFileName"] ?? "waka.db"
        );
        return opt.UseSqlite($"Data Source={dbPath}");
    }
}