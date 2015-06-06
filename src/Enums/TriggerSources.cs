namespace JuliusSweetland.OptiKey.Enums
{
    public enum TriggerSources
    {
        Fixations,
        KeyboardKeyDownsUps,
        MouseButtonDownUps
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this TriggerSources triggerSources)
        {
            switch (triggerSources)
            {
                case TriggerSources.Fixations: return "Fixations (dwell)";
                case TriggerSources.KeyboardKeyDownsUps: return "Keyboard key";
                case TriggerSources.MouseButtonDownUps: return "Mouse button";
            }

            return triggerSources.ToString();
        }
    }
}
