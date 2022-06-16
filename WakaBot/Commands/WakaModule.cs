using Discord;
using Discord.Interactions;
using WakaBot.Data;
using WakaBot.Graphs;
using WakaBot.Extensions;
using Newtonsoft.Json.Linq;

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
    public WakaModule(GraphGenerator graphGenerator, WakaContext wakaContext, WakaTime wakaTime)
    {
        _graphGenerator = graphGenerator;
        _wakaContext = wakaContext;
        _wakaTime = wakaTime;
        _maxUsersPerPage = 4;
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

        var users = _wakaContext.Users.ToList();

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

            fields.Add(new EmbedFieldBuilder()
            {
                Name = $"#{user.index + 1} - " + user.value.data.username,
                Value = user.value.data.human_readable_total + range + languages
            });

            // Store data point for pie chart
            points.Add(new DataPoint<double>(Convert.ToString(user.value.data.username), Convert.ToDouble(user.value.data.total_seconds)));

            totalSeconds += Convert.ToDouble(user.value.data.total_seconds);
        }

        fields.Insert(0, new EmbedFieldBuilder()
        {
            Name = "Total programming time",
            Value = $"{(int)totalSeconds / (60 * 60)} hrs {(int)(totalSeconds % (60 * 60)) / 60} mins"
        });


        byte[] image = _graphGenerator.GeneratePie(points.ToArray());
        int numPages = (int)Math.Ceiling(users.Count / (decimal)_maxUsersPerPage);

        var message = await Context.Channel.SendFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = "User Ranking",
            Color = Color.Purple,
            Fields = fields.Take(_maxUsersPerPage).ToList(),
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

        var user = _wakaContext.Users.FirstOrDefault(user => user.DiscordId == discordUser.Id);

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

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Programming time",
            Value = $"{stats.data.human_readable_total} {stats.data.human_readable_range}"
        });

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Daily average",
            Value = stats.data.human_readable_daily_average
        });

        // Force C# to treat dynamic object as JArray instead of JObject
        JArray lanList = JArray.Parse(Convert.ToString(stats.data.languages));
        List<DataPoint<double>> points = new List<DataPoint<double>>();

        var languages = lanList.ConcatForEach((token, last) =>
        {
            points.Add(new DataPoint<double>(Convert.ToString(token.name), Convert.ToDouble(token.total_seconds)));
            return $"{token.name} {token.percent}%" + (last ? "" : ", ");
        });

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Languages",
            Value = languages
        });

        // Force C# to treat dynamic object as JArray instead of JObject
        JArray editorList = JArray.Parse(Convert.ToString(stats.data.editors));

        var editors = editorList.ConcatForEach((token, last) =>
            $"{token.name} {token.percent}%" + (last ? "" : ", "));

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Editors",
            Value = editors
        });


        // Force C# to treat dynamic object as JArray instead of JObject
        JArray osList = JArray.Parse(Convert.ToString(stats.data.operating_systems));

        var os = osList.ConcatForEach((token, last) =>
            $"{token.name} {token.percent}%" + (last ? "" : ", "));


        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Operating Systems",
            Value = os
        });

        byte[] image = _graphGenerator.GeneratePie(points.ToArray());

        await DeleteOriginalResponseAsync();
        await Context.Channel.SendFileAsync(new MemoryStream(image), "graph.png", embed: new EmbedBuilder()
        {
            Title = discordUser.Username,
            Color = Color.Purple,
            Fields = fields
        }.Build());
    }

    private MessageComponent GetPaginationButtons(ulong messageId = 0, bool forwardDisabled = false)
    {
        return new ComponentBuilder()
        /// operations: (page number), (message id)
        .WithButton("⏮️", $"first:0,{messageId}", disabled: true)
        .WithButton("◀️", $"previous:0,{messageId}", disabled: true)
        .WithButton("▶️", $"next:0,{messageId}", disabled: forwardDisabled)
        .WithButton("⏭️", $"last:0,{messageId}", disabled: forwardDisabled)
        .Build();
    }


}