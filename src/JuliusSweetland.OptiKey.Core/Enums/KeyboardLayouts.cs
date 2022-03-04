// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Globalization;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum KeyboardLayouts
    {
        Default,
        Alphabetic,
        Communikate,
        Simplified
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this KeyboardLayouts layout)
        {
            switch (layout)
            {
                case KeyboardLayouts.Default: return Resources.USE_DEFAULT_KEYBOARD_LAYOUT;
                case KeyboardLayouts.Alphabetic: return Resources.USE_ALPHABETICAL_KEYBOARD_LAYOUT;
                case KeyboardLayouts.Communikate: return Resources.USE_COMMUNIKATE_KEYBOARD_LAYOUT;
                case KeyboardLayouts.Simplified: return Resources.USE_SIMPLIFIED_KEYBOARD_LAYOUT;
            }

            return layout.ToString();
        }
    }
}
