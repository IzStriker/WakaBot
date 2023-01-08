using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using WakaBot.Core.Data;
using WakaBot.Core.WakaTimeAPI.Stats;
using WakaBot.Core.OAuth2;
using WakaBot.Core.Models;
using WakaBot.Core.Extensions;
using System.Net.Http.Headers;

namespace WakaBot.Core.WakaTimeAPI;

/// <summary>
/// <c> WakaTime </c> Class handles interactions with WakaTime API.
/// </summary>
public class WakaTime : OAuth2Client
{
    private readonly IMemoryCache _cache;
    private readonly IDbContextFactory<WakaContext> _contextFactory;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly HttpClient _client;

    [Flags]
    public enum RegistrationErrors
    {
        None = 0,
        UserNotFound = 1,
        StatsNotFound = 2,
        TimeNotFound = 4,
    }

    public WakaTime(
        IMemoryCache cache,
        IDbContextFactory<WakaContext> factory,
        IConfiguration config,
        ILogger<WakaTime> logger,
        HttpClient client
    ) : base(config)
    {
        _cache = cache;
        _contextFactory = factory;
        _config = config;
        _logger = logger;
        _client = client;
    }

    /// <summary>
    /// Check that a given WakaTime user is valid and has all the correct settings to user the system.
    /// </summary>
    /// <param name="username">Username of user who wishes to register.</param>
    /// <returns>Enum of bit flags of errors.</returns>
    public async Task<(string?, RegistrationErrors)> ValidateRegistration(string username)
    {
        RegistrationErrors errors = RegistrationErrors.None;

        var response = await _client.GetAsync($"users/{username}/stats");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            errors |= RegistrationErrors.UserNotFound;
            return (null, errors);
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

        return (data.data.user_id, errors);
    }

    /// <summary>
    /// Returns WakaTime stats of specified users.
    /// </summary>
    /// <param name="username">User we wish to gets stats for.</param>
    /// <returns>Users stats.</returns>
    public async Task<RootStat> GetStatsAsync(string username)
    {
        var stats = await _cache.GetOrCreateAsync(username, async cacheEntry =>
        {
            var response = await _client.GetAsync($"users/{username}/stats");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Request failed");
                _logger.LogError(await response.Content.ReadAsStringAsync());
            }

            RootStat entry;
            try
            {
                entry = JsonConvert.DeserializeObject<RootStat>(await response.Content.ReadAsStringAsync())!;
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                return null;
            }

            // 3:00 AM tomorrow morning
            var timeTillExpiration = DateTime.Parse("00:00").AddDays(1)
                .AddHours(3).Subtract(DateTime.Now);

            if (entry.data.is_up_to_date)
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

    public async Task<RootStat> GetStatsAsync(WakaUser user, TimeRange range)
    {
        if (!user.usingOAuth)
        {
            throw new ArgumentException("User is not registered with OAuth2");
        }

        if (user.AccessToken == null || user.RefreshToken == null)
        {
            throw new Exception($"User {user.Id} has invalid authentication tokens");
        }

        var response = await _client.GetAsync($"users/current/stats/{range.GetValue()}", user.AccessToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError($"Invalid Access Token, {user.Username}");
            _logger.LogError(await response.Content.ReadAsStringAsync());

            // Refresh token
            if (user.RefreshToken == null)
            {
                _logger.LogError($"Refresh token is null for {user.Username}");
                throw new Exception($"Refresh token is null for {user.Username}");
            }

            var newToken = await RefreshTokenAsync(user.RefreshToken);

            // store new token in db
            using (var context = await _contextFactory.CreateDbContextAsync())
            {
                var dbUser = context.WakaUsers.FirstOrDefault(u => u.Id == user.Id);
                if (dbUser == null)
                {
                    _logger.LogError($"User {user.Username} not found in database");
                    throw new Exception($"User {user.Username} not found in database");
                }

                dbUser.AccessToken = newToken.AccessToken;
                dbUser.RefreshToken = newToken.RefreshToken;
                dbUser.ExpiresAt = newToken.ExpiresAt;
                context.SaveChanges();
            }

            // Try again
            response = await _client.GetAsync($"/users/current/stats/{range.GetValue()}", user.AccessToken);

        }

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Request failed");
            _logger.LogError(await response.Content.ReadAsStringAsync());
        }

        var stats = JsonConvert.DeserializeObject<RootStat>(await response.Content.ReadAsStringAsync())!;
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
        var userStats = await Task.WhenAll(statsTasks);

        _logger.LogInformation("All users refreshed.");
    }

    /// <summary>
    /// Add Users removed back into cache.
    /// </summary>
    private void PostEvictionCallBack(object key, object value, EvictionReason reason, object state)
    {
        Task.Run(async () => await GetStatsAsync((string)key)).Wait();
        _logger.LogInformation($"{key} cache refreshed");
    }

    public async Task<string> GetUserIdAsync(string username)
    {
        var response = _client.GetAsync($"users/{username}/stats").Result;
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Request failed");
            _logger.LogError(response.Content.ReadAsStringAsync().Result);
        }

        var result = await response.Content.ReadAsStringAsync();
        if (result == null)
        {
            return "";
        }
        RootStat data = JsonConvert.DeserializeObject<RootStat>(result)!;
        return data.data.user_id;
    }
}