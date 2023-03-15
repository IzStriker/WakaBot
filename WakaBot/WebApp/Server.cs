using Serilog;

namespace WakaBot.WebApp;

public class Server
{

    public async Task StartAsync(ServiceProvider services)
    {
        var config = services.GetService<IConfiguration>()!.GetSection("webServer");
        var logger = services.GetService<ILogger<Server>>()!;

        // Force whole app to exit, not must web server
        // So control-c only needs to be pressed once
        Console.CancelKeyPress += (_, _) => Environment.Exit(0);

        var port = config["port"] ?? "3000";
        var ipAddr = config["ipAddr"] ?? "0.0.0.0";

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger: services.GetService<Serilog.ILogger>());
        var app = builder.Build();

        app.MapGet("/", () => "active");

        logger.LogInformation("Web Server running on http://{ipAddr}:{port}", ipAddr, port);
        await app.RunAsync($"http://{ipAddr}:{port}");
    }
}