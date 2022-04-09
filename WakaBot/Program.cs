using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using WakaBot.Data;
using WakaBot.Models;
using Microsoft.Extensions.DependencyInjection;
using WakaBot.Services;

namespace WakaBot;

public class WakaBot
{
    public static Task Main(string[] args) => new WakaBot().MainAsync();

    private DiscordSocketClient? _client;
    private InteractionService? _interactionService;

    public async Task MainAsync()
    {
        using var services = ConfigureServices();
        _client = services.GetRequiredService<DiscordSocketClient>();
        _interactionService = services.GetRequiredService<InteractionService>();

        _client.Log += Log;
        _client.Ready += ClientReady;

        var token = File.ReadAllText("token.txt");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await services.GetRequiredService<CommandHandler>().InitializeAsync();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }


    private async Task GetUsers(SocketMessage msg)
    {
        using WakaContext context = new();

        // Format users in table

        string column = "+".PadRight(27, '-') + "+".PadRight(27, '-') + "+\n";

        string output = "```\n";
        output += column;
        output += String.Format("| {0,-25}| {1,-25}|\n", "User", "WakaName");
        output += column;


        foreach (User user in context.Users.ToList())
        {
            var dUser = _client!.GetUser(user.DiscordId).Username;
            output += $"| {dUser,-25}| {user.WakaName,-25}|\n";
        }

        output += column;
        output += "```";
        await msg.Channel.SendMessageAsync(output);
    }


    private static async Task GetRanking(SocketMessage msg)
    {
        using WakaContext context = new();

        var users = context.Users.ToList();
        List<Task<dynamic>> stats = new List<Task<dynamic>>();

        foreach (var user in users)
        {
            stats.Add(WakaTime.GetStatsAsync(user.WakaName));
        }
        dynamic[] userStats = await Task.WhenAll(stats);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds).ToArray();
        String output = String.Empty;

        foreach (var stat in userStats)
        {
            output += $"**{stat.data.username}**\n";
            output += $"\t {stat.data.human_readable_total}\n";
        }
        await msg.Channel.SendMessageAsync(output);
    }

    private async Task ClientReady()
    {
        await _interactionService.RegisterCommandsToGuildAsync(753255439403319326);
    }

    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }
}
