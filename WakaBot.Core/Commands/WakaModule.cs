using Discord;
using Discord.Interactions;
using WakaBot.Core.Data;
using WakaBot.Core.Graphs;
using WakaBot.Core.WakaTimeAPI;
using WakaBot.Core.WakaTimeAPI.Stats;
using WakaBot.Core.Extensions;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace WakaBot.Core.Commands;

/// <summary>
/// Specifies what a WakaBot command should do.
/// </summary>
public class WakaModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly GraphGenerator _graphGenerator;
    private readonly WakaContext _wakaContext;
    private readonly WakaTime _wakaTime;
    private readonly int _maxUsersPerPage;

    /// <summary>
    /// Create an instance of WakaModule.
    /// </summary>
    /// <param name="graphGenerator">Instance of graph generator class</param>
    /// <param name="wakaContext">Instance of database context.</param>/
    public WakaModule(
        GraphGenerator graphGenerator,
        WakaContext wakaContext,
        WakaTime wakaTime,
        IConfiguration config)
    {
        _graphGenerator = graphGenerator;
        _wakaContext = wakaContext;
        _wakaTime = wakaTime;
        _maxUsersPerPage = config["maxUsersPerPage"] != null
            ? config.GetValue<int>("maxUsersPerPage") : 3;
    }
    /// <summary>
    /// Checks that bot can respond to messages.
    /// </summary>
    [SlashCommand("ping", "Recieve a pong")]
    public async Task Ping()
    {
        var embed = new EmbedBuilder()
        {
            Title = "pong!",
            Color = Discord.Color.LightGrey,
        };
        await RespondAsync(embed: embed.Build());
    }

    /// <summary>
    /// Rank all registered WakaBot users by programming time in decreasing order.
    /// </summary>
    /// <returns></returns>
    [SlashCommand("rank", "Get rank of programming time.")]
    public async Task Rank(TimeRange? timeRange = null)
    {
        await DeferAsync();

        var users = _wakaContext.DiscordGuilds.Include(x => x.Users).ThenInclude(x => x.WakaUser)
            .FirstOrDefault(guild => guild.Id == Context.Guild.Id)?.Users;

        if (users == null || users.Count == 0)
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Embed = new EmbedBuilder()
                {
                    Title = "Error",
                    Description = "No users registered.",
                    Color = Color.Red
                }.Build()
            );
            return;
        }

        IEnumerable<Task<RootStat>> statsTasks;
        if (timeRange == null)
        {
            statsTasks = users.Select(user => _wakaTime.GetStatsAsync(user.WakaUser!.Username));
        }
        else
        {
            users = users.Where(u => u.WakaUser != null && u.WakaUser.usingOAuth).ToList();
            statsTasks = users.Select(user => _wakaTime.GetStatsAsync(user.WakaUser!, timeRange.Value));
            if (users.Count() == 0)
            {
                await ModifyOriginalResponseAsync(msg =>
                    msg.Embed = new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = "No OAuth users registered.",
                        Color = Color.Red
                    }.Build()
                );
                return;
            }
        }

        RootStat[] userStats = await Task.WhenAll(statsTasks);
        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds).ToArray();

        var fields = new List<EmbedFieldBuilder>();
        List<DataPoint<double>> points = new List<DataPoint<double>>();
        double totalSeconds = 0;

        foreach (var user in userStats.Select((value, index) => new { index, value }))
        {
            string range = "\nIn " + user.value.data.range.Replace("_", " ");
            string languages = "\nTop languages: ";

            languages += user.value.data.languages.ToList().ConcatForEach(6, (token, last) =>
                $"{token.name} {token.percent}%" + (last ? "" : ", "));

            fields.Add(CreateEmbedField($"#{user.index + 1} - " + user.value.data.username,
                user.value.data.human_readable_total + range + languages));

            // Store data point for pie chart
            points.Add(new DataPoint<double>(user.value.data.username, user.value.data.total_seconds));

            totalSeconds += user.value.data.total_seconds;
        }

        fields = fields.Take(_maxUsersPerPage).ToList();
        fields.Insert(0, CreateEmbedField("Total programming time", $"{totalSeconds / (60 * 60):N0} hrs {totalSeconds % (60 * 60) / 60:N0} mins"));

        byte[] image = _graphGenerator.GeneratePie(points.ToArray());
        int numPages = (int)Math.Ceiling(users.Count / (decimal)_maxUsersPerPage);

        var message = await FollowupWithFileAsync(new MemoryStream(image), "graph.png", components: GetPaginationButtons(timeRange: timeRange),
            embed: new EmbedBuilder()
            {
                Title = $"User Ranking {(timeRange != null ? "for " + timeRange.GetDisplay() : string.Empty)}",
                Color = Color.Purple,
                Fields = fields,
                Footer = new EmbedFooterBuilder() { Text = $"page 1 of {numPages}" }
            }.Build()
        );

        await ModifyOriginalResponseAsync(msg => msg.Components = GetPaginationButtons(message.Id, numPages <= 1, timeRange));
    }

    /// <summary>
    /// Get profile and detailed information about specific WakaBot user.
    /// </summary>
    /// <param name="discordUser">Subject Discord user.</param>
    [SlashCommand("profile", "Get profile for specific WakaTime user")]
    public async Task Profile(IUser discordUser)
    {
        await DeferAsync();

        var fields = new List<EmbedFieldBuilder>();

        var user = _wakaContext.DiscordGuilds.Include(x => x.Users).ThenInclude(x => x.WakaUser)
            .FirstOrDefault(guild => guild.Id == Context.Guild.Id)?
            .Users.FirstOrDefault(x => x.Id == discordUser.Id);

        if (user == null)
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Embed = new EmbedBuilder()
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = $"{discordUser.Mention} isn't registered with WakaBot."
                }.Build()
            );
            return;
        }

        var stats = await _wakaTime.GetStatsAsync(user.WakaUser!.Username);

        fields.Add(CreateEmbedField("Programming time",
         $"{stats.data.human_readable_total} {stats.data.human_readable_range}"));

        fields.Add(CreateEmbedField("Daily average", $"{stats.data.human_readable_daily_average}"));

        List<DataPoint<double>> points = new List<DataPoint<double>>();

        // Generate data points for pie chart
        foreach (var lan in stats.data.languages)
        {
            points.Add(new DataPoint<double>(lan.name, lan.percent));
        }

        fields.Add(CreateEmbedField("Languages",
             stats.data.languages.Select(lan => $"{lan.name} {lan.percent}%").ToList())
        );

        fields.Add(CreateEmbedField("Editors",
            stats.data.editors.Select(editor => $"{editor.name} {editor.percent}%").ToList())
        );

        fields.Add(CreateEmbedField("Operating Systems",
            stats.data.operating_systems.Select(lan => $"{lan.name} {lan.percent}%").ToList())
        );

        byte[] image = _graphGenerator.GeneratePie(points.ToArray());

        await FollowupWithFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = discordUser.Username,
            Color = Color.Purple,
            Fields = fields
        }.Build());
    }

    [SlashCommand("toplangs", "Get programming stats for whole server")]
    public async Task Stats()
    {
        await DeferAsync();

        var users = _wakaContext.DiscordGuilds.Include(x => x.Users).ThenInclude(x => x.WakaUser)
            .FirstOrDefault(guild => guild.Id == Context.Guild.Id)?.Users;

        if (users == null || users.Count() == 0)
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Embed = new EmbedBuilder()
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = "No users are registered with WakaBot."
                }.Build());
            return;
        }

        var statsTasks = users.Select(user => _wakaTime.GetStatsAsync(user.WakaUser!.Username));
        var userStats = await Task.WhenAll(statsTasks);

        Dictionary<string, float> languages = new Dictionary<string, float>();

        // Calculate top languages in for each user in server
        foreach (var user in userStats)
        {
            foreach (var lang in user.data.languages)
            {
                float originalValue;

                if (languages.TryGetValue(lang.name, out originalValue))
                {
                    languages[lang.name] = originalValue + lang.total_seconds;
                }
                else
                {
                    languages[lang.name] = lang.total_seconds;
                }
            }
        }

        var topLanguages = languages.OrderByDescending(lang => lang.Value).Select(lang => lang.Key).Take(6).ToArray();
        List<DataPoint<float[]>> userTopLangs = new List<DataPoint<float[]>>();

        // Calculate each users programming time with top languages
        foreach (var user in userStats)
        {
            float[] languageTotals = new float[6];

            // Get programming time for each top language
            for (int i = 0; i < languageTotals.Count(); i++)
            {
                // Search corresponding language in languages
                foreach (var lang in user.data.languages)
                {
                    // If user hasn't used language, value defaults to zero
                    if (lang.name == topLanguages[i])
                    {
                        // Convert seconds to hours
                        languageTotals[i] = lang.total_seconds / 3600;
                        break;
                    }
                }
            }
            userTopLangs.Add(new DataPoint<float[]>(user.data.username, languageTotals));
        }

        // Create fields of detailed top language stats
        var fields = new List<EmbedFieldBuilder>();
        for (int i = 0; i < topLanguages.Count(); i++)
        {
            List<string> userPercentages = new List<string>();

            // for each user, get programming time for current language
            userTopLangs.OrderByDescending(ele => ele.value[i]).ToList().ForEach(point =>
            {
                var hours = (point.value[i]).ToString("0.##");
                userPercentages.Add($"{point.label} {hours}h ");
            });

            fields.Add(CreateEmbedField($"#{i + 1} {topLanguages[i]}", userPercentages));
        }

        byte[] image = _graphGenerator.GenerateBar(topLanguages, userTopLangs.ToArray());

        await FollowupWithFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = "Top Languages",
            Fields = fields,
            ImageUrl = "attachment://graph.png"
        }.Build());
    }

    [SlashCommand("languagestats", "Get stats about specific programming language.")]
    public async Task LanguageStasts(string language)
    {
        await DeferAsync();

        var users = _wakaContext.DiscordGuilds.Include(x => x.Users).ThenInclude(x => x.WakaUser)
            .FirstOrDefault(guild => guild.Id == Context.Guild.Id)?.Users;

        if (users == null || users.Count() == 0)
        {
            await ModifyOriginalResponseAsync(msg =>
                msg.Embed = new EmbedBuilder()
                {
                    Title = "Error",
                    Color = Color.Red,
                    Description = "No users are registered with WakaBot."
                }.Build());
            return;

        }
        var statsTasks = users.Select(user => _wakaTime.GetStatsAsync(user.WakaUser!.Username));
        var userStats = await Task.WhenAll(statsTasks);

        var langStats = new List<DataPoint<double>>();
        foreach (var user in userStats)
        {
            float seconds = 0;
            var lang = user.data.languages.FirstOrDefault(lang => lang.name.ToLower() == language.ToLower());
            if (lang != null)
            {
                seconds = lang.total_seconds;
            }
            langStats.Add(new DataPoint<double>(user.data.username, seconds));
        }

        if (langStats.Sum(stat => stat.value) <= 0)
        {
            await FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = "Language Not Found!"
            }.Build());
            return;
        }

        var image = _graphGenerator.GeneratePie(langStats.ToArray());
        await FollowupWithFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = $"{language} stats",
            ImageUrl = "attachment://graph.png",
            Color = Color.Purple,
        }.Build());

    }

    [SlashCommand("project", "Get project stats")]
    public async Task GetProjectStats(IUser discordUser, TimeRange timeRange = TimeRange.AllTime)
    {
        await DeferAsync();

        var wakaUser = _wakaContext.WakaUsers.FirstOrDefault(x => x.DiscordUser!.Id == discordUser.Id);

        if (wakaUser == null)
        {
            await FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = "User is not registered with WakaBot."
            }.Build());
            return;
        }

        // Projects are stats are only returned from the api when using oauth
        if (!wakaUser?.usingOAuth ?? false)
        {
            await FollowupAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = "You must be registered using OAuth to use this feature."
            }.Build());
            return;
        }

        var stats = await _wakaTime.GetStatsAsync(wakaUser!, timeRange);
        var fields = new List<EmbedFieldBuilder>();

        var projects = stats.data.projects.OrderByDescending(project => project.total_seconds).Take(15);
        var points = projects.Select(project => new DataPoint<double>(project.name, project.total_seconds)).ToArray();

        var graph = _graphGenerator.GeneratePie(points);
        for (int i = 0; i < projects.Count(); i++)
        {
            var project = projects.ElementAt(i);
            fields.Add(CreateEmbedField($"#{i + 1} {project.name}", project.text, true));
        }

        await FollowupWithFileAsync(new MemoryStream(graph), "graph.png", embed: new EmbedBuilder()
        {
            Title = $"Project Stats - {discordUser.Username}",
            Color = Color.Purple,
            Description = timeRange.GetDisplay(),
            Fields = fields,
        }.Build());
    }

    /// <summary>
    /// Create pagination buttons for component. 
    /// </summary>
    /// <param name="messageId">Id of message for which buttons are applied to.</param>
    /// <param name="forwardDisabled">disables next and last button, should be set if only one page.</param>
    /// <returns>Returns generated buttons.</returns>/
    private MessageComponent GetPaginationButtons(
        ulong messageId = 0,
        bool forwardDisabled = false,
        TimeRange? timeRange = null
    )
    {

        string? rawTimeRange = timeRange is null ? "none" : timeRange.ToString();
        return new ComponentBuilder()
        /// operations: (page number), (message id), (timeRange)
        .WithButton("⏮️", $"rank-first:0,{messageId},{rawTimeRange}", disabled: true)
        .WithButton("◀️", $"rank-previous:0,{messageId},{rawTimeRange}", disabled: true)
        .WithButton("▶️", $"rank-next:0,{messageId},{rawTimeRange}", disabled: forwardDisabled)
        .WithButton("⏭️", $"rank-last:0,{messageId},{rawTimeRange}", disabled: forwardDisabled)
        .Build();
    }

    /// <summary>
    /// Creates a safe Embed Field Builder 
    /// </summary>
    /// <param name="name">Name of the embedded filed</param>
    /// <param name="values">Content of value from list</param>
    /// <returns>Safe embedded field builder</returns>
    private EmbedFieldBuilder CreateEmbedField(string name, List<string> values)
    {
        StringBuilder sb = new StringBuilder();

        // Check sb length is less than or equal to 1024
        for (int i = 0; i < values.Count; i++)
        {
            if (sb.Length + values[i].ToString().Length > EmbedFieldBuilder.MaxFieldValueLength)
            {
                break;
            }

            sb.Append(values[i].ToString());

            // Only append comma if not last and next won't go over max length
            if (i != values.Count - 1 &&
             sb.Length + 1 + values[i + 1].ToString().Length
             <= EmbedFieldBuilder.MaxFieldValueLength)
            {
                sb.Append(", ");
            }
        }

        if (sb.Length == 0)
        {
            sb.Append("No data");
        }

        return new EmbedFieldBuilder()
        {
            Name = name,
            Value = sb.ToString()
        };
    }

    /// <summary>
    /// Creates a safe Embed Field Builder 
    /// </summary>
    /// <param name="name">Name of the embedded filed</param>
    /// <param name="values">Content of value from string</param>
    /// <returns>Safe embedded field builder</returns>
    private EmbedFieldBuilder CreateEmbedField(string name, string value, bool inLine = false)
    {
        // value isn't empty
        if (value.Length == 0)
        {
            value = "No data";
        }

        // value is too long
        if (value.Length > EmbedFieldBuilder.MaxFieldValueLength)
        {
            value = value.Substring(0, EmbedFieldBuilder.MaxFieldValueLength);
        }

        return new EmbedFieldBuilder()
        {
            Name = name,
            Value = value,
            IsInline = inLine
        };
    }

}