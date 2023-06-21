using QuickChart;
using Newtonsoft.Json;

namespace WakaBot.Core.Graphs;

/// <summary>
/// Generate graphs to display in messages.
/// </summary>
public class QuickChartGenerator : GraphGenerator
{
    public QuickChartGenerator(string url) : base(url)
    { }

    private string DiscordBackgroundColour => "#36393f";

    public override byte[] GeneratePie(DataPoint<double>[] dataPoints)
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

        return chart.ToByteArray();
    }

    public override byte[] GenerateBar(String[] labels, DataPoint<float[]>[] dataPoints)
    {
        Chart chart = new Chart()
        {
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
                // Key map
                legend = new
                {
                    labels = new
                    {
                        fontColor = "#FFFFFF",
                        fontSize = 16
                    }
                },
                scales = new
                {
                    xAxes = new[] {
                        new
                        {
                            stacked = true,
                            // X-Axis Labels
                            ticks = new
                            {
                                fontColor = "#FFFFFF",
                                fontSize = 16
                            },
                            gridLines = new
                            {
                                display = false,
                            }
                        }
                    },
                    yAxes = new[] {
                        new
                        {
                            stacked = true,
                            // Y-Axis Labels
                            ticks = new
                            {
                                fontColor = "#FFFFFF",
                                fontSize = 16
                            },
                            gridLines = new
                            {
                                display = false,
                            }
                        },
                    },
                },
            }
        };

        chart.Config = JsonConvert.SerializeObject(config);
        chart.BackgroundColor = DiscordBackgroundColour;

        return chart.ToByteArray();
    }
}