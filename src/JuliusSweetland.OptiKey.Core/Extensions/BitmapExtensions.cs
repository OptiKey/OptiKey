// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class BitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this System.Drawing.Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp); //Use Png if you need to retain transparency
                ms.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
