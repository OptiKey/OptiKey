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
                case TriggerStopSignals.NextHigh: return "Next hight";
                case TriggerStopSignals.NextLow: return "Next low";
            }

            return triggerStopSignal.ToString();
        }
    }
}
