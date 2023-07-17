using System.Diagnostics;
using System.Text;
using WakaBot.Core;
using WakaBot.Core.Data;
using WakaBot.Core.MessageBroker;
using WakaBot.Core.OAuth2;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("DOTNET_");

builder.Services.AddHostedService<WakaBotService>();
builder.Services.AddSingleton<MessageQueue>();

var client = new OAuth2Client(builder.Configuration);

var stopwatch = Stopwatch.StartNew();
var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () =>
{
    return "Up time is " + stopwatch.Elapsed;
});

app.MapGet("/metrics", () =>
{
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("waka_uptime " + stopwatch.Elapsed);
    sb.AppendLine("waka_memory_usage" + Process.GetCurrentProcess().PrivateMemorySize64);
    var database = app.Services.GetService<WakaContext>();
    sb.AppendLine("waka_users " + database.WakaUsers.ToList().Count());
    sb.AppendLine("waka_guilds " + database.DiscordGuilds.ToList().Count());
    return sb.ToString();
});

app.MapGet("/callback", async (string? code, string state, string? error, string? error_description) =>
{
    var queue = app.Services.GetService<MessageQueue>();
    if (code != null)
    {
        var response = await client.GetTokenAsync(code);
        response.State = state;
        queue!.Send("auth:success", response);
        return "Successfully Authenticated! You can close this window now and return to the Discord.";
    }
    else if (error != null && error_description != null)
    {
        queue!.Send("auth:fail", new ErrorResponse
        {
            Error = error,
            Description = error_description,
            State = state
        });

        return $"Error: {error_description}";
    }
    return "Something went wrong. :/";
});

app.Run();
