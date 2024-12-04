using Discord;
using Discord.Interactions;
using WakaBot.Core.WakaTimeAPI;
using WakaBot.Core.Data;
using WakaBot.Core.Extensions;
using WakaBot.Core.WakaTimeAPI.Stats;
using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Commands;

/// <summary>
/// Handles interactions with components.
/// </summary>
public class ComponentModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly WakaContext _wakaContext;
    private readonly WakaTime _wakaTime;
    private readonly int _maxUsersPerPage;
    private readonly ILogger<ComponentModule> _logger;

    /// <summary>
    /// Create instance of ComponentModule.
    /// </summary>
    /// <param name="wakaContext">Database context.</param>
    /// <param name="wakaTime">Instance of WakaTime class.</param>
    /// <param name="config">Instance of global configuration.</param>
    /// <param name="logger">Instance of logger.</param>
    public ComponentModule(
        WakaContext wakaContext,
        WakaTime wakaTime,
        IConfiguration config,
        ILogger<ComponentModule> logger
    )
    {
        _wakaContext = wakaContext;
        _wakaTime = wakaTime;
        _maxUsersPerPage = config["maxUsersPerPage"] != null
            ? config.GetValue<int>("maxUsersPerPage") : 3;
        _logger = logger;
    }

    [ComponentInteraction("rank-*:*,*,*")]
    public async Task HandlePagination(string operation, int page, ulong messageId, string? rawTimeRange)
    {
        await DeferAsync();

        var users = _wakaContext
            .DiscordGuilds
            .Include(x => x.Users)
            .ThenInclude(x => x.WakaUser)
            .FirstOrDefault(guild => guild.Id == Context.Guild.Id)?.Users;

        if (users == null || users.Count() == 0)
        {
            _logger.LogWarning("No users found in guild {guildId} for rank pagination", Context.Guild.Id);
            return;
        }

        TimeRange? timeRange = null;
        if (!string.IsNullOrEmpty(rawTimeRange) && rawTimeRange != "none")
        {
            timeRange = Enum.Parse(typeof(TimeRange), rawTimeRange, true) as TimeRange?;
            users = users.Where(user => user.WakaUser != null && user.WakaUser.usingOAuth).ToList();
        }

        int maxPages = (int)Math.Ceiling(users.Count() / (decimal)_maxUsersPerPage);

        switch (operation)
        {
            case "first":
                page = 0;
                break;
            case "next":
                page++;
                break;
            case "previous":
                page--;
                break;
            case "last":
                page = maxPages - 1;
                break;
        }

        var statsTasks = timeRange == null ?
            users.Select(user => _wakaTime.GetStatsAsync(user.WakaUser!.Username)) :
            users.Select(user => _wakaTime.GetStatsAsync(user.WakaUser!, timeRange.Value));

        var totalUserStats = await Task.WhenAll(statsTasks);

        var userStats = totalUserStats.OrderByDescending(stat => stat.data.total_seconds)
            .Skip(_maxUsersPerPage * page)
            .Take(_maxUsersPerPage)
            .ToArray();
        await UpdatePage(page, messageId, userStats.ToList(), totalUserStats.ToList(), maxPages, timeRange);
    }

    /// <summary>
    /// Update rank table message with relevant users.
    /// </summary>
    /// <param name="page">New page number for table.</param>
    /// <param name="messageId">Id of message to be edited</param>
    /// <param name="userStats">Users to be shown in table.</param>
    /// <param name="maxPages">Total number of existing pages.</param>
    public async Task UpdatePage(
        int page, ulong messageId,
        List<RootStat> userStats,
        List<RootStat> totalUserStats,
        int maxPages,
        TimeRange? timeRange
    )
    {
        var fields = new List<EmbedFieldBuilder>();

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Total programming time",
            Value = GetTotalTimeAsync(totalUserStats),
        });

        foreach (var user in userStats.Select((value, index) => new { value, index }))
        {
            string range = "\nIn " + user.value.data.range.Replace("_", " ");
            string languages = "\nTop languages: ";


            languages += user.value.data.languages.ToList().ConcatForEach(6, (token, last) =>
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
                Title = $"User Ranking {(timeRange != null ? "for " + timeRange.GetDisplay() : string.Empty)}",
                Color = Color.Purple,
                Fields = fields,
                Footer = new EmbedFooterBuilder() { Text = $"page {page + 1} of {maxPages}" },
            }.Build();

            msg.Components = new ComponentBuilder()
                /// operations: (page number), (message id), (timeRange)
                .WithButton("⏮️", $"rank-first:{page},{messageId},{timeRange}", disabled: page <= 0)
                .WithButton("◀️", $"rank-previous:{page},{messageId},{timeRange}", disabled: page <= 0)
                .WithButton("▶️", $"rank-next:{page},{messageId},{timeRange}", disabled: page >= maxPages - 1)
                .WithButton("⏭️", $"rank-last:{page},{messageId},{timeRange}", disabled: page >= maxPages - 1)
                .Build();
        });
    }

    /// <summary>
    /// Get total programming time for all registered users.
    /// </summary>
    private string GetTotalTimeAsync(List<RootStat> totalUserStats)
    {
        int totalSeconds = 0;
        totalUserStats.ForEach(stat => totalSeconds += (int)stat.data.total_seconds);
        return $"{totalSeconds / (60 * 60):N0} hrs {totalSeconds % (60 * 60) / 60:N0} mins";
    }

}