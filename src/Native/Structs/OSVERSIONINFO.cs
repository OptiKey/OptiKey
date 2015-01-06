using System.Runtime.InteropServices;

namespace JuliusSweetland.ETTA.Native.Structs
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
