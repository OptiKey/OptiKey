// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class SuggestionsPaged : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null
                && values.Count() == 4
                && values.All(v => v != DependencyProperty.UnsetValue))
            {
                var suggestions = values[0] as List<string>;
                var suggestionsPage = (int) values[1];
                var suggestionsPerPage = (int) values[2];
                var suggestionIndex = (int) values[3];

                if (suggestions != null)
                {
                    var index = (suggestionsPage * suggestionsPerPage) + suggestionIndex;

                    if (suggestions.Count > index)
                    {
                        return suggestions[index];
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
