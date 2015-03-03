using System;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Graphics
    {
        public static int DpiX
        {
            get
            {
                var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                return PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSX);
            }
        }
        
        public static int ScalingFactorX
        {
            get { return DpiX / 96; }
        }

        public static int DpiY
        {
            get
            {
                var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                return PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY);
            }
        }
        
        public static int ScalingFactorY
        {
            get { return DpiY / 96; }
        }
    }
}
