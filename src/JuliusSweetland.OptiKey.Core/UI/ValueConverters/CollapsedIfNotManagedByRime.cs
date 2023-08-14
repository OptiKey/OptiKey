using System;
using System.Windows.Data;
using System.Globalization;
using JuliusSweetland.OptiKey.Enums;
using System.Windows;
using System.Diagnostics;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class CollapsedIfNotManagedByRime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (Enum.TryParse(value.ToString(), out Languages language))
                {
                    if (language.ManagedByRime())
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }                
            }

            Debug.Assert(false, "Receiving an invalid language as input");
            return Visibility.Collapsed;        
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
