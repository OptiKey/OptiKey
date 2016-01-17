using System.Drawing;

namespace JuliusSweetland.OptiKey.UI.Utilities
{
    public struct ColourRgb
    {
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public ColourRgb(byte red, byte green, byte blue) : this()
        {
            R = red;
            G = green;
            B = blue;
        }

        private ColourRgb(Color value) : this(value.R, value.G, value.B) { }

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