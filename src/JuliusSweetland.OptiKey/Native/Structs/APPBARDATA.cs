using System;
using System.Runtime.InteropServices;
using JuliusSweetland.OptiKey.Native.Enums;

namespace JuliusSweetland.OptiKey.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public AppBarEdge uEdge;
        public RECT rc;
        public int lParam;
    }
}
