using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;

namespace JuliusSweetland.ETTA.UI.ValueConverters
{
    public class KeyboardDataTemplateCriteriaCreator : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null
                && values.Length == 2
                && values.All(v => v != null))
            {
                var language = (Languages)Enum.Parse(typeof(Languages), values[0].ToString());
                var keyboard = values[1] as IKeyboard;

                return new KeyboardDataTemplateCriteria
                {
                    Language = language, 
                    Keyboard = keyboard
                };
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
