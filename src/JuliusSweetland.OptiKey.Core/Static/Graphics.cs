// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Common.Enums;
using Microsoft.Win32;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Graphics
    {
        static Graphics()
        {
            DisplaySettingsChanged(); // Set initial values
            SystemEvents.DisplaySettingsChanged += new EventHandler((s,e) => { DisplaySettingsChanged(); });
        }

        private static void DisplaySettingsChanged()
        {
            ComputeDpiXandY();
        }

        public static int DpiX { get; private set; }
        public static int DpiY { get; private set; }

        public static void ComputeDpiXandY()
        {
            using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                var desktop = g.GetHdc();
                DpiX = PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSX);
                DpiY = PInvoke.GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSY);
            }
        }

        public static double DipScalingFactorX
        {
            get { return (double)DpiX / (double)96; }
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

        public static double PrimaryScreenWidthInPixels
        {
            get { return SystemParameters.PrimaryScreenWidth * DipScalingFactorX; }
        }

        public static double PrimaryScreenHeightInPixels
        {
            get { return SystemParameters.PrimaryScreenHeight * DipScalingFactorY; }
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
