using Discord;
using Discord.Interactions;

namespace WakaBot.Commands;

public class WakaModule : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("wakaping", "Recieve a pong")]
    public async Task Ping()
    {
        var embed = new EmbedBuilder()
        {
            Title = "wakapong",
            Color = Color.LightGrey,
        };
        await RespondAsync("", embed: embed.Build());
    }
}