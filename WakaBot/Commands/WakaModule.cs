using Discord;
using Discord.Interactions;
using WakaBot.Data;
using WakaBot.Models;
using WakaBot.Graphs;
using Newtonsoft.Json.Linq;

namespace WakaBot.Commands;

public class WakaModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly GraphGenerator _graphGenerator;

    public WakaModule(GraphGenerator graphGenerator)
    {
        _graphGenerator = graphGenerator;
    }

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

    [SlashCommand("wakaregister", "Register new server member to WakaTime Service")]
    public async Task RegisterUser(IUser discordUser, String wakaUser)
    {
        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Just checking your profile.",
            Color = Color.Orange,
            Description = "Should only take a second."
        }.Build());

        var errors = await WakaTime.ValidateRegistration(wakaUser);

        if (errors.HasFlag(WakaTime.RegistrationErrors.UserNotFound))
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = $"Invalid username **{wakaUser}**, ensure your username is correct."
            }
            .Build());
            await DeleteOriginalResponseAsync();
            return;
        }

        string description = string.Empty;

        if (errors.HasFlag(WakaTime.RegistrationErrors.StatsNotFound))
        {
            description += "Stats not available, ensure `Display languages, editors, os, categories publicly    ` is enabled in profile.\n\n";
        }

        if (errors.HasFlag(WakaTime.RegistrationErrors.TimeNotFound))
        {
            description += "Coding time not available, ensure `Display code time publicly` is enabled in profile.";
        }

        if (!errors.Equals(WakaTime.RegistrationErrors.None))
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "Error",
                Color = Color.Red,
                Description = description
            }.Build());
            await DeleteOriginalResponseAsync();
            return;
        }

        using WakaContext context = new();

        if (context.Users.Where(x => x.DiscordId == discordUser.Id || x.WakaName == wakaUser).Count() > 0)
        {
            await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
            {
                Title = "User already registered",
                Color = Color.Red,
                Description = $"User {discordUser.Mention} **{wakaUser}**, already registered"
            }.Build());
            await DeleteOriginalResponseAsync();
            return;
        }

        context.Add(new User() { DiscordId = discordUser.Id, WakaName = wakaUser });
        context.SaveChanges();

        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder()
        {
            Title = "User registered",
            Color = Color.Green,
            Description = $"User {discordUser.Mention} register as {wakaUser}"
        }.Build());
        await DeleteOriginalResponseAsync();
    }


    [SlashCommand("wakausers", "Get list of registered users")]
    public async Task Users()
    {
        using WakaContext context = new();

        var fields = new List<EmbedFieldBuilder>();
        var users = context.Users.ToList();
        await Context.Guild.DownloadUsersAsync();
        foreach (User user in users)
        {
            string disUser;
            var data = Context.Guild.GetUser(user.DiscordId);
            if (Context.Guild.GetUser(user.DiscordId).Nickname != null)
                disUser = Context.Guild.GetUser(user.DiscordId).Nickname;
            else
                disUser = Context.Guild.GetUser(user.DiscordId).Username;

            fields.Add(new EmbedFieldBuilder()
            {
                Name = disUser,
                Value = $"[{user.WakaName}](https://wakatime.com/@{user.WakaName})"
            });
        }

        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Registered users",
            Color = Color.Purple,
            Fields = fields
        }.Build());

    }

    [SlashCommand("wakarank", "Get rank of programming time.")]
    public async Task Rank()
    {
        await RespondAsync(embed: new EmbedBuilder()
        {
            Title = "Hold tight",
            Color = Color.Orange,
            Description = "This could take a second."
        }.Build());

        using WakaContext context = new();

        var users = context.Users.ToList();
        List<Task<dynamic>> stats = new List<Task<dynamic>>();

        foreach (var user in users)
        {
            stats.Add(WakaTime.GetStatsAsync(user.WakaName));
        }
        dynamic[] userStats = await Task.WhenAll(stats);

        userStats = userStats.OrderByDescending(stat => stat.data.total_seconds).ToArray();

        var fields = new List<EmbedFieldBuilder>();

        List<DataPoint<double>> points = new List<DataPoint<double>>();

        foreach (var stat in userStats)
        {
            string range = "\nIn " + Convert.ToString(stat.data.range).Replace("_", " ");
            string languages = "\nTop languages: ";

            // Force C# to treat dynamic object as JArray instead of JObject
            var lanList = JArray.Parse(Convert.ToString(stat.data.languages));

            // Print top 6 languages
            for (int i = 0; i < lanList.Count && i < 6; i++)
            {
                languages += $"{lanList[i].name} {lanList[i].percent}%";
                if (i < 5 && i < lanList.Count - 1) languages += ", ";
            }

            fields.Add(new EmbedFieldBuilder()
            {
                Name = stat.data.username,
                Value = stat.data.human_readable_total + range + languages
            });

            // Store data point for pie chart
            points.Add(new DataPoint<double>(Convert.ToString(stat.data.username), Convert.ToDouble(stat.data.total_seconds)));
        }

        using MemoryStream graph = new MemoryStream();
        _graphGenerator.GeneratePie(points.ToArray(), graph);

        await Context.Channel.SendFileAsync(graph, "graph.png", embed: new EmbedBuilder()
        {
            Title = "User Ranking",
            Color = Color.Purple,
            Fields = fields,
        }.Build());

        // Remove hold tight message
        await DeleteOriginalResponseAsync();

    }

    [SlashCommand("wakaprofile", "Get profile for specific WakaTime user")]
    public async Task Profile(IUser discordUser)
    {
        var fields = new List<EmbedFieldBuilder>();
        string languages = string.Empty;
        string editors = string.Empty;
        string os = string.Empty;
        var context = new WakaContext();

        var user = context.Users.FirstOrDefault(user => user.DiscordId == discordUser.Id);

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

        var stats = await WakaTime.GetStatsAsync(user.WakaName);

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
        var lanList = JArray.Parse(Convert.ToString(stats.data.languages));
        List<DataPoint<double>> points = new List<DataPoint<double>>();

        for (int i = 0; i < lanList.Count; i++)
        {
            points.Add(new DataPoint<double>(Convert.ToString(lanList[i].name), Convert.ToDouble(lanList[i].total_seconds)));
            languages += $"{lanList[i].name} {lanList[i].percent}%";
            if (i < lanList.Count - 1) languages += ", ";
        }

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Languages",
            Value = languages
        });

        // Force C# to treat dynamic object as JArray instead of JObject
        var editorList = JArray.Parse(Convert.ToString(stats.data.editors));

        for (int i = 0; i < editorList.Count; i++)
        {
            editors += $"{editorList[i].name} {editorList[i].percent}%";
            if (i < editorList.Count - 1) editors += ", ";
        }

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Editors",
            Value = editors
        });


        // Force C# to treat dynamic object as JArray instead of JObject
        var osList = JArray.Parse(Convert.ToString(stats.data.editors));

        for (int i = 0; i < osList.Count; i++)
        {
            os += $"{osList[i].name} {osList[i].percent}%";
            if (i < osList.Count - 1) os += ", ";
        }

        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Editors",
            Value = editors
        });

        using MemoryStream graph = new MemoryStream();
        _graphGenerator.GeneratePie(points.ToArray(), graph);

        await DeleteOriginalResponseAsync();
        await Context.Channel.SendFileAsync(graph, "graph.png", embed: new EmbedBuilder()
        {
            Title = discordUser.Username,
            Color = Color.Purple,
            Fields = fields,
        }.Build());
    }
}