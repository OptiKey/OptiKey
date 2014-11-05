using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class KeyDownStatesExtensions
    {
        public static bool IsOnOrLock(this KeyDownStates value)
        {
            return value == KeyDownStates.On || value == KeyDownStates.Lock;
        }
    }
}
