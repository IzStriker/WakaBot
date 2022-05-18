using ZedGraph;
using System.Drawing;

namespace WakaBot.Graphs;

public class GraphGenerator
{
    private readonly int Width = 650;
    private readonly int Height = 650;
    private readonly float FontScaler = 2.0f;
    private string _imageDirectory = "Images";

    public GraphGenerator(string imageDirectory)
    {
        _imageDirectory = imageDirectory ?? "Images";
    }

    public string GeneratePie(DataPoint<double>[] dataPoints, string filename)
    {
        Rectangle rect = new Rectangle(12, 12, Width - 24, Height - 24);

        // Don't want any titles or headings
        GraphPane pane = new GraphPane(rect, String.Empty, String.Empty, String.Empty);
        pane.Fill = new Fill(Color.Transparent);
        pane.Chart.Fill = new Fill(Color.Transparent);
        pane.Border.Color = Color.Transparent;

        // Draw each slice of the pie
        Random rand = new Random();
        foreach (var point in dataPoints)
        {
            Random randomGen = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            KnownColor randomColorName = names[randomGen.Next(names.Length)];
            Color randomColor = Color.FromKnownColor(randomColorName);

            var slice = pane.AddPieSlice(point.value, randomColor, 0f, point.label);
            slice.LabelDetail.FontSpec.Size = FontScaler * (Width / 100);
            slice.LabelType = PieLabelType.Name_Percent;
            slice.Label.IsVisible = false;
            slice.Border.Color = Color.White;
        }

        // Adjust graph to fit size
        pane.AxisChange();

        var path = Path.Join(_imageDirectory, filename);
        Directory.CreateDirectory(_imageDirectory);

        pane.GetImage().Save(path, System.Drawing.Imaging.ImageFormat.Png);
        return path;
    }
}