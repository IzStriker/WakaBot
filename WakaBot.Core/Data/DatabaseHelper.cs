namespace WakaBot.Core.Data;

public class DatabaseHelper
{
    public static void GetContext(IConfiguration config, IServiceCollection services)
    {
        var dbType = config["DatabaseType"].ToLower() ?? "sqlite";
        switch (dbType)
        {
            case "sqlite":
                services.AddDbContextFactory<SqliteContext>();
                break;
            case "mysql":
                services.AddDbContextFactory<MySqlContext>();
                break;
            default:
                throw new Exception("Invalid database type.");
        }
    }

    public static WakaContext GetContext(IConfiguration config)
    {
        var dbType = config["DatabaseType"].ToLower() ?? "sqlite";
        switch (dbType)
        {
            case "sqlite":
                return new SqliteContext();
            case "mysql":
                return new MySqlContext();
            default:
                throw new Exception("Invalid database type.");
        }
    }
}