using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class ConvertToStarGridLength : IValueConverter
    {
        public int DefaultGridLength { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueAsDouble = value != null && value != DependencyProperty.UnsetValue
                ? (int)value
                : DefaultGridLength;

            if (valueAsDouble <= 0)
            {
                valueAsDouble = DefaultGridLength;
            }

            return new GridLength((double)valueAsDouble, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
