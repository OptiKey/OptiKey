using System;
using System.Collections.Generic;
using System.Text;
using JuliusSweetland.OptiKey.Native;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Windows
    {
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, string> GetOpenWindows(IntPtr? excludeHWnd)
        {
            IntPtr shellWindow = PInvoke.GetShellWindow();
            var windows = new Dictionary<IntPtr, string>();

            PInvoke.EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!PInvoke.IsWindowVisible(hWnd)) return true;
                if (excludeHWnd != null && excludeHWnd.Value == hWnd) return true;

                int length = PInvoke.GetWindowTextLength(hWnd);
                if (length == 0) return true;

                var builder = new StringBuilder(length);
                PInvoke.GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;
            }, 0);

            return windows;
        }
    }
}
