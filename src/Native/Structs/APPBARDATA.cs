using System;
using System.Runtime.InteropServices;
using JuliusSweetland.OptiKey.Native.Enums;

namespace JuliusSweetland.OptiKey.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public AppBarEdge uEdge;
        public RECT rc;
        public int lParam;
    }
}
