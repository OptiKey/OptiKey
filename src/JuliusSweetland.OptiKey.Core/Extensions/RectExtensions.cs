// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class RectExtensions
    {
        public static double CalculateArea(this Rect rect)
        {
            return rect.Width * rect.Height;
        }

        public static Point CalculateCentre(this Rect rect)
        {
            return new Point
            {
                X = (rect.Left + rect.Right) / 2,
                Y = (rect.Top + rect.Bottom) / 2,
            };
        }

        /// <summary>
        /// Calculates the distances from each edge of a Rect to each edge of another Rect that presumably lies
        /// within it. If that is not the case, the margins may be negative or exceed the size of the Rect itself.
        /// </summary>
        /// <param name="outer">the "outer" rectangle</param>
        /// <param name="inner">the "inner" rectangle</param>
        /// <returns>
        /// the left, right, top, and bottom distances between the two Rects
        /// </returns>
        public static Thickness CalculateMarginsAround(this Rect outer, Rect inner)
        {
            return new Thickness
            {
                Left = inner.Left - outer.Left,
                Right = outer.Right - inner.Right,
                Top = inner.Top - outer.Top,
                Bottom = outer.Bottom - inner.Bottom,
            };
        }
    }
}
