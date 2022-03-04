// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
            if (string.IsNullOrEmpty(image))
                return false;
            if (!File.Exists(image))
                return false;
            if (new FileInfo(image).Length == 0)
                return false;
            return Path.GetExtension(image).ToLower().EndsWith("svg");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
