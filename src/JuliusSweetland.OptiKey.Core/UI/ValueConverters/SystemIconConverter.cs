// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class SystemIconConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = value as Icon;
            if (icon == null)
            {
                return null;
            }

            ImageSource originalImageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Calculate scale
            int desiredHeight = 50;
            int desiredWidth = 50;

            double scaleX = (double)desiredWidth / originalImageSource.Width;
            double scaleY = (double)desiredHeight / originalImageSource.Height;
            ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY);

            TransformedBitmap resizedImageSource = new TransformedBitmap((BitmapSource)originalImageSource, scaleTransform);

            return resizedImageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
