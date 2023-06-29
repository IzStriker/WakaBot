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

    public override byte[] GeneratePie(DataPoint<double>[] dataPoints)
    {
        var plotModel = new PlotModel
        {
            TextColor = OxyColors.White,
            DefaultFontSize = 30,

        };

        var seriesP1 = new PieSeries
        {
            StrokeThickness = 2.0,
            AngleSpan = 360,
            StartAngle = 0,
            InsideLabelFormat = "",
            OutsideLabelFormat = "{1} {2:0}%",
            EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
        };

        foreach (var point in dataPoints)
        {
            seriesP1.Slices.Add(new PieSlice(point.label, point.value) { Fill = OxyColor.Parse(GetColour(point.label)) });
        }

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