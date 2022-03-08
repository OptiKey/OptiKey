// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Common.Enums;
using JuliusSweetland.OptiKey.Native.Common.Static;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Windows
    {
        // UWP apps have a top-level container window that can be present before the app is actually launched
        // and can stick around after the app is "closed". This window's CoreWindow children, if any, are what 
        // one thinks of as the actual top-level windows and only show up when the app is "running".
        private const string UWPTopLevelWindowClassName = "ApplicationFrameWindow";
        private const string UWPCoreWindowClassName = "Windows.UI.Core.CoreWindow";

        public static long GetWindowStyle(IntPtr hWnd)
        {
            return PInvoke.GetWindowLong(hWnd, (int)GWL.GWL_STYLE);
        }

        public static ExtendedWindowStyles GetExtendedWindowStyle(IntPtr hWnd)
        {
            return (ExtendedWindowStyles)PInvoke.GetWindowLong(hWnd, (int)GWL.GWL_EXSTYLE);
        }

        public static void SetExtendedWindowStyle(IntPtr hWnd, ExtendedWindowStyles exStyle)
        {
            PInvoke.SetWindowLong(hWnd, (int)GWL.GWL_EXSTYLE, (int)exStyle);
        }

        public static List<IntPtr> GetHandlesOfTopLevelWindows()
        {
            var hWnds = new List<IntPtr>();

            PInvoke.EnumWindows(delegate (IntPtr hWnd, int lParam) 
            {
                hWnds.Add(hWnd);
                return true; // Keep enumerating.
            }, 0);

            return hWnds;
        }

        public static List<IntPtr> GetHandlesOfChildWindows(IntPtr hWndParent)
        {
            var hWnds = new List<IntPtr>();

            PInvoke.EnumChildWindows(hWndParent, delegate (IntPtr hWnd, int lParam)
            {
                hWnds.Add(hWnd);
                return true; // Keep enumerating.
            }, 0);

            return hWnds;
        }

        public static bool IsWindowOfClass(IntPtr hWnd, string className)
        {
            var builder = new StringBuilder(className.Length);

            if (PInvoke.GetClassName(hWnd, builder, className.Length + 1) > 0)
            {
                return string.Equals(className, builder.ToString());
            }
            else
            {
                return false;
            }
        }

        public static bool IsUWPTopLevelWindow(IntPtr hWnd)
        {
            return IsWindowOfClass(hWnd, UWPTopLevelWindowClassName);
        }

        public static bool IsUWPCoreWindow(IntPtr hWnd)
        {
            return IsWindowOfClass(hWnd, UWPCoreWindowClassName);
        }

        public static List<IntPtr> ReplaceUWPTopLevelWindowsWithCoreWindowChildren(IEnumerable<IntPtr> hWnds)
        {
            return hWnds.SelectMany(hWnd => 
            {
                if (IsUWPTopLevelWindow(hWnd))
                {
                    return GetHandlesOfChildWindows(hWnd).Where(hWndChild => IsUWPCoreWindow(hWndChild));
                }
                else
                {
                    return new[] { hWnd };
                }
            }).ToList();
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            int length = PInvoke.GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;

            var builder = new StringBuilder(length);
            PInvoke.GetWindowText(hWnd, builder, length + 1);

            return builder.ToString();
        }

        /// <summary>Returns a list that contains the handle and title of all the visible overlapped windows.</summary>
        /// <returns>A list that contains the handle and title of all the visible overlapped windows.</returns>
        public static List<Tuple<string, IntPtr>> GetVisibleOverlappedWindows(IntPtr? excludeHWnd)
        {
            IntPtr shellWindow = PInvoke.GetShellWindow();

            return GetHandlesOfTopLevelWindows()
                .Where(hWnd => 
                {
                    if (hWnd == shellWindow) return false; //Skip shell window
                    if (excludeHWnd != null && excludeHWnd.Value == hWnd) return false; //Skip exclude window handle (if supplied)

                    //Ignore popups, child windows and invisible windows, leaving only overlapped windows
                    //https://msdn.microsoft.com/en-us/library/windows/desktop/ms632599(v=vs.85).aspx#overlapped
                    var gwlStyle = GetWindowStyle(hWnd);
                    if ((gwlStyle & WindowStyles.WS_POPUP) != 0) return false; //Skip popup windows
                    if ((gwlStyle & WindowStyles.WS_CHILD) != 0) return false; //Skip child windows
                    if ((gwlStyle & WindowStyles.WS_VISIBLE) == 0) return false; //Skip invisible windows
                    if ((gwlStyle & WindowStyles.WS_MAXIMIZEBOX) == 0) return false; //Skip windows without Maximise button
                    if ((gwlStyle & WindowStyles.WS_MINIMIZE) != 0) return false; //Skip windows which are minimised

                    return true;
                })
                .Select(hWnd => new Tuple<string, IntPtr>(GetWindowTitle(hWnd), hWnd))
                .Where(tuple => !string.IsNullOrEmpty(tuple.Item1))
                .ToList();
        }

        public static IntPtr GetFrontmostWindow(List<IntPtr> hWnds)
        {
            var frontmostHWnd = IntPtr.Zero;

            if (hWnds.Any())
            {
                var hWndSet = new HashSet<IntPtr>(hWnds);
                IntPtr hWnd = hWnds.First();

                do
                {
                    if (hWndSet.Contains(hWnd))
                    {
                        frontmostHWnd = hWnd;
                    }

                    // The previous window in the Z-order is the one in front.
                    hWnd = PInvoke.GetWindow(hWnd, (uint)GetWindowCommand.GW_HWNDPREV);
                }
                while (hWnd != IntPtr.Zero);
            }

            return frontmostHWnd;
        }
    }
}
