using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using WakaBot.Core.WakaTimeAPI.Stats;

namespace WakaBot.Core.WakaTimeAPI;

public class WakaTimeCacheHandler : DelegatingHandler
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<WakaTimeCacheHandler> _logger;
    private long _cacheHits = 0;
    private long _cacheMisses = 0;

    public WakaTimeCacheHandler(IMemoryCache cache, ILogger<WakaTimeCacheHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        if (_cache.TryGetValue(request.RequestUri.AbsoluteUri, out string? cachedContent))
        {
            _cacheHits++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(cachedContent!)
            };
        }
        _cacheMisses++;

        var response = await base.SendAsync(request, cancellationToken);
        var entry = JsonConvert.DeserializeObject<RootStat>(await response.Content.ReadAsStringAsync())!;

        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
        TimeSpan timeTillExpiration;

        if (entry.data.is_up_to_date)
        {
            timeTillExpiration = DateTime.Today.AddDays(1).AddHours(3) - DateTime.Now;
        }
        else
        {
            // Stats will be refreshed soon, so try again in 30 minutes.
            timeTillExpiration = TimeSpan.FromMinutes(30);
        }
        options.AbsoluteExpirationRelativeToNow = timeTillExpiration;

        // Remove item from cache when expires, instead of when get is next called.
        options.AddExpirationToken(new CancellationChangeToken(
            new CancellationTokenSource(timeTillExpiration).Token));

        // Don't refresh the cache after eviction if includes time range
        if (request.RequestUri.AbsolutePath.EndsWith("/stats"))
        {
            options.RegisterPostEvictionCallback(PostEvictionCallBack);
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            _cache.Set(request.RequestUri.AbsoluteUri, content, options);
        }

        return response;
    }

    /// <summary>
    /// Add Users removed back into cache.
    /// </summary>
    private void PostEvictionCallBack(object key, object value, EvictionReason reason, object state)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, (string)key);
        var result = base.Send(request, CancellationToken.None);
        if (result.StatusCode == HttpStatusCode.OK)
        {
            _cache.Set(key, result.Content.ReadAsStringAsync());
        }
        _logger.LogInformation($"{key} cache refreshed");
    }

    public long GetCacheMisses()
    {
        return _cacheMisses;
    }

    public long GetCacheHits()
    {
        return _cacheHits;
    }
}