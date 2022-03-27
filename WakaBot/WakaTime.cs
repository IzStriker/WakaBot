using Newtonsoft.Json.Linq;

namespace WakaBot;

public class WakaTime
{
    const string BaseUrl = "https://wakatime.com/api/v1/";

    public static async Task<bool> UserExists(string username)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{BaseUrl}/users/{username}/stats");
        
        return response.StatusCode == System.Net.HttpStatusCode.OK;
    }

    public static async Task<bool> StatsAvaible(string username)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{BaseUrl}/users/{username}/stats");

        var data = JObject.Parse(await response.Content.ReadAsStringAsync());

        if (!data.ContainsKey("data")) return false;
        
        data = JObject.FromObject(data["data"]!);

        return data.ContainsKey("categories") && data.ContainsKey("editors") && data.ContainsKey("languages");
    }
}