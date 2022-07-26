using QuickChart;
using Newtonsoft.Json;
using System.Net;

namespace WakaBot.Graphs;

/// <summary>
/// Generate graphs to display in messages.
/// </summary>
public class GraphGenerator
{
    private readonly int Width = 650;
    private readonly int Height = 650;
    private Dictionary<string, Dictionary<string, string>> _colourMap;


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
    /// Creates an image of a pie chart.
    /// </summary>
    /// <param name="dataPoints">Data used to create the chart.</param>
    /// <returns></returns>
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

        // I prefer image above embed rather than inside,
        // Discord doesn't allow bot to post images by link.
        // Therefore the image needs to be downloaded.
        return webClient.DownloadData(chart.GetUrl());
    }

    public byte[] GenerateBar(String[] labels, DataPoint<float[]>[] dataPoints)
    {
        Chart chart = new Chart()
        {
            DevicePixelRatio = 10,
            Width = this.Width,
            Height = this.Height
        };

        var config = new
        {
            type = "bar",
            data = new
            {
                labels,
                datasets = dataPoints.Select(point => new
                {
                    label = point.label,
                    data = point.value
                })
            },
            options = new
            {
                scales = new
                {
                    xAxes = new[] {
                        new
                        {
                            stacked = true
                        }
                    },
                    yAxes = new[] {
                        new
                        {
                            stacked = true
                        }
                    }
                },
            }
        };

        chart.Config = JsonConvert.SerializeObject(config);
        chart.BackgroundColor = "#FFF";

        Console.WriteLine(chart.GetUrl());

        return chart.ToByteArray();
    }

    /// <summary>
    /// If key is a programming language it picks the colour associated with that language,
    /// else picks a random colour.  
    /// </summary>
    /// <param name="key">Value used to generate colour.</param>
    /// <returns>hash code of colour.</returns>
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