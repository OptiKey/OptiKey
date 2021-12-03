// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class KeyDownStatesExtensions
    {
        public static bool IsDownOrLockedDown(this KeyDownStates value)
        {
            return value == KeyDownStates.Down || value == KeyDownStates.LockedDown;
        }
    }
}
