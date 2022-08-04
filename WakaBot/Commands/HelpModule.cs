using Discord;
using Discord.Interactions;

namespace WakaBot.Commands;

public class HelpModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("wakahelp", "See information about how to use WakaBot")]
    public async Task GetHelp()
    {
        var fields = new List<EmbedFieldBuilder>()
        {
            new EmbedFieldBuilder()
            {
                Name = "Hello 👋",
                Value = "Hi, I'm WakaBot. I use data [WakaTime](https://wakatime.com) to " +
                "compare programming metrics of registered discord users."
            },
            new EmbedFieldBuilder()
            {
                Name = "Register",
                Value = "Visit [WakaTime](https://wakatime.com) website and create an account. " +
                " It's easiest to use your Github login.\n" +
                "Then, either follow the instructions of screen or goto [plugin page](https://wakatime.com/plugins). " +
                "WakaTime supports more than just IDEs and code editors - it also supports office software, database mangers, etc.\n\n" +
                "Now navigate to your [profile](https://wakatime.com/settings/profile)." +
                "Select `Display code time publicly` and `Display languages, editors, os, categories publicly.\n`" +
                "It's also a good idea to set `Display code time publicly` to `All Time`.\n\n"  +
                "Now you can register to WakaBot using the `/wakaregister` command and your programming" +
                "metrics will appear in 24 hours."

            },
            new EmbedFieldBuilder()
            {
                Name = "FAQ",
                Value = "[Why is my dashboard different to WakaBot?](https://wakatime.com/faq#leaderboard-vs-dashboard)\n\n"+
                "Visit [FAQ](https://wakatime.com/faq) more details about WakaTime."
            }
        };
        await RespondAsync("Help", embed: new EmbedBuilder()
        {
            Title = "Help Page",
            Fields = fields
        }.Build());
    }

}