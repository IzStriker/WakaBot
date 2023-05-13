using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using WakaBot.Core.WakaTimeAPI;

namespace WakaBot.Core.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interaction;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly WakaTime _wakaTime;
    private readonly ILogger _logger;

    /// <summary>
    /// Create new instance of <c>CommandHandler</c> service.
    /// </summary>
    /// <param name="client">Instance of discord client.</param>
    /// <param name="interaction">Instance of discord interaction service</param>
    /// <param name="services">Dependency injection services</param>
    /// <param name="configuration">Discord bot configuration</param>
    public CommandHandler(DiscordSocketClient client, InteractionService interaction,
     IServiceProvider services, IConfiguration configuration, WakaTime wakaTime, ILogger<CommandHandler> logger)
    {
        _client = client;
        _interaction = interaction;
        _services = services;
        _configuration = configuration;
        _wakaTime = wakaTime;
        _logger = logger;
    }

    /// <summary>
    /// Initialises interaction framework and application event listeners.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _interaction.AddModulesAsync(typeof(WakaBotService).Assembly, _services);

        _client.InteractionCreated += HandleInteraction;
        _client.Ready += ClientReady;
        _client.Log += Log;
        _client.GuildAvailable += (socket) =>
        {
            _logger.LogInformation("Joined server {serverName}", socket.Name);
            return Task.CompletedTask;
        };

        _interaction.SlashCommandExecuted += SlashCommandExecuted;
        _interaction.ContextCommandExecuted += ContextCommandExecuted;
        _interaction.ComponentCommandExecuted += ComponentCommandExecuted;
    }


    /// <summary>
    /// Called on one bot is ready to accept commands and registers it to a guild.
    /// </summary>
    private async Task ClientReady()
    {

        _logger.LogInformation("Registered on {serverCount} servers", _client.Guilds.Count);
        _logger.LogInformation("Connected as {currentUser}", _client.CurrentUser);

        // Could take up to an hour.
        await _interaction!.RegisterCommandsGloballyAsync();

        if (_configuration.GetValue<bool>("alwaysCacheUsers"))
        {
            await _wakaTime.RefreshAllUsersAsync();
        }
    }

    /// <summary>
    /// Log bot exceptions.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private Task Log(LogMessage message)
    {
        // Taken from https://www.gngrninja.com/code/2019/7/19/c-discord-bot-logging-all-the-things
        string logText = $": {message.Exception?.ToString() ?? message.Message} - {message.Source}";

        switch (message.Severity.ToString())
        {
            case "Critical":
                {
                    _logger.LogCritical(logText);
                    break;
                }
            case "Warning":
                {
                    _logger.LogWarning(logText);
                    break;
                }
            case "Info":
                {
                    _logger.LogInformation(logText);
                    break;
                }
            case "Verbose":
                {
                    _logger.LogInformation(logText);
                    break;
                }
            case "Debug":
                {
                    _logger.LogDebug(logText);
                    break;
                }
            case "Error":
                {
                    _logger.LogError(logText);
                    _logger.LogError(message.Exception!.StackTrace);
                    break;
                }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs discord bot's component exceptions.
    /// </summary>
    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2,
        Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            _logger.LogError(arg3.ErrorReason);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs discord bot's context command exceptions.
    /// </summary>
    private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2,
        Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            _logger.LogError(arg3.ErrorReason);

        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs discord bot's slash command exceptions.
    /// </summary>
    private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2,
        Discord.Interactions.IResult arg3)
    {
        // get how long the command took to execute
        var time = DateTime.Now - arg2.Interaction.CreatedAt;
        if (!arg3.IsSuccess)
        {
            arg2.Interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            _logger.LogError($"Command: {arg1.Name}");
            _logger.LogError(arg3.ErrorReason);
            var error = (ExecuteResult)arg3;
            _logger.LogError(error.Exception.StackTrace);
        }
        else
        {
            _logger.LogInformation($"Command: {arg1.Name}, By {arg2.User.Username} In {arg2.Guild.Name} and took {time.TotalMilliseconds}ms");
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
            _logger.LogError(e, "interaction error");

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}