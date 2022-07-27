using Discord;
using Discord.Interactions;
using WakaBot.Data;
using WakaBot.Graphs;
using WakaBot.Extensions;
using Newtonsoft.Json.Linq;
using System.Text;

namespace WakaBot.Commands;

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
    [SlashCommand("wakaping", "Recieve a pong")]
    public async Task Ping()
    {
        var embed = new EmbedBuilder()
        {
            Title = "wakapong!",
            Color = Discord.Color.LightGrey,
        };
        await RespondAsync(embed: embed.Build());
    }

    /// <summary>
    /// Rank all registered WakaBot users by programming time in decreasing order.
    /// </summary>
    /// <returns></returns>
    [SlashCommand("wakarank", "Get rank of programming time.")]
    public async Task Rank()
    {
        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Hold tight",
            Color = Color.Orange,
            Description = "This could take a second."
        }.Build());

        var users = _wakaContext.Users.Where(user => user.GuildId == Context.Guild.Id).ToList();

        var statsTasks = users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));

        dynamic[] userStats = await Task.WhenAll(statsTasks);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds).ToArray();

        var fields = new List<EmbedFieldBuilder>();

        List<DataPoint<double>> points = new List<DataPoint<double>>();
        double totalSeconds = 0;

        foreach (var user in userStats.Select((value, index) => new { index, value }))
        {
            string range = "\nIn " + Convert.ToString(user.value.data.range).Replace("_", " ");
            string languages = "\nTop languages: ";

            // Force C# to treat dynamic object as JArray instead of JObject
            JArray lanList = JArray.Parse(Convert.ToString(user.value.data.languages));

            languages += lanList.ConcatForEach(6, (token, last) =>
                $"{token.name} {token.percent}%" + (last ? "" : ", "));

            fields.Add(CreateEmbedField($"#{user.index + 1} - " + user.value.data.username,
                Convert.ToString(user.value.data.human_readable_total + range + languages)));

            // Store data point for pie chart
            points.Add(new DataPoint<double>(Convert.ToString(user.value.data.username), Convert.ToDouble(user.value.data.total_seconds)));

            totalSeconds += Convert.ToDouble(user.value.data.total_seconds);
        }

        fields = fields.Take(_maxUsersPerPage).ToList();
        fields.Insert(0, CreateEmbedField("Total programming time", $"{(int)totalSeconds / (60 * 60)} hrs {(int)(totalSeconds % (60 * 60)) / 60} mins"));

        byte[] image = _graphGenerator.GeneratePie(points.ToArray());
        int numPages = (int)Math.Ceiling(users.Count / (decimal)_maxUsersPerPage);

        var message = await Context.Channel.SendFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = "User Ranking",
            Color = Color.Purple,
            Fields = fields,
            Footer = new EmbedFooterBuilder() { Text = $"page 1 of {numPages}" }
        }.Build(),
        components: GetPaginationButtons(forwardDisabled: numPages <= 1));

        await message.ModifyAsync(msg => msg.Components = GetPaginationButtons(message.Id, numPages <= 1));

        // Remove hold tight message
        await DeleteOriginalResponseAsync();
    }

    /// <summary>
    /// Get profile and detailed information about specific WakaBot user.
    /// </summary>
    /// <param name="discordUser">Subject Discord user.</param>
    [SlashCommand("wakaprofile", "Get profile for specific WakaTime user")]
    public async Task Profile(IUser discordUser)
    {
        var fields = new List<EmbedFieldBuilder>();

        var user = _wakaContext.Users.FirstOrDefault(user => user.DiscordId == discordUser.Id
            && user.GuildId == Context.Guild.Id);

        if (user == null)
        {
            await RespondAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = $"{discordUser.Mention} isn't registered with WakaBot."
            }.Build());
            return;
        }

        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Just pulling the profile data.",
            Color = Color.Orange,
            Description = "Hang on"
        }.Build());

        var stats = await _wakaTime.GetStatsAsync(user.WakaName);

        fields.Add(CreateEmbedField("Programming time",
         $"{stats.data.human_readable_total} {stats.data.human_readable_range}"));

        fields.Add(CreateEmbedField("Daily average", $"{stats.data.human_readable_daily_average}"));

        // Force C# to treat dynamic object as JArray instead of JObject
        JArray lanList = JArray.Parse(Convert.ToString(stats.data.languages));
        List<DataPoint<double>> points = new List<DataPoint<double>>();

        // Generate data points for pie chart
        foreach (dynamic lan in lanList)
        {
            points.Add(new DataPoint<double>(Convert.ToString(lan.name), Convert.ToDouble(lan.percent)));
        }

        fields.Add(CreateEmbedField("Languages",
             lanList.AsEnumerable<dynamic>().Select(lan => $"{lan.name} {lan.percent}%").ToList()));

        // Force C# to treat dynamic object as JArray instead of JObject
        JArray editorList = JArray.Parse(Convert.ToString(stats.data.editors));

        fields.Add(CreateEmbedField("Editors",
            editorList.AsEnumerable<dynamic>().Select(editor => $"{editor.name} {editor.percent}%").ToList()));

        // Force C# to treat dynamic object as JArray instead of JObject
        JArray osList = JArray.Parse(Convert.ToString(stats.data.operating_systems));

        fields.Add(CreateEmbedField("Operating Systems",
            osList.AsEnumerable<dynamic>().Select(lan => $"{lan.name} {lan.percent}%").ToList()));

        byte[] image = _graphGenerator.GeneratePie(points.ToArray());

        await DeleteOriginalResponseAsync();
        await Context.Channel.SendFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = discordUser.Username,
            Color = Color.Purple,
            Fields = fields
        }.Build());
    }

    [SlashCommand("wakastats", "Get programming stats for whole server")]
    public async Task Stats()
    {
        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Hold the line",
            Color = Color.Orange,
            Description = "Complex processing happening here!"
        }.Build());

        var users = _wakaContext.Users.Where(user => user.GuildId == Context.Guild.Id);
        var statsTasks = users.Select(user => _wakaTime.GetStatsAsync(user.WakaName));
        dynamic[] userStats = await Task.WhenAll(statsTasks);

        Dictionary<string, float> languages = new Dictionary<string, float>();

        // Calculate top languages in for each user in server
        foreach (var user in userStats)
        {
            // Force C# to treat as JArray
            JArray langList = JArray.Parse(Convert.ToString(user.data.languages));

            foreach (dynamic lang in langList)
            {
                string langName = Convert.ToString(lang.name);
                float totalSeconds = Convert.ToSingle(lang.total_seconds);
                float originalValue;

                if (languages.TryGetValue(langName, out originalValue))
                {
                    languages[langName] = originalValue + totalSeconds;
                }
                else
                {
                    languages[langName] = totalSeconds;
                }
            }
        }

        var topLanguages = languages.OrderByDescending(lang => lang.Value).Select(lang => lang.Key).Take(6).ToArray();
        List<DataPoint<float[]>> userTopLangs = new List<DataPoint<float[]>>();

        // Calculate each users programming time with top languages
        foreach (var user in userStats)
        {
            float[] languageTotals = new float[6];
            // Force C# to treat as JArray
            var langList = JArray.Parse(Convert.ToString(user.data.languages));

            // Get programming time for each top language
            for (int i = 0; i < languageTotals.Count(); i++)
            {
                // Search corresponding language in languages
                foreach (dynamic lang in langList)
                {

                    if (lang.name == topLanguages[i])
                    {
                        // Convert hours to seconds
                        languageTotals[i] = Convert.ToSingle(lang.total_seconds) / 3600;
                        break;
                    }

                    // If user hasn't used language, value defaults to zero
                }
            }
            userTopLangs.Add(new DataPoint<float[]>(Convert.ToString(user.data.username), languageTotals));
        }

        // Create fields of detailed top language stats
        var fields = new List<EmbedFieldBuilder>();
        for (int i = 0; i < topLanguages.Count(); i++)
        {
            fields.Add(CreateEmbedField($"#{i + 1} {topLanguages[i]}", "value"));
            // Value should be list of user who used language and percentage of their contribution to language
            // e.g. user1 25%, user2 15% ... user(n) n% sorted
        }

        byte[] image = _graphGenerator.GenerateBar(topLanguages, userTopLangs.ToArray());
        await DeleteOriginalResponseAsync();
        await Context.Channel.SendFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = "Top Languages",
            Fields = fields,
            ImageUrl = "attachment://graph.png"
        }.Build());
    }

    /// <summary>
    /// Create pagination buttons for component. 
    /// </summary>
    /// <param name="messageId">Id of message for which buttons are applied to.</param>
    /// <param name="forwardDisabled">disables next and last button, should be set if only one page.</param>
    /// <returns>Returns generated buttons.</returns>/
    private MessageComponent GetPaginationButtons(ulong messageId = 0, bool forwardDisabled = false)
    {
        return new ComponentBuilder()
        /// operations: (page number), (message id)
        .WithButton("⏮️", $"rank-first:0,{messageId}", disabled: true)
        .WithButton("◀️", $"rank-previous:0,{messageId}", disabled: true)
        .WithButton("▶️", $"rank-next:0,{messageId}", disabled: forwardDisabled)
        .WithButton("⏭️", $"rank-last:0,{messageId}", disabled: forwardDisabled)
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
    private EmbedFieldBuilder CreateEmbedField(string name, string value)
    {
        // value isn't empty
        if (value.Length == 0)
        {
            value = "No data";
        }

        // value is too long
        if (value.Length > 1024)
        {
            value = value.Substring(0, 1024);
        }

        return new EmbedFieldBuilder()
        {
            Name = name,
            Value = value
        };
    }

}