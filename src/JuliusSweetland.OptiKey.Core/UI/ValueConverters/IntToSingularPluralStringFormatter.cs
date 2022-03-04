// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class IntToSingularPluralStringFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue || parameter == null) return null;

            var valueAsInt = (int)value;
            var stringParam = parameter as string;

            if (stringParam == null || !stringParam.Contains("|")) return null;

            var splitParams = stringParam.Split('|');

            return valueAsInt == 1 || splitParams.Length == 1
                ? string.Format(splitParams[0], value)
                : string.Format(splitParams[1], value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
