// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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

        public static double Clamp(this double value, double lowerLimit, double upperLimit)
        {
            return Math.Min(Math.Max(value, lowerLimit), upperLimit);
        }
    }
}
