namespace WakaBot.Core.Extensions;
public class Metrics
{
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    private readonly List<TimeSpan> _requestTimes = new();
    public double? TotalResponseTime
    {
        get => _requestTimes.Select(t => t.TotalMilliseconds).Sum();
    }

    public long NumberOfRequests
    {
        get => _requestTimes.Count;
    }

    public void AddResponseTime(TimeSpan time)
    {
        _requestTimes.Add(time);
    }
}
