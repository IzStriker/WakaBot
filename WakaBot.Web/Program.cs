using System.Diagnostics;
using System.Text;
using WakaBot.Core;
using WakaBot.Core.Data;
using WakaBot.Core.Extensions;
using WakaBot.Core.MessageBroker;
using WakaBot.Core.OAuth2;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("DOTNET_");
builder.Services.AddWakaBot();
builder.Services.AddHostedService<WakaBotService>();

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
    using var scope = app.Services.CreateScope();
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("waka_memory_usage " + Process.GetCurrentProcess().PrivateMemorySize64);

    var database = scope.ServiceProvider.GetService<WakaContext>();
    sb.AppendLine("waka_users " + database!.WakaUsers.Count());
    sb.AppendLine("waka_guilds " + database.DiscordGuilds.Count());

    Metrics cacheMetrics = app.Services.GetService<Metrics>()!;
    sb.AppendLine("waka_cache_hits " + cacheMetrics.CacheHits);
    sb.AppendLine("waka_cache_misses " + cacheMetrics.CacheMisses);
    sb.AppendLine("waka_total_response_time " + cacheMetrics.TotalResponseTime);
    sb.AppendLine("waka_number_of_requests " + cacheMetrics.NumberOfRequests);
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
