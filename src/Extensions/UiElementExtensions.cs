using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class UiElementExtensions
    {
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
    }
}
