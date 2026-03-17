using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ControleSoxUI.ViewModels;

namespace ControleSoxUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }

    /// <summary>
    /// Converter para convertir bool a ancho del panel lateral
    /// </summary>
    public class BoolToWidthConverter : IValueConverter
    {
        public double CollapsedWidth { get; set; } = 60;
        public double ExpandedWidth { get; set; } = 280;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOpen)
            {
                return isOpen ? ExpandedWidth : CollapsedWidth;
            }
            return CollapsedWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}