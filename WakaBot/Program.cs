using Discord;
using Discord.WebSocket;
using WakaBot.Data;
using WakaBot.Models;

namespace WakaBot;

public class WakaBot
{
    public static Task Main(string[] args) => new WakaBot().MainAsync();

    private DiscordSocketClient? _client;

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient();

        _client.MessageReceived += CommandHandler;
        _client.Log += Log;

        var token = File.ReadAllText("token.txt");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private Task CommandHandler(SocketMessage msg)
    {
        if (!msg.Content.StartsWith('~')) return Task.CompletedTask;

        if (msg.Author.IsBot) return Task.CompletedTask;

        string message = msg.Content.Substring(1).ToLower().Split(" ")[0];

        switch (message)
        {
            case "ping":
                msg.Channel.SendMessageAsync("pong");
                break;
            case "register":
                RegisterUser(msg);
                break;
            case "users":
                GetUsers(msg);
                break;
            default:
                msg.Channel.SendMessageAsync("Invalid command");
                break;
        }

        return Task.CompletedTask;
    }

    private async void GetUsers(SocketMessage msg)
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

    private async void RegisterUser(SocketMessage msg)
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

        using WakaContext context = new();


        context.Add(new User() { DiscordId = msg.MentionedUsers.First().Id, WakaName = options[2] });
        context.SaveChanges();



        await msg.Channel.SendMessageAsync($"User {msg.MentionedUsers.FirstOrDefault()!.Mention} register as {options[2]}");
    }
}
