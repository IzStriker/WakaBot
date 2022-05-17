using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace WakaBot.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interaction;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;


    public CommandHandler(DiscordSocketClient client, InteractionService interaction, IServiceProvider services, IConfiguration configuration)
    {
        _client = client;
        _interaction = interaction;
        _services = services;
        _configuration = configuration;
    }

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



    private async Task ClientReady()
    {
        await _interaction!.RegisterCommandsToGuildAsync(_configuration.GetValue<ulong>("guildId"));
    }
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

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Console.WriteLine(arg3.ErrorReason);
        }

        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Console.WriteLine(arg3.ErrorReason);

        }

        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Console.WriteLine(arg3.ErrorReason);
        }

        return Task.CompletedTask;
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
            Console.WriteLine("Error");
            Console.WriteLine(e.Message);

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}