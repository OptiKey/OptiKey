using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.ETTA.UI.ValueConverters
{
    public class LengthDividedBySpan : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Count() == 2)
            {
                var length = (double)values[0];
                var span = (int)values[1];

                var singleUnit = length/span;

                double parameterAsDouble;
                if(double.TryParse((string)parameter, out parameterAsDouble))
                {
                    singleUnit = singleUnit - parameterAsDouble;
                }

                return singleUnit;
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
