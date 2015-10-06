using System;
using System.Globalization;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class IntToSingularPluralStringFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueAsInt = (int)value;
            var stringParam = parameter as string;
            var splitParams = stringParam.Split('|');

            return valueAsInt == 1
                ? string.Format(splitParams[0], value)
                : string.Format(splitParams[1], value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
