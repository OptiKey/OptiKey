using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    class WidthWithoutScrollbarConverter : IValueConverter
    {
        // Subtract the VerticalScrollBarWidth as well as optional extra padding as input parameter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)value;
            
            int extraSpaceRequested = 0;
            if (parameter != null) 
                extraSpaceRequested = int.Parse((string)parameter);

            return width - SystemParameters.VerticalScrollBarWidth - extraSpaceRequested;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
