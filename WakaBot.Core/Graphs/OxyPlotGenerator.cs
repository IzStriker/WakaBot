using OxyPlot;
using OxyPlot.Series;
using OxyPlot.ImageSharp;
using OxyPlot.Axes;
using OxyPlot.Legends;

namespace WakaBot.Core.Graphs;

public class OxyPlotGenerator : GraphGenerator
{
    protected override int Width => 600;
    protected override int Height => 400;

    public override byte[] GeneratePie(DataPoint<double>[] dataPoints, double otherThreshold)
    {
        var plotModel = new PlotModel
        {
            TextColor = OxyColors.White,
        };

        var seriesP1 = new PieSeries
        {
            StrokeThickness = 2.0,
            AngleSpan = 360,
            StartAngle = 0,
            InsideLabelFormat = "",
            OutsideLabelFormat = "{1} {2:0}%",
            FontSize = 15,
        };

        double total = dataPoints.Sum(point => point.value);
        double other = 0;
        foreach (var point in dataPoints)
        {
            if (point.value / total < otherThreshold)
            {
                other += point.value;
            }
            else
            {
                seriesP1.Slices.Add(new PieSlice(point.label, point.value) { Fill = OxyColor.Parse(GetColour(point.label)) });
            }
        }

        seriesP1.Slices.Add(new PieSlice("Other", other) { Fill = OxyColor.Parse(GetColour("Other")) });

        plotModel.Series.Add(seriesP1);

        using var stream = new MemoryStream();
        var pngExporter = new PngExporter(this.Width, this.Height);
        pngExporter.Export(plotModel, stream);
        return stream.ToArray();
    }

    public override byte[] GenerateBar(string[] labels, DataPoint<float[]>[] dataPoints)
    {
        var plotModel = new PlotModel
        {
            TextColor = OxyColors.White,
            Legends = {
                new Legend {
                    LegendPosition = LegendPosition.RightTop,
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendFontSize = 15,
                }
            },
        };

        foreach (var point in dataPoints.Select((value, index) => new { value, index }))
        {
            var series = new BarSeries
            {
                Title = point.value.label,
                FontSize = 20,
                IsStacked = true,
                XAxisKey = "Value",
                YAxisKey = "Category"
            };

            foreach (var v in point.value.value)
            {
                series.Items.Add(new BarItem { Value = v });
            }
            plotModel.Series.Add(series);
        }

        var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom, Key = "Category", FontSize = 20 };
        foreach (var label in labels)
        {
            categoryAxis.Labels.Add(label);
        }
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Key = "Value", FontSize = 20 });

        using var stream = new MemoryStream();
        var pngExporter = new PngExporter(this.Width, this.Height);
        pngExporter.Export(plotModel, stream);
        return stream.ToArray();
    }
}