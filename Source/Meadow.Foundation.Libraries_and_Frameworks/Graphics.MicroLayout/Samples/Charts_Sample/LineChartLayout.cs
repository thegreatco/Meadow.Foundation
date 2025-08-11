using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Charts_Sample;

public class LineChartLayout : StackLayout
{
    private const int PointsPerSeries = 50;

    public LineChartLayout(int width, int height)
        : base(0, 0, width, height, Orientation.Vertical)
    {
        var title = new Label(width, 22, text: "Basic Chart Example")
        {
            Font = new Font12x16(),
            TextColor = Color.Black,
            BackgroundColor = Color.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var horizontalStack = new StackLayout(
            10, 10,
            width - 20, height - 20,
            Orientation.Horizontal,
            CrossAxisAlignment.Center);
        horizontalStack.BackgroundColor = Color.DarkGray;

        var chart = new LineChart(0, 0, width, height)
        {
            BackgroundColor = Color.FromHex("111111"),
            ShowYAxisLabels = true,

        };

        chart.Series.Add(
            GetSineSeries(),
            GetCosineSeries(4, 4.2, 0));

        horizontalStack.Controls.Add(chart);

        Controls.Add(title);
        Controls.Add(horizontalStack);

        Padding = 5;
        Spacing = 5;
        BackgroundColor = Color.DimGray;
    }

    private LineChartSeries GetSineSeries(double xScale = 4, double yScale = 1.5, double yOffset = 1.5)
    {
        var series = new LineChartSeries()
        {
            LineColor = Color.Red,
            PointColor = Color.Green,
            LineStroke = 1,
            PointSize = 6,
            ShowLines = true,
            ShowPoints = false,

        };

        for (var p = 0; p < PointsPerSeries; p++)
        {
            series.Points.Add(p * 2, (Math.Sin(p / xScale) * yScale) + yOffset);
        }

        return series;
    }

    private LineChartSeries GetCosineSeries(double xScale = 4, double yScale = 1.5, double yOffset = 4.5)
    {
        var series = new LineChartSeries()
        {
            LineColor = Color.DarkBlue,
            PointColor = Color.DarkGreen,
            LineStroke = 1,
            PointSize = 6,
            ShowLines = true,
            ShowPoints = false,

        };

        for (var p = 0; p < PointsPerSeries; p++)
        {
            series.Points.Add(p * 2, (Math.Cos(p / xScale) * yScale) + yOffset);
        }

        return series;
    }
}