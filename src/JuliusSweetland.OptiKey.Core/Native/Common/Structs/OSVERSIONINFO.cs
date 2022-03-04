// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Runtime.InteropServices;

namespace JuliusSweetland.OptiKey.Native.Common.Structs
{
    public struct OSVERSIONINFO 
    { 
        public int dwOSVersionInfoSize; 
        public int dwMajorVersion; 
        public int dwMinorVersion; 
        public int dwBuildNumber; 
        public int dwPlatformId; 
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] 
        public string szCSDVersion; 
    }
}
