using System;
using System.Windows;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Common.Enums;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Graphics
    {
        public static int DpiX
        {
            get
            {
                using(var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    var desktop = g.GetHdc();
                    return PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSX);
                }
            }
        }
        
        public static double DipScalingFactorX
        {
            get { return (double)DpiX / (double)96; }
        }

        public static int DpiY
        {
            get
            {
                using(var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    var desktop = g.GetHdc();
                    return PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY);
                }
            }
        }
        
        public static double DipScalingFactorY
        {
            get { return (double)DpiY / (double)96; }
        }

        public static double VirtualScreenWidthInPixels
        {
            get { return SystemParameters.VirtualScreenWidth * DipScalingFactorX; }
        }

        public static double VirtualScreenHeightInPixels
        {
            get { return SystemParameters.VirtualScreenHeight * DipScalingFactorY; }
        }

        public static Rect DipsToPixels(Rect bounds)
        {
            bounds.Scale(DipScalingFactorX, DipScalingFactorY);
            return bounds;
        }

        public static Rect PixelsToDips(Rect bounds)
        {
            bounds.Scale(1.0 / DipScalingFactorX, 1.0 / DipScalingFactorY);
            return bounds;
        }

        public static Thickness PixelsToDips(Thickness thickness)
        {
            thickness.Left /= DipScalingFactorX;
            thickness.Right /= DipScalingFactorX;
            thickness.Top /= DipScalingFactorY;
            thickness.Bottom /= DipScalingFactorY;

            return thickness;
        }
    }
}
