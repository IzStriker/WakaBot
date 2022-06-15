using Discord;
using Discord.Interactions;
using WakaBot;
using WakaBot.Data;
using WakaBot.Extensions;
using Newtonsoft.Json.Linq;

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
        await DeferAsync();
        int maxPages = _wakaContext.Users.Count() / 5;

        if (page + 1 >= maxPages)
        {
            return;
        }
        page += 1;

        var statsTasks = _wakaContext.Users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));

        dynamic[] userStats = await Task.WhenAll(statsTasks);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds).Skip(page * 5).Take(5).ToArray();
        await UpdatePage(page, messageId, userStats.ToList(), maxPages);
    }

    [ComponentInteraction("last:*,*")]
    public async Task RankLast(int page, ulong messageId)
    {
        Console.WriteLine(page);
        Console.WriteLine(messageId);
        await DeferAsync();

    }

    public async Task UpdatePage(int page, ulong messageId, List<dynamic> userStats, int maxPages)
    {
        var fields = new List<EmbedFieldBuilder>();


        foreach (var stat in userStats)
        {
            string range = "\nIn " + Convert.ToString(stat.data.range).Replace("_", " ");
            string languages = "\nTop languages: ";

            // Force C# to treat dynamic object as JArray instead of JObject
            JArray lanList = JArray.Parse(Convert.ToString(stat.data.languages));

            languages += lanList.ConcatForEach(6, (token, last) =>
                $"{token.name} {token.percent}%" + (last ? "" : ", "));

            fields.Add(new EmbedFieldBuilder()
            {
                Name = stat.data.username,
                Value = stat.data.human_readable_total + range + languages
            });
        }

        await Context.Channel.ModifyMessageAsync(messageId, msg =>
        {
            msg.Embed = new EmbedBuilder
            {
                Title = "User Ranking",
                Color = Color.Purple,
                Fields = fields,
            }.Build();

            msg.Components = new ComponentBuilder()
                /// operations: (page number), (message id)
                .WithButton("⏮️", $"first:{page},{messageId}", disabled: page <= 0)
                .WithButton("◀️", $"previous:{page},{messageId}", disabled: page <= 0)
                .WithButton("▶️", $"next:{page},{messageId}", disabled: page >= maxPages - 1)
                .WithButton("⏭️", $"last:{page},{messageId}", disabled: page >= maxPages - 1)
                .Build();
        });
    }

}