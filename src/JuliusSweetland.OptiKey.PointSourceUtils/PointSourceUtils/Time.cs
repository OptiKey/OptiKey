// Copyright (c) 2023 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System;
using System.Runtime.InteropServices;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Time
    {
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static DateTime HighResolutionUtcNow
        {
            get
            {
                try
                {
                    long filetime;
                    GetSystemTimePreciseAsFileTime(out filetime);
                    return DateTime.FromFileTimeUtc(filetime);
                }
                catch (EntryPointNotFoundException)
                {
                    // GetSystemTimePreciseAsFileTime is available from Windows 8+
                    // Fall back to lower resolution alternative
                    return DateTime.UtcNow;
                }
            }
        }
    }
}
