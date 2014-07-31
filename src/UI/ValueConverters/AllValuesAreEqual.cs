using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JuliusSweetland.ETTA.UI.ValueConverters
{
    public class AllValuesAreEqual : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values != null && values.Distinct().Count() == 1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
