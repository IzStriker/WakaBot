namespace WakaBot.Core.Extensions;
public class Metrics
{
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    private readonly List<TimeSpan> _requestTimes = new();
    public TimeSpan? AverageResponseTime
    {
        get
        {
            return _requestTimes.Count > 0 ? TimeSpan.FromMilliseconds(_requestTimes.Average(t => t.TotalMilliseconds)) : null;
        }

        set
        {
            if (value != null)
            {
                _requestTimes.Add(value.Value);
            }
        }
    }

    public long NumberOfRequests
    {
        get
        {
            return _requestTimes.Count;
        }
    }
}
