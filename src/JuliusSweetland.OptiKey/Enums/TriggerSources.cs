// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
                case TriggerSources.Fixations: return Resources.FIXATIONS_DWELL;
                case TriggerSources.KeyboardKeyDownsUps: return Resources.KEYBOARD_KEY;
                case TriggerSources.MouseButtonDownUps: return Resources.MOUSE_BUTTON;
            }

            return triggerSources.ToString();
        }
    }
}
