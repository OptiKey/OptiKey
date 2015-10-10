using System;
using System.Collections.Generic;
using System.Text;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;
using JuliusSweetland.OptiKey.Native.Static;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Windows
    {
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static List<Tuple<string, IntPtr>> GetVisibleOverlappedWindows(IntPtr? excludeHWnd)
        {
            IntPtr shellWindow = PInvoke.GetShellWindow();
            var windows = new List<Tuple<string, IntPtr>>();

            PInvoke.EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true; //Skip shell window
                if (excludeHWnd != null && excludeHWnd.Value == hWnd) return true; //Skip exclude window handle (if supplied)

                //Ignore popups, child windows and invisible windows, leaving only overlapped windows
                //https://msdn.microsoft.com/en-us/library/windows/desktop/ms632599(v=vs.85).aspx#overlapped
                var gwlStyle = PInvoke.GetWindowLong(hWnd, (int)GWL.GWL_STYLE);
                if ((gwlStyle & WindowStyles.WS_POPUP) != 0) return true; //Skip popup windows
                if ((gwlStyle & WindowStyles.WS_CHILD) != 0) return true; //Skip child windows
                if ((gwlStyle & WindowStyles.WS_VISIBLE) == 0) return true; //Skip invisible windows
                if ((gwlStyle & WindowStyles.WS_MAXIMIZEBOX) == 0) return true; //Skip windows without Maximise button
                if ((gwlStyle & WindowStyles.WS_MINIMIZE) != 0) return true; //Skip windows which are minimised

                int length = PInvoke.GetWindowTextLength(hWnd);
                if (length == 0) return true;

                var builder = new StringBuilder(length);
                PInvoke.GetWindowText(hWnd, builder, length + 1);

                windows.Add(new Tuple<string, IntPtr>(builder.ToString(), hWnd));
                return true;
            }, 0);

            return windows;
        }
    }
}
