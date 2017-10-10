using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class ConvertBoolToStarGridLength : IValueConverter
    {
        public int DefaultGridLength { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueAsDouble = (bool)value == true
                ? DefaultGridLength
                : 0;

            return new GridLength((double)valueAsDouble, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
