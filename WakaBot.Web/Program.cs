using WakaBot.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<WakaBotService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
