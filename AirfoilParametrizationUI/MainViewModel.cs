namespace AirfoilParametrizationUI
{
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class MainViewModel
    {
        public MainViewModel()
        {
            
        }

        public MainViewModel(string filepath)
        {
            AirfoilModel = new PlotModel { TitleFontSize = 14, TitleFont = "Segoe UI", TitleFontWeight = 10, DefaultColors = new List<OxyColor> { OxyColor.FromRgb(0, 0, 0) }, PlotType = PlotType.XY };
            Random r = new Random();
            ScatterSeries scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerFill = OxyColors.Red};

            List<string> lines = File.ReadAllLines(filepath).ToList();
            AirfoilModel.Title = lines[0];
            lines.RemoveAt(0);

            foreach (string line in lines)
            {
                List<string> values = line.Split("     ").ToList();

                double x = double.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                double y = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                scatterSeries.Points.Add(new ScatterPoint(x, y, 2));
            }

            AirfoilModel.Axes.Add(new LinearAxis() { Maximum = 0.3, Minimum = -0.3, Position = AxisPosition.Left, Key = "Vertical", MinorGridlineStyle = LineStyle.Solid, MinorGridlineColor = OxyColors.LightGray, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray});
            AirfoilModel.Axes.Add(new LinearAxis() { AbsoluteMaximum = 1, AbsoluteMinimum = 0, Position = AxisPosition.Bottom, Key = "Horizontal", MinorGridlineStyle = LineStyle.Solid, MinorGridlineColor = OxyColors.LightGray, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray });
            scatterSeries.XAxisKey = "Horizontal";
            scatterSeries.YAxisKey = "Vertical";
            AirfoilModel.Series.Add(scatterSeries);
        }

        public PlotModel AirfoilModel { get; private set; }
    }
}
