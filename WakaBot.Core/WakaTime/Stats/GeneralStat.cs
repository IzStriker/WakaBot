namespace WakaBot.Core.WakaTimeAPI.Stats;
public class GeneralStat
{
    public string @decimal { get; set; } = string.Empty;
    public string digital { get; set; } = string.Empty;
    public int hours { get; set; }
    public int minutes { get; set; }
    public string name { get; set; } = string.Empty;
    public float percent { get; set; }
    public string text { get; set; } = string.Empty;
    public float total_seconds { get; set; }
}