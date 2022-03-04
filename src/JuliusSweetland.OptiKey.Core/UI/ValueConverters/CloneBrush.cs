// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class CloneBrush : IValueConverter
    {
        public static CloneBrush Instance = new CloneBrush();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Freezable)
            {
                value = (value as Freezable).Clone();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
