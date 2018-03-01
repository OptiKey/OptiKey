using System.Windows;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class RectExtensions
    {
        public static double CalculateArea(this Rect rect)
        {
            return rect.Width * rect.Height;
        }

        public static Point CalculateCenter(this Rect rect)
        {
            return new Point
            {
                X = (rect.Left + rect.Right) / 2,
                Y = (rect.Top + rect.Bottom) / 2,
            };
        }
    }
}
