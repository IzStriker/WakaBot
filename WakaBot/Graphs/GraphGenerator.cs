using ZedGraph;
using System.Drawing;

namespace WakaBot.Graphs;

public class GraphGenerator
{
    private static readonly int Width = 650;
    private static readonly int Height = 650;
    private static readonly float FontScaler = 3.0f;

    public static Bitmap GeneratePie(DataPoint<double>[] dataPoints)
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
        pane.GetImage().Save("test.png", System.Drawing.Imaging.ImageFormat.Png);
        return pane.GetImage();
    }
}