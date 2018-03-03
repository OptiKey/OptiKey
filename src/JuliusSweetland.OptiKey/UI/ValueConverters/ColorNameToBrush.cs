using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class ColorNameToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Transparent;

            if (value != null)
            {
                string colorName = value.ToString();
                object colorObj = ColorConverter.ConvertFromString(colorName);

                if (colorObj != null)
                {
                    brush = new SolidColorBrush((Color)colorObj);
                }
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
