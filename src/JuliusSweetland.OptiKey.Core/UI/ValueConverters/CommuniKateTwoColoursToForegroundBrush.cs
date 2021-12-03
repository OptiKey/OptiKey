// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class CommuniKateTwoColoursToForegroundBrush : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Color Border = (Color)ColorConverter.ConvertFromString(values[0].ToString());
            int colourSum = Border.R + Border.G + Border.B;
            //*
            if (Border.A < 0xFF)
            {
                Color Background = (Color)ColorConverter.ConvertFromString(values[1].ToString());
                int colourSumBackground = Background.R + Background.G + Background.B;
                if (Background.A < 0xFF)
                {
                    if (Settings.Default.Theme.ToLower().Contains("light"))
                        colourSumBackground = (colourSumBackground * Background.A + (0xFF - Background.A) * 0xFF * 3) / 0xFF;
                    else
                        colourSumBackground = (colourSumBackground * Background.A) / 0xFF;
                }
                colourSum = (colourSum * Border.A + (0xFF - Border.A) * colourSumBackground) / 0xFF;
            }
            //*/
            if (colourSum < 0x17E) // 0XFF * 3 / 2
                return new SolidColorBrush(Colors.White);
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
