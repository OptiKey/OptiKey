using System;
using System.Globalization;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class IsMenuKey : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeyValue key = (KeyValue)value;
            if (String.IsNullOrEmpty(key.String))
                return false;
            return key.String.Contains(":action:board:");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
