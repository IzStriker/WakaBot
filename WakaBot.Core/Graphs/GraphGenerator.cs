using Newtonsoft.Json;

namespace WakaBot.Core.Graphs;

public abstract class GraphGenerator
{

    protected int Width => 650;
    protected int Height => 650;
    protected Dictionary<string, Dictionary<string, string>> _colourMap;

    /// <summary>
    /// Creates an instance of GraphGenerator.
    /// </summary>
    /// <param name="url">url of github colour repo.</param>
    public GraphGenerator(string url)
    {
        string colourURL = url ?? "https://raw.githubusercontent.com/ozh/github-colors/master/colors.json";
        using var client = new HttpClient();
        string data = "";

        // Decided to block as application is dependent of this data.
        Task.Run(async () =>
         data = await client.GetStringAsync(colourURL)
        ).Wait();

        this._colourMap = JsonConvert.DeserializeObject<Dictionary<
                string, Dictionary<string, string>>>(data)!;
    }

    /// <summary>
    /// If key is a programming language it picks the colour associated with that language,
    /// else picks a random colour.  
    /// </summary>
    /// <param name="key">Value used to generate colour.</param>
    /// <returns>hash code of colour.</returns>
    protected string GetColour(string key)
    {
        if (_colourMap.ContainsKey(key))
        {
            return _colourMap[key]["color"];
        }

        Random rnd = new Random();
        return _colourMap.ElementAt(rnd.Next(_colourMap.Count())).Value["color"];
    }

    /// <summary>
    /// Creates an image of a pie chart.
    /// </summary>
    /// <param name="dataPoints">Data used to create the chart.</param>
    /// <returns></returns>
    public abstract byte[] GeneratePie(DataPoint<double>[] dataPoints);

    /// <summary>
    /// Creates an image of a stacked bar chart.
    /// </summary>
    /// <param name="labels">Labels for the x axis.</param>
    /// <param name="dataPoints">Data used to create the chart.</param>
    /// <returns></returns>
    public abstract byte[] GenerateBar(String[] labels, DataPoint<float[]>[] dataPoints);
}