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

        // Константи для розрахунку F2
        private double B_val;
        private double k_squared_val;
        private double omega_val;
        private double friction_coef;

        // Списки для збереження даних про сили (для дочірнього вікна)
        private List<double> _forcesT = new List<double>();
        private List<double> _forcesN1 = new List<double>();
        private List<double> _forcesN2 = new List<double>();
        private List<double> _forcesNRes = new List<double>();

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
            f_Input.ValueChanged += OnParameterChanged;
        }

        private void OnParameterChanged(object? sender, EventArgs e)
        {
            if (_isSettingDefaults) return;
            UpdatePlot();
        }

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
                // Ігноруємо помилки
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
            f_Input.Value = 0.1; // Значення за замовчуванням для тертя

            StepComboBox.SelectedIndex = 1;

            _isSettingDefaults = false;
            UpdatePlot();
        }

        // --- МЕТОД ОНОВЛЕННЯ ---
        private void UpdatePlot()
        {
            // 1. Зчитуємо параметри
            double c = c_Input.Value;
            double m = m_Input.Value;
            double omega = omega_Input.Value;
            double x0 = x0_Input.Value;
            double v0 = v0_Input.Value;
            double l0 = l0_Input.Value;
            double f = f_Input.Value;

            if (m == 0) { MessageBox.Show("Маса (m) не може бути 0."); return; }

            // 2. Зберігаємо параметри у змінні класу для методу F2
            B_val = (c * l0) / m;
            k_squared_val = (c / m) - Math.Pow(omega, 2);
            omega_val = omega;
            friction_coef = f;

            // 3. Отримуємо крок
            double h = 0.01;
            if (StepComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                double.TryParse(selectedItem.Tag.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out h);
            }

            double k = (k_squared_val > 0) ? Math.Sqrt(k_squared_val) : 0;
            if (k_squared_val > 0) FrequencyLabel.Text = $"k (без тертя): {k:F4} рад/с";
            else FrequencyLabel.Text = "k (без тертя): - (аперіод.)";

            // 4. Підготовка списків
            List<double> tNum = new List<double>();
            List<double> xNum = new List<double>();

            // Очищуємо списки сил
            _forcesT.Clear();
            _forcesN1.Clear();
            _forcesN2.Clear();
            _forcesNRes.Clear();

            // Початкові умови
            double y1 = x0;
            double y2 = v0;
            double t_max = 10.0;
            double t_current = 0;

            while (t_current <= t_max + h / 1000.0)
            {
                tNum.Add(t_current);
                xNum.Add(y1);

                double g = 9.81;
                double N1 = m * g;

                double N2 = m * 2 * omega * y2;

                // Результуюча сила реакції
                double N_res = Math.Sqrt(N1 * N1 + N2 * N2);

                _forcesT.Add(t_current);
                _forcesN1.Add(N1);
                _forcesN2.Add(N2);
                _forcesNRes.Add(N_res);

                // --- Рунге-Кутта 4 ---
                double k1_y1 = h * F1(t_current, y1, y2);
                double k1_y2 = h * F2(t_current, y1, y2);

                double k2_y1 = h * F1(t_current + 0.5 * h, y1 + 0.5 * k1_y1, y2 + 0.5 * k1_y2);
                double k2_y2 = h * F2(t_current + 0.5 * h, y1 + 0.5 * k1_y1, y2 + 0.5 * k1_y2);

                double k3_y1 = h * F1(t_current + 0.5 * h, y1 + 0.5 * k2_y1, y2 + 0.5 * k2_y2);
                double k3_y2 = h * F2(t_current + 0.5 * h, y1 + 0.5 * k2_y1, y2 + 0.5 * k2_y2);

                double k4_y1 = h * F1(t_current + h, y1 + k3_y1, y2 + k3_y2);
                double k4_y2 = h * F2(t_current + h, y1 + k3_y1, y2 + k3_y2);

                y1 = y1 + (1.0 / 6.0) * (k1_y1 + 2 * k2_y1 + 2 * k3_y1 + k4_y1);
                y2 = y2 + (1.0 / 6.0) * (k1_y2 + 2 * k2_y2 + 2 * k3_y2 + k4_y2);

                t_current += h;
            }

            // 5. Малюємо графік руху
            MainPlot.Plot.Clear();

            var numPlot = MainPlot.Plot.Add.Scatter(tNum.ToArray(), xNum.ToArray());
            numPlot.LineWidth = 2;
            numPlot.MarkerSize = 0;
            numPlot.Color = Colors.Blue;
            numPlot.LegendText = $"Чисельний (f={f})";

            MainPlot.Plot.Title($"Графік руху x(t) з тертям. Крок h={h}");
            MainPlot.Plot.XLabel("Час t (c)");
            MainPlot.Plot.YLabel("Положення x (м)");
            MainPlot.Plot.ShowLegend();
            MainPlot.Plot.Axes.AutoScale();
            MainPlot.Refresh();
        }

        public double F1(double t, double y1, double y2)
        {
            return y2;
        }

        public double F2(double t, double y1, double y2)
        {
            double a_base = B_val - k_squared_val * y1;

            // Розрахунок прискорення від сили тертя
            // m скорочується
            // N1_accel = g
            double g = 9.81;
            double n1_accel = g;

            // N2_accel = 2 * omega * v
            // y2 це швидкість (v)
            double n2_accel = 2 * omega_val * y2;

            double n_res_accel = Math.Sqrt(n1_accel * n1_accel + n2_accel * n2_accel);

            double a_friction = friction_coef * n_res_accel;

            // Фінальне рівняння
            // Тертя завжди протидіє швидкості: -sign(v) * a_friction
            return a_base - Math.Sign(y2) * a_friction;
        }

        // Відкриття вікна графіків сил
        private void MainPlot_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Передаємо збережені списки сил у нове вікно
            ForcesWindow forcesWin = new ForcesWindow(_forcesT, _forcesN1, _forcesN2, _forcesNRes);
            forcesWin.Owner = this;
            forcesWin.ShowDialog();
            e.Handled = true;
        }
    }
}