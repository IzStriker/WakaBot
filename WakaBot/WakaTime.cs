using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using WakaBot.Data;

namespace WakaBot;

/// <summary>
/// <c> WakaTime </c> Class handles interactions with WakaTime API.
/// </summary>
public class WakaTime
{
    const string BaseUrl = "https://wakatime.com/api/v1/";
    private readonly IMemoryCache _cache;
    private readonly IDbContextFactory<WakaContext> _contextFactory;
    private readonly IConfiguration _config;

    [Flags]
    public enum RegistrationErrors
    {
        None = 0,
        UserNotFound = 1,
        StatsNotFound = 2,
        TimeNotFound = 4,
    }

    public WakaTime(IMemoryCache cache, IDbContextFactory<WakaContext> factory,
    IConfiguration config)
    {
        _cache = cache;
        _contextFactory = factory;
        _config = config;
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

            var timeTillExpiration = DateTime.Parse("23:59").Subtract(DateTime.Now);

            if (Convert.ToBoolean(entry.data.is_up_to_date))
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = timeTillExpiration;
            }
            else
            {
                // Stats will be refreshed soon
                cacheEntry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30);
            }

            if (_config.GetValue<bool>("alwaysCacheUsers"))
            {
                // Remove item from cache when expires, instead of when set is next called.
                cacheEntry.AddExpirationToken(new CancellationChangeToken(
                    new CancellationTokenSource(timeTillExpiration).Token));

                // Add back to cache when removed
                cacheEntry.RegisterPostEvictionCallback(PostEvictionCallBack);
            }

            return entry;
        });

        return stats;
    }

    /// <summary>
    /// Refreshes all users in the cache or adds them if they're not present.
    /// </summary>
    public async Task RefreshAllUsersAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var users = context.Users.ToList();
        var statsTasks = users.Select(user => GetStatsAsync(user.WakaName));
        dynamic[] userStats = await Task.WhenAll(statsTasks);

        Console.WriteLine("All users refreshed.");
    }

    /// <summary>
    /// Add Users removed back into cache.
    /// </summary>
    private void PostEvictionCallBack(object key, object value, EvictionReason resaon, object state)
    {
        Task.Run(async () => await GetStatsAsync((string)key)).Wait();
        Console.WriteLine($"{key} cache refreshed");
    }
}