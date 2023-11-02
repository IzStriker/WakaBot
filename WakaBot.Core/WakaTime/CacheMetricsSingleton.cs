namespace WakaBot.Core.WakaTime;
public class CacheMetricsSingleton
{
    private CacheMetricsSingleton() { }

    private static readonly CacheMetricsSingleton _instance = new CacheMetricsSingleton();

    public long CacheHits { get; set; }

    public long CacheMisses { get; set; }

    public static CacheMetricsSingleton Instance()
    {
        return _instance;
    }
}
