// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class SuggestionsToKeyValue: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null
                && values.Count() == 2
                && values.All(v => v != DependencyProperty.UnsetValue))
            {
                var suggestions = values[0] as List<string>;
                var index = (int) values[1];

                if (suggestions != null)
                {   
                    if (suggestions.Count > index)
                    {
                        return new KeyValue(suggestions[index]);
                    }
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
