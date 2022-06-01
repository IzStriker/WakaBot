using QuickChart;
using Newtonsoft.Json;
using System.Net;

namespace WakaBot.Graphs;

public class GraphGenerator
{
    private readonly int Width = 650;
    private readonly int Height = 650;

    public byte[] GeneratePie(DataPoint<double>[] dataPoints)
    {
        Chart chart = new Chart()
        {
            DevicePixelRatio = 10,
            Width = this.Width,
            Height = this.Height
        };

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
                        data = dataPoints.Select(point => point.value)
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

        return webClient.DownloadData(chart.GetUrl());
    }
}