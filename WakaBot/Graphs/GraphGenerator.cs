using QuickChart;
using Newtonsoft.Json;
using System.Net;

namespace WakaBot.Graphs;

public class GraphGenerator
{
    private readonly int Width = 650;
    private readonly int Height = 650;
    private Dictionary<string, Dictionary<string, string>> _colourMap;

    public GraphGenerator()
    {
        string path = Path.Join(AppContext.BaseDirectory,
            "github-colors", "colors.json");
        this._colourMap = JsonConvert.DeserializeObject<Dictionary<
                string, Dictionary<string, string>>>(File.ReadAllText(path))!;
    }

    public byte[] GeneratePie(DataPoint<double>[] dataPoints)
    {
        Chart chart = new Chart()
        {
            DevicePixelRatio = 10,
            Width = this.Width,
            Height = this.Height
        };

        List<string> colours = new List<string>();
        foreach (string label in dataPoints.Select(point => point.label))
        {
            colours.Add(GetColour(label));
        }

        var config = new
        {
            type = "outlabeledPie",
            data = new
            {
                labels = dataPoints.Select(point => point.label),
                datasets = new[]
                {
                    new
                    {
                        data = dataPoints.Select(point => point.value),
                        backgroundColor = colours
                    }
                }
            },
            options = new
            {
                plugins = new
                {
                    legend = false, // disable legend label box
                    outlabels = new
                    {
                        stretch = 35,
                        font = new
                        {
                            text = "%l %p",
                            resizeable = true,
                            minSize = 12,
                            maxSize = 18
                        }
                    }
                }
            }
        };


        chart.Config = JsonConvert.SerializeObject(config);
        using WebClient webClient = new WebClient();
        var httpClient = new HttpClient();

        // I prefer image above embed rather than inside,
        // Discord doesn't allow bot to post images by link.
        // Therefore the image needs to be downloaded.
        return webClient.DownloadData(chart.GetUrl());
    }

    private string GetColour(string key)
    {
        if (_colourMap.ContainsKey(key))
        {
            return _colourMap[key]["color"];
        }

        Random rnd = new Random();
        return _colourMap.ElementAt(rnd.Next(_colourMap.Count())).Value["color"];
    }
}