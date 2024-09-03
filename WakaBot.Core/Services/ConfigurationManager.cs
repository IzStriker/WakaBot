namespace WakaBot.Core.Services;
/// <summary>
/// Centralised singleton object for accessing WakaBot's configuration.
/// </summary>]
/// <remarks>
/// This class is a singleton, and should be accessed via the <see cref="Configuration"/> property.
/// Moreover, this isn't inside the normal dependency injection system, so it can be used when updating the database.
/// </remarks>
public sealed class ConfigManager
{
    private static ConfigManager _instance = null!;
    private IConfiguration _configuration { get; }

    public static IConfiguration Configuration
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ConfigManager();
            }

            return _instance._configuration;
        }
    }

    private ConfigManager()
    {
        try
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var config = new ConfigurationBuilder();
            config.SetBasePath(AppContext.BaseDirectory);
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddJsonFile("../appsettings.json", optional: true);
            config.AddJsonFile("WakaBot.Core/appsettings.json", optional: true);
            config.AddJsonFile("WakaBot.Web/appsettings.json", optional: true);
            config.AddJsonFile("logconfig.json", optional: true);
            config.AddEnvironmentVariables("DOTNET_");
            _configuration = config.Build();
        }
        catch (FileNotFoundException e)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ForegroundColor = originalColor;
            Environment.Exit(1);
        }
    }
}