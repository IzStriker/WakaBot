using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WakaBot.Core;
using WakaBot.Core.CommandLine;
using WakaBot.Core.Data;

var services = new ServiceCollection()
    .AddDbContextFactory<WakaContext>()
    .AddLogging()
    .BuildServiceProvider();

Parser.Default.ParseArguments<MigrationsCmd>(args)
    .WithParsed<MigrationsCmd>(cmd => cmd.Execute(services))
    .WithNotParsed(err =>
    {
        // if invalid command line args 
        Environment.Exit(3);
    });

Host.CreateDefaultBuilder().ConfigureServices((context, hServices) =>
{
    hServices.AddHostedService<WakaBotService>();
}).Build().Run();