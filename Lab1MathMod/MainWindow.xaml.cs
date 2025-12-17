using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lab1MathMod
{
    public partial class MainWindow : Window
    {
        private const double ParamStep = 0.01;
        private bool _isSettingDefaults = false;

        // Дані для передачі у вікно помилок (щоб не перераховувати їх двічі)
        private List<CalculationResult> _lastResults = new List<CalculationResult>();

        public MainWindow()
        {
            InitializeComponent();
            SetupNumericInputs();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImages();
            SetDefaultValues();
        }

        private void SetupNumericInputs()
        {
            c_Input.ValueChanged += OnParameterChanged;
            m_Input.ValueChanged += OnParameterChanged;
            omega_Input.ValueChanged += OnParameterChanged;
            x0_Input.ValueChanged += OnParameterChanged;
            v0_Input.ValueChanged += OnParameterChanged;
            l0_Input.ValueChanged += OnParameterChanged;
        }

        private void OnParameterChanged(object? sender, EventArgs e)
        {
            if (_isSettingDefaults) return;
            UpdatePlot();
        }

        // Обробник зміни кроку в ComboBox
        private void StepComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSettingDefaults) return;
            if (IsLoaded) UpdatePlot();
        }

        private void LoadImages()
        {
            try
            {
                string equationImagePath = "C:\\Users\\artur\\Downloads\\Formula.png";
                string modelImagePath = "C:\\Users\\artur\\Downloads\\Model_4.png";

                EquationImage.Source = new BitmapImage(new Uri(equationImagePath));
                ModelImage.Source = new BitmapImage(new Uri(modelImagePath));
            }
            catch (Exception ex)
            {
                // Ігноруємо помилки завантаження
            }
        }

        private void DefaultValuesButton_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            _isSettingDefaults = true;

            c_Input.Value = 0.45;
            m_Input.Value = 0.02;
            omega_Input.Value = 2.0 / 3.0;
            x0_Input.Value = 0.1;
            v0_Input.Value = 0.3;
            l0_Input.Value = 0.5;

            StepComboBox.SelectedIndex = 1; // 0.01 за замовчуванням

            _isSettingDefaults = false;
            UpdatePlot();
        }

        // АНАЛІТИЧНИЙ МЕТОД
        private double CalculateAnalytical(double t, double k_squared, double B, double x0, double v0, double k)
        {
            if (k_squared > 0)
            {
                return (x0 - B / k_squared) * Math.Cos(k * t) + (v0 / k) * Math.Sin(k * t) + (B / k_squared);
            }
            else if (k_squared < 0)
            {
                double k_abs_sqrt = Math.Sqrt(Math.Abs(k_squared));
                double C1 = (x0 / 2.0) + v0 / (2.0 * k_abs_sqrt) - B / (2.0 * k_squared);
                double C2 = (x0 / 2.0) - v0 / (2.0 * k_abs_sqrt) - B / (2.0 * k_squared);
                return C1 * Math.Exp(k_abs_sqrt * t) + C2 * Math.Exp(-k_abs_sqrt * t) + (B / k_squared);
            }
            else // k_squared == 0
            {
                return (B * Math.Pow(t, 2)) / 2.0 + v0 * t + x0;
            }
        }

        private void UpdatePlot()
        {
            // 1. Зчитуємо параметри
            double c = c_Input.Value;
            double m = m_Input.Value;
            double omega = omega_Input.Value;
            double x0 = x0_Input.Value;
            double v0 = v0_Input.Value;
            double l0 = l0_Input.Value;

            if (m == 0) { MessageBox.Show("Маса (m) не може бути 0."); return; }

            // 2. Отримуємо крок з ComboBox
            double h = 0.01;
            if (StepComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                double.TryParse(selectedItem.Tag.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out h);
            }

            // 3. Параметри моделі
            k_squared = (c / m) - Math.Pow(omega, 2);
            B = (c * l0) / m;
            double k = (k_squared > 0) ? Math.Sqrt(k_squared) : 0;

            if (k_squared > 0) FrequencyLabel.Text = $"Частота коливань k: {k:F4} рад/с";
            else if (k_squared < 0) FrequencyLabel.Text = "Частота коливань k: - (аперіодичний)";
            else FrequencyLabel.Text = "Частота коливань k: 0 (критичний)";

            // 4. Підготовка даних
            _lastResults.Clear();
            double t_max = 10.0;

            // --- ЧИСЕЛЬНИЙ РОЗВ'ЯЗОК (Рунге-Кутта) ---
            List<double> tNum = new List<double>();
            List<double> xNum = new List<double>();

            // Початкові умови: y1 = x (положення), y2 = v (швидкість)
            double y1 = x0;
            double y2 = v0;

            double t_current = 0;

            // +h/1000.0 для компенсації похибки float при порівнянні
            while (t_current <= t_max + h / 1000.0)
            {
                tNum.Add(t_current);
                xNum.Add(y1);

                // Розрахунок аналітичного для таблиці
                double analVal = CalculateAnalytical(t_current, k_squared, B, x0, v0, k);

                CalculationResult res = new CalculationResult
                {
                    Time = t_current,
                    Analytical = analVal,
                    Numerical = y1
                };
                res.AbsError = Math.Abs(res.Analytical - res.Numerical);
                res.RelError = (Math.Abs(res.Analytical) > 0) ? (res.AbsError / Math.Abs(res.Analytical)) : 0;
                res.RelErrorPercent = res.RelError * 100.0;
                _lastResults.Add(res);

                // k1
                double k1_y1 = h * F1(t_current, y1, y2);
                double k1_y2 = h * F2(t_current, y1, y2);

                // k2
                double k2_y1 = h * F1(t_current + 0.5 * h, y1 + 0.5 * k1_y1, y2 + 0.5 * k1_y2);
                double k2_y2 = h * F2(t_current + 0.5 * h, y1 + 0.5 * k1_y1, y2 + 0.5 * k1_y2);

                // k3
                double k3_y1 = h * F1(t_current + 0.5 * h, y1 + 0.5 * k2_y1, y2 + 0.5 * k2_y2);
                double k3_y2 = h * F2(t_current + 0.5 * h, y1 + 0.5 * k2_y1, y2 + 0.5 * k2_y2);

                // k4
                double k4_y1 = h * F1(t_current + h, y1 + k3_y1, y2 + k3_y2);
                double k4_y2 = h * F2(t_current + h, y1 + k3_y1, y2 + k3_y2);

                y1 = y1 + (1.0 / 6.0) * (k1_y1 + 2 * k2_y1 + 2 * k3_y1 + k4_y1);
                y2 = y2 + (1.0 / 6.0) * (k1_y2 + 2 * k2_y2 + 2 * k3_y2 + k4_y2);

                t_current += h;
            }

            // --- АНАЛІТИЧНИЙ РОЗВ'ЯЗОК (для графіка) ---
            int N_anal = 1001;
            double[] tAnalPoints = new double[N_anal];
            double[] xAnalPoints = new double[N_anal];
            for (int i = 0; i < N_anal; i++)
            {
                double t = (t_max * i) / (N_anal - 1);
                tAnalPoints[i] = t;
                xAnalPoints[i] = CalculateAnalytical(t, k_squared, B, x0, v0, k);
            }

            // 5. Малюємо графіки
            MainPlot.Plot.Clear();

            // --- АНАЛІТИЧНИЙ: ЧОРНИЙ ---
            var analPlot = MainPlot.Plot.Add.Scatter(tAnalPoints, xAnalPoints);
            analPlot.LineWidth = 2;
            analPlot.Color = Colors.Black; // Чорний колір
            analPlot.MarkerSize = 0;
            analPlot.LegendText = "Аналітичний";

            // --- ЧИСЕЛЬНИЙ: СИНІЙ ---
            var numPlot = MainPlot.Plot.Add.Scatter(tNum.ToArray(), xNum.ToArray());
            numPlot.LineWidth = 2;       // Повноцінна лінія
            numPlot.MarkerSize = 0;      // Без точок, суцільна крива
            numPlot.Color = Colors.Blue; // Синій колір
            numPlot.LegendText = "Чисельний (Рунге-Кутта)";

            MainPlot.Plot.Title($"Графік руху x(t). Крок h={h}");
            MainPlot.Plot.XLabel("Час t (c)");
            MainPlot.Plot.YLabel("Положення x (м)");
            MainPlot.Plot.ShowLegend();
            MainPlot.Plot.Axes.AutoScale();
            MainPlot.Refresh();
        }

        double B;
        double k_squared;

        public double F1(double t, double y1, double y2) {
            return y2;
        }

        public double F2(double t, double y1, double y2)
        {
            return B - k_squared * y1;
        }

        private void MainPlot_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ErrorWindow errorWin = new ErrorWindow(_lastResults);
            errorWin.Owner = this;
            errorWin.ShowDialog();
            e.Handled = true;
        }
    }

    public class CalculationResult
    {
        public double Time { get; set; }
        public double Analytical { get; set; }
        public double Numerical { get; set; }
        public double AbsError { get; set; }
        public double RelError { get; set; }
        public double RelErrorPercent { get; set; }
    }
}