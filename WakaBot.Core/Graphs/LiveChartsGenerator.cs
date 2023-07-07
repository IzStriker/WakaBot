// using LiveChartsCore.SkiaSharpView;
// using LiveChartsCore.SkiaSharpView.Painting;
// using LiveChartsCore.SkiaSharpView.SKCharts;
// using SkiaSharp;

// namespace WakaBot.Core.Graphs;

// public class LiveChartsGraphGenerator : GraphGenerator
// {
//     public LiveChartsGraphGenerator(string url) : base(url)
//     { }

//     public override byte[] GeneratePie(DataPoint<double>[] dataPoints)
//     {

//         var pieChart = new SKPieChart()
//         {
//             Height = 400,
//             Width = 600,
//             Background = SKColors.Transparent,
//         };

//         pieChart.Series = dataPoints.Select(point => new PieSeries<double>
//         {
//             Values = new[] { point.value },
//             DataLabelsFormatter = p => point.label,
//             DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
//             DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
//         }).ToList();

//         return pieChart.GetImage().Encode().ToArray();
//     }

//     public override byte[] GenerateBar(string[] labels, DataPoint<float[]>[] dataPoints)
//     {
//         throw new NotImplementedException();
//     }

// }