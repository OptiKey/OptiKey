using System;
using System.Globalization;
using System.Windows.Data;
using System.IO;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class IsSvgImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string image = value.ToString();
            if (!File.Exists(image))
                return false;
            return Path.GetExtension(image).ToLower().EndsWith("svg");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
