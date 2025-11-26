using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab1MathMod
{
    public partial class NumericInputControl : UserControl
    {
        // Подія, яка виникає при зміні значення
        public event EventHandler? ValueChanged;

        // Властивість DependencyProperty для Label
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(NumericInputControl), new PropertyMetadata("Label", OnLabelChanged));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericInputControl)d).LabelText.Text = (string)e.NewValue;
        }

        // Властивість DependencyProperty для Value
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumericInputControl), new PropertyMetadata(0.0, OnValueChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericInputControl)d;
            control.ValueTextBox.Text = ((double)e.NewValue).ToString(control.Format, CultureInfo.InvariantCulture);
            control.ValueChanged?.Invoke(control, EventArgs.Empty);
        }

        // Властивість DependencyProperty для Step
        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(double), typeof(NumericInputControl), new PropertyMetadata(1.0));

        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        // Властивість DependencyProperty для Format
        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register("Format", typeof(string), typeof(NumericInputControl), new PropertyMetadata("F2")); // F2 - 2 знаки після коми

        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public NumericInputControl()
        {
            InitializeComponent();
            ValueTextBox.Text = Value.ToString(Format, CultureInfo.InvariantCulture);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Value += Step;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Value -= Step;
        }

        private void ValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ParseTextBox();
                // Втрачаємо фокус, щоб спрацював LostFocus (для оновлення)
                Keyboard.ClearFocus();
            }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ParseTextBox();
        }

        private void ParseTextBox()
        {
            // Використовуємо CultureInfo.InvariantCulture для коректного парсингу
            if (double.TryParse(ValueTextBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                Value = result;
            }
            else
            {
                // Якщо введено некоректне значення, повертаємо текст до поточного Value
                ValueTextBox.Text = Value.ToString(Format, CultureInfo.InvariantCulture);
            }
        }
    }
}