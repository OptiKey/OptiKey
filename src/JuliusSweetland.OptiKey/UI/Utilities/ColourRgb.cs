using System.Drawing;

namespace JuliusSweetland.OptiKey.UI.Utilities
{
    public struct ColourRgb
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public ColourRgb(byte red, byte green, byte blue)
        {
            R = red;
            G = green;
            B = blue;
        }

        private ColourRgb(Color value)
        {
            R = value.R;
            G = value.G;
            B = value.B;
        }

        public static implicit operator Color(ColourRgb rgb)
        {
            var c = Color.FromArgb(rgb.R, rgb.G, rgb.B);
            return c;
        }
        public static explicit operator ColourRgb(Color c)
        {
            return new ColourRgb(c);
        }
    }
}