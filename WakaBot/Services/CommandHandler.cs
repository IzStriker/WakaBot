using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace WakaBot.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interaction;
    private readonly IServiceProvider _services;

    public CommandHandler(DiscordSocketClient client, InteractionService interaction, IServiceProvider services)
    {
        _client = client;
        _interaction = interaction;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += HandleInteraction;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _interaction.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}