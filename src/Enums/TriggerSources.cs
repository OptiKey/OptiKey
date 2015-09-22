using JuliusSweetland.OptiKey.Properties;
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
                case TriggerSources.Fixations: return Resources.FixationsDwell;
                case TriggerSources.KeyboardKeyDownsUps: return Resources.KeyboardKey;
                case TriggerSources.MouseButtonDownUps: return Resources.MouseButton;
            }

            return triggerSources.ToString();
        }
    }
}
