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

    private async Task CommandHandler(SocketMessage msg)
    {
        if (!msg.Content.StartsWith('~')) return;

        if (msg.Author.IsBot) return;

        string message = msg.Content.Substring(1).ToLower().Split(" ")[0];

        switch (message)
        {
            case "ping":
                await msg.Channel.SendMessageAsync("pong");
                break;
            case "register":
                await RegisterUser(msg);
                break;
            case "users":
                await GetUsers(msg);
                break;
            case "ranking":
                await GetRanking(msg);
                break;
            default:
                await msg.Channel.SendMessageAsync("Invalid command");
                break;
        }
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

    private async Task RegisterUser(SocketMessage msg)
    {
        var options = msg.Content.Split(" ");

        if (msg.MentionedUsers == null) return;

        if (options.Length != 3)
        {
            await msg.Channel.SendMessageAsync("Use format: `~register [@user] [wakaname]`");
            return;
        }

        if (msg.MentionedUsers.Count < 1)
        {
            await msg.Channel.SendMessageAsync("No User mentioned");
            return;
        }

        var errors = await WakaTime.ValidateRegistration(options[2]);

        if (errors.HasFlag(WakaTime.RegistrationErrors.UserNotFound))
        {
            await msg.Channel.SendMessageAsync($"Invalid user {options[2]}, ensure your username is correct.");
            return;
        }

        if (errors.HasFlag(WakaTime.RegistrationErrors.StatsNotFound))
        {
            await msg.Channel.SendMessageAsync($"Stats not avaible for {options[2]}," +
                $" ensure `Display languages, editors, os, categories publicly.` is selected in profile.");
        }

        if (errors.HasFlag(WakaTime.RegistrationErrors.TimeNotFound))
        {
            await msg.Channel.SendMessageAsync($"Coding time not avable for {options[2]}," +
                " ensure `Display code time publicly` is selected in profile.");
        }

        if (!errors.Equals(WakaTime.RegistrationErrors.None)) return;

        using WakaContext context = new();

        context.Add(new User() { DiscordId = msg.MentionedUsers.First().Id, WakaName = options[2] });
        context.SaveChanges();

        await msg.Channel.SendMessageAsync($"User {msg.MentionedUsers.FirstOrDefault()!.Mention} register as {options[2]}");
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
