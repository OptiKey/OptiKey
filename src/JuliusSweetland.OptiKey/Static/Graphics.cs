using System;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Common;
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
    }
}
