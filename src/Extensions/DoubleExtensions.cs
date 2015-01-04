namespace JuliusSweetland.ETTA.Extensions
{
    public static class DoubleExtensions
    {
        public static double CoerceToUpperLimit(this double value, double upperLimit)
        {
            return value > upperLimit ? upperLimit : value;
        }

        public static double CoerceToLowerLimit(this double value, double lowerLimit)
        {
            return value < lowerLimit ? lowerLimit : value;
        }
    }
}
