using JuliusSweetland.OptiKey.Properties;
namespace JuliusSweetland.OptiKey.Enums
{
    public enum TriggerStopSignals
    {
        NextHigh,
        NextLow
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this TriggerStopSignals triggerStopSignal)
        {
            switch (triggerStopSignal)
            {
                case TriggerStopSignals.NextHigh: return Resources.NextHight;
                case TriggerStopSignals.NextLow: return Resources.NextLow;
            }

            return triggerStopSignal.ToString();
        }
    }
}
