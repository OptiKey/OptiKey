using System;
using System.Drawing;

namespace JuliusSweetland.ETTA.UI.Utilities
{
    class DrawingUtils
    {
        // Given H,S,L in range of 0-1 this returns a Colour (RGB struct) in range of 0-255
        public static ColourRGB HSL2RGB(double h, double sl, double l)
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
            ColourRGB rgb;
            rgb.R = Convert.ToByte(r * 255.0f);
            rgb.G = Convert.ToByte(g * 255.0f);
            rgb.B = Convert.ToByte(b * 255.0f);
            return rgb;
        }
    }

    public struct ColourRGB
    {
        public byte R;
        public byte G;
        public byte B;

        public ColourRGB(Color value)
        {
            R = value.R;
            G = value.G;
            B = value.B;
        }
        public static implicit operator Color(ColourRGB rgb)
        {
            var c = Color.FromArgb(rgb.R, rgb.G, rgb.B);
            return c;
        }
        public static explicit operator ColourRGB(Color c)
        {
            return new ColourRGB(c);
        }
    }
}
