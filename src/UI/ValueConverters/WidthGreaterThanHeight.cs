using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class WidthGreaterThanHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var frameworkElement = value as FrameworkElement;
            if (frameworkElement != null)
            {
                return frameworkElement.ActualWidth >= frameworkElement.ActualHeight;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
