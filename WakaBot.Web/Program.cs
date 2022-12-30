using WakaBot.Core;
using WakaBot.Core.MessageBroker;
using WakaBot.Core.OAuth2;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<WakaBotService>();
builder.Services.AddSingleton<MessageQueue>();

var client = new OAuth2Client(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () =>
{
    return "Hello World!";
});

app.MapGet("/callback", async (string code) =>
{
    var response = await client.GetTokenAsync(code);
    var queue = app.Services.GetService<MessageQueue>();
    queue!.Send("auth", response);
    return "Successfully Authenticated! You can close this window now and return to the Discord.";
});

app.Run();
