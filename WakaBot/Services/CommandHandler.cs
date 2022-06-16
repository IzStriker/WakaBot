using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using WakaBot.Data;

namespace WakaBot.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interaction;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly WakaTime _wakaTime;

    /// <summary>
    /// Create new instance of <c>CommandHandler</c> service.
    /// </summary>
    /// <param name="client">Instance of discord client.</param>
    /// <param name="interaction">Instance of discord interaction service</param>
    /// <param name="services">Dependency injection services</param>
    /// <param name="configuration">Discord bot configuration</param>
    public CommandHandler(DiscordSocketClient client, InteractionService interaction,
     IServiceProvider services, IConfiguration configuration, WakaTime wakaTime)
    {
        _client = client;
        _interaction = interaction;
        _services = services;
        _configuration = configuration;
        _wakaTime = wakaTime;
    }

    /// <summary>
    /// Initialises interaction framework and application event listeners.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += HandleInteraction;
        _client.Ready += ClientReady;
        _client.Log += Log;


        _interaction.SlashCommandExecuted += SlashCommandExecuted;
        _interaction.ContextCommandExecuted += ContextCommandExecuted;
        _interaction.ComponentCommandExecuted += ComponentCommandExecuted;

    }


    /// <summary>
    /// Called on one bot is ready to accept commands and registers it to a guild.
    /// </summary>
    private async Task ClientReady()
    {

        await _interaction!.RegisterCommandsToGuildAsync(_configuration.GetValue<ulong>("guildId"));
        if (_configuration.GetValue<bool>("alwaysCacheUsers"))
        {
            await _wakaTime.RefreshAllUsersAsync();
            Console.WriteLine("All users loaded into cache.");
        }
    }

    /// <summary>
    /// Log bot exceptions.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private Task Log(LogMessage message)
    {
        if (message.Exception is Discord.Commands.CommandException cmdException)
        {
            Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                + $" failed to execute in {cmdException.Context.Channel}.");
            Console.WriteLine(cmdException);
        }
        else
            Console.WriteLine($"[General/{message.Severity}] {message}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs discord bot's component exceptions.
    /// </summary>
    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Console.WriteLine(arg3.ErrorReason);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs discord bot's context command exceptions.
    /// </summary>
    private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Console.WriteLine(arg3.ErrorReason);

        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs discord bot's slash command exceptions.
    /// </summary>
    private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Console.WriteLine(arg3.ErrorReason);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Listens for and handles all interactions from clients.
    /// </summary>
    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _interaction.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error");
            Console.WriteLine(e.Message);

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}