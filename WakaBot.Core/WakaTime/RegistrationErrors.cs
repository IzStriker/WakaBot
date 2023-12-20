using WakaBot.Core.Extensions;

namespace WakaBot.Core.WakaTimeAPI;

[Flags]
public enum RegistrationErrors
{

    None = 0,
    [RegistrationErrorMessage("Invalid username, ensure your WakaTime username is correct.", true)]
    UserNotFound = 1,
    [RegistrationErrorMessage("Stats not available, ensure `Display languages, editors, os, categories publicly` is enabled in your [profile](https://wakatime.com/settings/profile).")]
    StatsNotFound = 2,
    [RegistrationErrorMessage("Coding time not available, ensure `Display code time publicly` is enabled in your [profile](https://wakatime.com/settings/profile).")]
    TimeNotFound = 4,
}