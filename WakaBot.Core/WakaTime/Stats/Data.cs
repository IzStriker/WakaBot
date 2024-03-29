namespace WakaBot.Core.WakaTimeAPI.Stats;

public class Data
{
    public GeneralStat[] categories { get; set; } = new GeneralStat[] { };
    public float daily_average { get; set; }
    public float daily_average_including_other_language { get; set; }
    public int days_including_holidays { get; set; }
    public int days_minus_holidays { get; set; }
    public GeneralStat[] editors { get; set; } = new GeneralStat[] { };
    public int holidays { get; set; }
    public string human_readable_daily_average { get; set; } = string.Empty;
    public string human_readable_daily_average_including_other_language { get; set; } = string.Empty;
    public string human_readable_range { get; set; } = string.Empty;
    public string human_readable_total { get; set; } = string.Empty;
    public string human_readable_total_including_other_language { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public bool is_already_updating { get; set; }
    public bool is_coding_activity_visible { get; set; }
    public bool is_including_today { get; set; }
    public bool is_other_usage_visible { get; set; }
    public bool is_stuck { get; set; }
    public bool is_up_to_date { get; set; }
    public GeneralStat[] languages { get; set; } = new GeneralStat[] { };
    public GeneralStat[] operating_systems { get; set; } = new GeneralStat[] { };
    public int percent_calculated { get; set; }
    public string range { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public int timeout { get; set; }
    public float total_seconds { get; set; }
    public float total_seconds_including_other_language { get; set; }
    public string user_id { get; set; } = string.Empty;
    public string username { get; set; } = string.Empty;
    public bool writes_only { get; set; }
    public GeneralStat[] projects { get; set; } = new GeneralStat[] { };
}