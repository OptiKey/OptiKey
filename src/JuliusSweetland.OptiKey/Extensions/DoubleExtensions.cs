using System;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class DoubleExtensions
    {
        public static double CoerceToUpperLimit(this double value, double upperLimit)
        {
            return Math.Min(value, upperLimit);
        }

        public static double CoerceToLowerLimit(this double value, double lowerLimit)
        {
            return Math.Max(value, lowerLimit);
        }
    }
}
