using WakaBot.Core;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddHostedService(services => new WakaBot.Core.WakaBot(args));
//builder.Services.AddHostedService<WakaBot.Core.WakaBot>();
builder.Services.AddHostedService<WakaBotService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
