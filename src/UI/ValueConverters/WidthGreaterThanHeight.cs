using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class WidthGreaterThanHeight : IMultiValueConverter
    {
        private bool defaultValue = true;

        public bool DefaultValue { get { return defaultValue; } set { defaultValue = value; } }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2
                && values[0] != DependencyProperty.UnsetValue
                && values[1] != DependencyProperty.UnsetValue)
            {
                var width = (double)values[0];
                var height = (double)values[1];
                return width >= height;    
            }

            return DefaultValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
