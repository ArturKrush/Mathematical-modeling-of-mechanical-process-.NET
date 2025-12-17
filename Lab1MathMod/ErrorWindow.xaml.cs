using System.Collections.Generic;
using System.Windows;

namespace Lab1MathMod
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(List<CalculationResult> results)
        {
            InitializeComponent();
            // Прив'язуємо дані до таблиці
            ErrorGrid.ItemsSource = results;
        }
    }
}