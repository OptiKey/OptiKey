// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;

namespace JuliusSweetland.OptiKey.Native.Common.Enums
{
    [Flags]
    public enum ExtendedWindowStyles : uint
    {
        WS_EX_NOACTIVATE          = 0x08000000,
        WS_EX_TRANSPARENT         = 0x00000020,
        WS_EX_TOOLWINDOW          = 0x00000080,

        // Other members omitted for brevity. The complete list can be found at:
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ff700543(v=vs.85).aspx
    }
}
