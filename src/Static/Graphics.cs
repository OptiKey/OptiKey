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

        public static int DpiY
        {
            get
            {
                var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                return PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY);
            }
        }

        public static int DpiXAltMethod
        {
            get
            {
                //N.B. This does NOT work for me, but is recommended here; http://stackoverflow.com/questions/5977445/how-to-get-windows-display-settings
                var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                int logicalScreenWidth = PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.HORZRES);
                int physicalScreenWidth = PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPHORZRES);
                float screenScalingFactor = (float)physicalScreenWidth / (float)logicalScreenWidth; // 1.25 = 125%
                return Convert.ToInt32(screenScalingFactor * 96); 
            }
        }

        public static int DpiYAltMethod
        {
            get
            {
                //N.B. This does NOT work for me, but is recommended here; http://stackoverflow.com/questions/5977445/how-to-get-windows-display-settings
                var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                int logicalScreenHeight = PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
                int physicalScreenHeight = PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                float screenScalingFactor = (float)physicalScreenHeight / (float)logicalScreenHeight; // 1.25 = 125%
                return Convert.ToInt32(screenScalingFactor * 96); 
            }
        }
    }
}
