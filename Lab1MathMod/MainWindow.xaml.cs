using ScottPlot; // Додаємо ScottPlot
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lab1MathMod
{
    public partial class MainWindow : Window
    {
        // Змінні для параметрів з кроком (для кнопок вгору/вниз)
        private const double ParamStep = 0.01;

        private bool isSettingDefaults = false;

        public MainWindow()
        {
            InitializeComponent();
            SetupNumericInputs();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImages();
            SetDefaultValues(); // Встановлюємо значення за замовчуванням при завантаженні
        }

        private void SetupNumericInputs()
        {
            // Додаємо обробник події ValueChanged до кожного NumericInputControls
            c_Input.ValueChanged += OnParameterChanged;
            m_Input.ValueChanged += OnParameterChanged;
            omega_Input.ValueChanged += OnParameterChanged;
            x0_Input.ValueChanged += OnParameterChanged;
            v0_Input.ValueChanged += OnParameterChanged;
            l0_Input.ValueChanged += OnParameterChanged;
        }

        // Обробник, який викликає UpdatePlot при зміні будь-якого параметра
        private void OnParameterChanged(object? sender, EventArgs e)
        {
            if (isSettingDefaults)
            {
                return;
            }

            UpdatePlot();
        }

        // Завантаження зображень
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
                MessageBox.Show($"Не вдалося завантажити зображення. Перевірте шляхи у методі LoadImages().\nПомилка: {ex.Message}", "Помилка завантаження зображень");
            }
        }

        // Обробник кнопки "Значеннь за замовчуванням"
        private void DefaultValuesButton_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultValues();
        }

        // Встановлює параметри Варіанту 4
        private void SetDefaultValues()
        {
            // Завдяки прапорцю OnParameterChanged не спрацює при завантаженні
            isSettingDefaults = true;

            // Варіант 4:
            c_Input.Value = 0.45;
            m_Input.Value = 0.02;
            omega_Input.Value = 2.0 / 3.0;
            x0_Input.Value = 0.1;
            v0_Input.Value = 0.3;
            l0_Input.Value = 0.5;

            isSettingDefaults = false;

            // Оновлюємо графік після встановлення *всіх* значень
            UpdatePlot();
        }

        // Обчислення та оновлення графіка
        private void UpdatePlot()
        {
            // 1. Зчитуємо параметри
            double c = c_Input.Value;
            double m = m_Input.Value;
            double omega = omega_Input.Value;
            double x0 = x0_Input.Value;
            double v0 = v0_Input.Value;
            double l0 = l0_Input.Value;

            // Перевірка на нульову масу, щоб уникнути ділення на нуль
            if (m == 0)
            {
                MessageBox.Show("Маса (m) не може дорівнювати нулю.", "Помилка в параметрах");
                return;
            }

            // 2. Обчислюємо проміжні величини
            double k_squared = (c / m) - Math.Pow(omega, 2);
            double B = (c * l0) / m;
            double k = 0;

            // 3. Готуємо дані для графіка
            int N = 1001; // Кількість точок
            double t_max = 10.0;
            double[] tValues = new double[N];
            double[] xValues = new double[N];

            // 4. Обираємо розв'язок ДР залежно від k^2
            // --- Випадок 1: k^2 > 0 (Коливання) ---
            if (k_squared > 0)
            {
                k = Math.Sqrt(k_squared);
                FrequencyLabel.Text = $"Частота коливань k: {k:F4} рад/с";

                for (int i = 0; i < N; i++)
                {
                    double t = (t_max * i) / (N - 1);
                    tValues[i] = t;
                    xValues[i] = (x0 - B / k_squared) * Math.Cos(k * t) + (v0 / k) * Math.Sin(k * t) + (B / k_squared);
                }
            }
            // --- Випадок 2: k^2 < 0 (Аперіодичний рух) ---
            else if (k_squared < 0)
            {
                FrequencyLabel.Text = "Частота коливань k: - (аперіодичний рух)";
                double k_abs_sqrt = Math.Sqrt(Math.Abs(k_squared));

                double C1 = (x0 / 2.0) + v0 / (2.0 * k_abs_sqrt) - B / (2.0 * k_squared);
                double C2 = (x0 / 2.0) - v0 / (2.0 * k_abs_sqrt) - B / (2.0 * k_squared);

                for (int i = 0; i < N; i++)
                {
                    double t = (t_max * i) / (N - 1);
                    tValues[i] = t;
                    xValues[i] = C1 * Math.Exp(k_abs_sqrt * t) + C2 * Math.Exp(-k_abs_sqrt * t) + (B / k_squared);
                }
            }
            // --- Випадок 3: k^2 = 0 (Критичний випадок) ---
            else // k_squared == 0
            {
                FrequencyLabel.Text = "Частота коливань k: 0 (критичний випадок)";
                for (int i = 0; i < N; i++)
                {
                    double t = (t_max * i) / (N - 1);
                    tValues[i] = t;
                    xValues[i] = (B * Math.Pow(t, 2)) / 2.0 + v0 * t + x0;
                }
            }

            // 5. Оновлюємо ScottPlot
            MainPlot.Plot.Clear(); // Очищуємо попередній графік

            // Додаємо Scatter (точковий графік), але робимо маркери невидимими
            var linePlot = MainPlot.Plot.Add.Scatter(tValues, xValues);

            // --- Ось тут ми встановлюємо товщину та колір лінії ---
            linePlot.LineWidth = 1;
            linePlot.MarkerSize = 0;  // Ховаємо маркери
            linePlot.Color = Colors.Blue;

            // Налаштування осей та назви
            MainPlot.Plot.Title("Графік руху x(t)");
            MainPlot.Plot.XLabel("Час t (c)");
            MainPlot.Plot.YLabel("Положення x (м)");

            // Автоматично налаштовуємо межі осей
            MainPlot.Plot.Axes.AutoScale();

            // Оновлюємо (перемальовуємо) графік
            MainPlot.Refresh();
        }
    }
}