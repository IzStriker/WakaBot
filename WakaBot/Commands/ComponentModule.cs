using Discord;
using Discord.Interactions;
using WakaBot;
using WakaBot.Data;
using WakaBot.Extensions;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;


namespace Wakabot.Commands;

public class ComponentModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly WakaContext _wakaContext;
    private readonly WakaTime _wakaTime;
    private readonly int _maxUsersPerPage;

    public ComponentModule(WakaContext wakaContext, WakaTime wakaTime, IConfiguration config)
    {
        _wakaContext = wakaContext;
        _wakaTime = wakaTime;
        _maxUsersPerPage = config["maxUsersPerPage"] != null
            ? config.GetValue<int>("maxUsersPerPage") : 4;
    }

    [ComponentInteraction("first:*,*")]
    public async Task RankFirst(int page, ulong messageId)
    {
        await DeferAsync();

        int maxPages = (int)Math.Ceiling(_wakaContext.Users.Count() / (decimal)_maxUsersPerPage);
        page = 0;

        var statsTasks = _wakaContext.Users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));
        dynamic[] userStats = await Task.WhenAll(statsTasks);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds)
            .Take(_maxUsersPerPage).ToArray();
        await UpdatePage(page, messageId, userStats.ToList(), maxPages);
    }

    [ComponentInteraction("previous:*,*")]
    public async Task RankPrevious(int page, ulong messageId)
    {
        await DeferAsync();

        int maxPages = (int)Math.Ceiling(_wakaContext.Users.Count() / (decimal)_maxUsersPerPage);

        page--;

        var statsTasks = _wakaContext.Users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));
        dynamic[] userStats = await Task.WhenAll(statsTasks);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds)
            .Skip(page * _maxUsersPerPage).Take(_maxUsersPerPage).ToArray();
        await UpdatePage(page, messageId, userStats.ToList(), maxPages);
    }

    [ComponentInteraction("next:*,*")]
    public async Task RankNext(int page, ulong messageId)
    {
        await DeferAsync();
        int maxPages = (int)Math.Ceiling(_wakaContext.Users.Count() / (decimal)_maxUsersPerPage);

        page++;

        var statsTasks = _wakaContext.Users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));

        dynamic[] userStats = await Task.WhenAll(statsTasks);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds)
            .Skip(page * _maxUsersPerPage).Take(_maxUsersPerPage).ToArray();
        await UpdatePage(page, messageId, userStats.ToList(), maxPages);
    }

    [ComponentInteraction("last:*,*")]
    public async Task RankLast(int page, ulong messageId)
    {
        await DeferAsync();

        int maxPages = (int)Math.Ceiling(_wakaContext.Users.Count() / (decimal)_maxUsersPerPage);

        page = maxPages - 1;

        var statsTasks = _wakaContext.Users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));
        dynamic[] userStats = await Task.WhenAll(statsTasks);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds)
            .TakeLast(_maxUsersPerPage).ToArray();
        await UpdatePage(page, messageId, userStats.ToList(), maxPages);
    }

    public async Task UpdatePage(int page, ulong messageId, List<dynamic> userStats, int maxPages)
    {
        var fields = new List<EmbedFieldBuilder>();

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Total programming time",
            Value = await GetTotalTimeAsync(),
        });

        foreach (var user in userStats.Select((value, index) => new { value, index }))
        {
            string range = "\nIn " + Convert.ToString(user.value.data.range).Replace("_", " ");
            string languages = "\nTop languages: ";

            // Force C# to treat dynamic object as JArray instead of JObject
            JArray lanList = JArray.Parse(Convert.ToString(user.value.data.languages));

            languages += lanList.ConcatForEach(6, (token, last) =>
                $"{token.name} {token.percent}%" + (last ? "" : ", "));

            int position = user.index + 1 + (page * _maxUsersPerPage);
            fields.Add(new EmbedFieldBuilder()
            {
                Name = $"#{position} - " + user.value.data.username,
                Value = user.value.data.human_readable_total + range + languages,
            });
        }

        await Context.Channel.ModifyMessageAsync(messageId, msg =>
        {
            msg.Embed = new EmbedBuilder
            {
                Title = "User Ranking",
                Color = Color.Purple,
                Fields = fields,
                Footer = new EmbedFooterBuilder() { Text = $"page {page + 1} of {maxPages}" },
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

    private async Task<string> GetTotalTimeAsync()
    {
        var statsTasks = _wakaContext.Users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));
        dynamic[] userStats = await Task.WhenAll(statsTasks);

        int totalSeconds = 0;

        foreach (dynamic stat in userStats)
        {
            totalSeconds += Convert.ToDouble(stat.data.total_seconds);
        }

        return $"{(int)totalSeconds / (60 * 60)} hrs {(int)(totalSeconds % (60 * 60)) / 60} mins";
    }

}