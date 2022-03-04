// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class UiElementExtensions
    {
        /// <summary>
        /// Returns the transformation matrix which converts WPF resolution independent co-ords to device (screen) co-ords
        /// </summary>
        public static Matrix GetTransformToDevice(this UIElement element)
        {
            //http://stackoverflow.com/questions/3286175/how-do-i-convert-a-wpf-size-to-physical-pixels
            Matrix transformToDevice;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
            {
                transformToDevice = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var altSource = new HwndSource(new HwndSourceParameters()))
                {
                    transformToDevice = altSource.CompositionTarget.TransformToDevice;
                }
            }

            return transformToDevice;
        }

        /// <summary>
        /// Returns the transformation matrix which converts device (screen) co-ords to WPF resolution independent co-ords
        /// </summary>
        public static Matrix GetTransformFromDevice(this UIElement element)
        {
            //http://stackoverflow.com/questions/3286175/how-do-i-convert-a-wpf-size-to-physical-pixels
            Matrix transformFromDevice;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
            {
                transformFromDevice = source.CompositionTarget.TransformFromDevice;
            }
            else
            {
                using (var altSource = new HwndSource(new HwndSourceParameters()))
                {
                    transformFromDevice = altSource.CompositionTarget.TransformFromDevice;
                }
            }

            return transformFromDevice;
        }
    }
}
