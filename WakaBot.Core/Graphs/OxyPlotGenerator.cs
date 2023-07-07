using OxyPlot;
using OxyPlot.Series;
using OxyPlot.ImageSharp;

namespace WakaBot.Core.Graphs;

public class OxyPlotGenerator : GraphGenerator
{
    protected override int Width => 600;
    protected override int Height => 400;
    public OxyPlotGenerator(string url) : base(url)
    { }

    public override byte[] GeneratePie(DataPoint<double>[] dataPoints, double otherThreshold = 0.01)
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
        throw new NotImplementedException();
    }
}