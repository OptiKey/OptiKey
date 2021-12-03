// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class CommuniKateBackgroundColourToForegroundBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color Background = (Color)ColorConverter.ConvertFromString(value.ToString());
            //return new SolidColorBrush(Color.FromRgb((byte)(0xFF - Background.R), (byte)(0xFF - Background.G), (byte)(0xFF - Background.B)));
            int colourSum = Background.R + Background.G + Background.B;
            if (Background.A < 0xFF)
            {
                if (Settings.Default.Theme.ToLower().Contains("light"))
                    colourSum = (colourSum * Background.A + (0xFF - Background.A) * 0xFF * 3) / 0xFF;
                else
                    colourSum = (colourSum * Background.A) / 0xFF;
            }

            if (colourSum < 0x17E) // 0XFF * 3 / 2
                return new SolidColorBrush(Colors.White);
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
