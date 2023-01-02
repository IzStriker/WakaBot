using Discord.Interactions;
using WakaBot.Core.Extensions;

namespace WakaBot.Core.WakaTimeAPI;

public enum TimeRange
{
    [ChoiceDisplay("Last 7 days")]
    [Value("last_7_days")]
    Last7Days,

    [ChoiceDisplay("Last 30 days")]
    [Value("last_30_days")]
    Last30Days,

    [ChoiceDisplay("Last 6 months")]
    [Value("last_6_months")]
    Last6Months,

    [ChoiceDisplay("Last year")]
    [Value("last_year")]
    LastYear,

    [ChoiceDisplay("All time")]
    [Value("all_time")]
    AllTime,

}