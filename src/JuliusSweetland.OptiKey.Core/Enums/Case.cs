// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
namespace JuliusSweetland.OptiKey.Enums
{
    // Case transformation applied to strings before displaying them on keys.
    public enum Case
    {
        // All upper
        Upper,
        // All lower
        Lower,
        // Only first letter is upper, others are lower
        Title,
        // No transformation
        None,
        // Special value used to indicate that settings value must be used as default
        Settings
    }
}
