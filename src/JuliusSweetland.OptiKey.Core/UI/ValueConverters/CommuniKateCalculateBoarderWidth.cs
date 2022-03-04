// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class CommuniKateCalculateBoarderWidth : IMultiValueConverter
    {
        public int DefaultGridLength { get; set; }
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
            {
                return (double)8;
            }

            int i = Math.Min((int)values[0], (int)values[1]);

            if (i > 4) {
                i = 4;
            } else if (i > 1) {
                i--;
            }

            if (i <= 0)
            {
                return (double)8;
            }

            return (double)8/i;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
