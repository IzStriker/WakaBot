using Discord.Interactions;
using WakaBot.Data;
using WakaBot;

namespace Wakabot.Commands;

public class ComponentModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly WakaContext _wakaContext;
    private readonly WakaTime _wakaTime;
    public ComponentModule(WakaContext wakaContext, WakaTime wakaTime)
    {
        _wakaContext = wakaContext;
        _wakaTime = wakaTime;
    }

    [ComponentInteraction("first:*,*")]
    public async Task RankFirst(int page, ulong messageId)
    {
        Console.WriteLine(page);
        Console.WriteLine(messageId);
        await DeferAsync();
    }

    [ComponentInteraction("previous:*,*")]
    public async Task RankPrevious(int page, ulong messageId)
    {
        Console.WriteLine(page);
        Console.WriteLine(messageId);
        await DeferAsync();
    }

    [ComponentInteraction("next:*,*")]
    public async Task RankNext(int page, ulong messageId)
    {
        Console.WriteLine(page);
        Console.WriteLine(messageId);
        await DeferAsync();
    }

    [ComponentInteraction("last:*,*")]
    public async Task RankLast(int page, ulong messageId)
    {
        Console.WriteLine(page);
        Console.WriteLine(messageId);
        await DeferAsync();

    }

}