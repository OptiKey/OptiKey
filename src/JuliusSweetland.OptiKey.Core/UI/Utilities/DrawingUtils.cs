// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Drawing;

namespace JuliusSweetland.OptiKey.UI.Utilities
{
    internal static class DrawingUtils
    {
        // Given H,S,L in range of 0-1 this returns a Colour (RGB struct) in range of 0-255
        public static Color HSL2RGB(double h, double sl, double l)
        {
            var r = l;
            var g = l;
            var b = l;
            var v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
            
            if (v > 0)
            {
                var m = l + l - v;
                var sv = (v - m) / v;
                
                h *= 6.0;
                
                var sextant = (int)h;
                var fract = h - sextant;
                var vsf = v * sv * fract;
                var mid1 = m + vsf;
                var mid2 = v - vsf;
                
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            
            var red = Convert.ToByte(r * 255.0f);
            var green = Convert.ToByte(g * 255.0f);
            var blue = Convert.ToByte(b * 255.0f);
            return Color.FromArgb(red, green, blue);
        }
    }
}
