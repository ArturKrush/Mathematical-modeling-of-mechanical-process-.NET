using ScottPlot;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Lab1MathMod
{
    public partial class ForcesWindow : Window
    {
        public ForcesWindow(List<double> t, List<double> n1, List<double> n2, List<double> nRes)
        {
            InitializeComponent();
            PlotForces(t, n1, n2, nRes);
        }

        private void PlotForces(List<double> t, List<double> n1, List<double> n2, List<double> nRes)
        {
            ForcesPlot.Plot.Clear();

            // Перетворюємо списки в масиви
            double[] tArray = t.ToArray();
            double[] n1Array = n1.ToArray();
            double[] n2Array = n2.ToArray();
            double[] nResArray = nRes.ToArray();

            // 1. Графік N1 (Чорний)
            var plotN1 = ForcesPlot.Plot.Add.Scatter(tArray, n1Array);
            plotN1.Color = Colors.Black;
            plotN1.LineWidth = 2;
            plotN1.MarkerSize = 0;
            plotN1.LegendText = "N1";

            // 2. Графік N2 (Синій)
            var plotN2 = ForcesPlot.Plot.Add.Scatter(tArray, n2Array);
            plotN2.Color = Colors.Blue;
            plotN2.LineWidth = 2;
            plotN2.MarkerSize = 0;
            plotN2.LegendText = "N2";

            // 3. Графік N результуюче (Червоний)
            var plotNRes = ForcesPlot.Plot.Add.Scatter(tArray, nResArray);
            plotNRes.Color = Colors.Red;
            plotNRes.LineWidth = 2;
            plotNRes.MarkerSize = 0;
            plotNRes.LegendText = "N результуюча";

            ForcesPlot.Plot.Title("Залежність сил реакції опори від часу");
            ForcesPlot.Plot.XLabel("Час t (c)");
            ForcesPlot.Plot.YLabel("Сила N (Ньютони)");
            ForcesPlot.Plot.ShowLegend();
            ForcesPlot.Plot.Axes.AutoScale();
            ForcesPlot.Refresh();
        }
    }
}