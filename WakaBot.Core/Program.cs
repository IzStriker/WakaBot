namespace WakaBot.Core;

/// <summary>
/// Main class for WakaTime Discord bot.
/// </summary>
public class Program
{

    /// <summary>
    /// Entry Point for console app.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static Task Main(string[] args) => new WakaBotService(args).StartAsync(new CancellationToken());


    /// <summary>
    /// Allow creation of migrations without running main.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder();
    }
}
