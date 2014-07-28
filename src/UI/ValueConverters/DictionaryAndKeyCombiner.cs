using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JuliusSweetland.ETTA.UI.ValueConverters
{
    public class DictionaryAndKeyCombiner : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null
                && values.Count() == 2)
            {
                var dictionary = values[0] as IDictionary;
                var key = values[1];

                if (dictionary != null)
                {
                    return dictionary[key];
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
