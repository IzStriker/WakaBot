using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
namespace WakaBot;

/// <summary>
/// <c> WakaTime </c> Class handles interactions with WakaTime API.
/// </summary>
public class WakaTime
{
    const string BaseUrl = "https://wakatime.com/api/v1/";
    private readonly IMemoryCache _cache;

    [Flags]
    public enum RegistrationErrors
    {
        None = 0,
        UserNotFound = 1,
        StatsNotFound = 2,
        TimeNotFound = 4,
    }

    public WakaTime(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Check that a given WakaTime user is valid and has all the correct settings to user the system.
    /// </summary>
    /// <param name="username">Username of user who wishes to register.</param>
    /// <returns>Enum of bit flags of errors.</returns>
    public async Task<RegistrationErrors> ValidateRegistration(string username)
    {
        RegistrationErrors errors = RegistrationErrors.None;

        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{BaseUrl}/users/{username}/stats");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            errors |= RegistrationErrors.UserNotFound;
            return errors;
        }

        dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());

        if (!(data.data.ContainsKey("categories") && data.data.ContainsKey("editors") && data.data.ContainsKey("languages")))
        {
            errors |= RegistrationErrors.StatsNotFound;
        }

        if (!data.data.ContainsKey("total_seconds"))
        {
            errors |= RegistrationErrors.TimeNotFound;
        }

        return errors;
    }

    /// <summary>
    /// Returns WakaTime stats of specified users.
    /// </summary>
    /// <param name="username">User we wish to gets stats for.</param>
    /// <returns>Users stats.</returns>
    public async Task<dynamic> GetStatsAsync(string username)
    {
        using var httpClient = new HttpClient();
        var stats = await _cache.GetOrCreateAsync(username, async cacheEntry =>
        {
            dynamic entry = JObject.Parse(
                await httpClient.GetStringAsync($"{BaseUrl}/users/{username}/stats"));

            if (Convert.ToBoolean(entry.data.is_up_to_date))
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = DateTime.Parse("23:59").Subtract(DateTime.Now);
            }
            else
            {
                // Stats will be refreshed soon
                cacheEntry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30);
            }
            return entry;
        });

        return stats;
    }
}