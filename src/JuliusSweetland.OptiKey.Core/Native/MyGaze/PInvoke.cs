// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Runtime.InteropServices;
using JuliusSweetland.OptiKey.Native.MyGaze.Structs;

namespace JuliusSweetland.OptiKey.Native.MyGaze
{
    public static class PInvoke
    {
        [DllImport("myGazeAPI.dll", EntryPoint = "iV_Connect")]
        public static extern int Connect();

        [DllImport("myGazeAPI.dll", EntryPoint = "iV_GetSystemInfo")]
        public static extern int GetSystemInfo(ref SystemInfoStruct systemInfo);

        [DllImport("myGazeAPI.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetSampleCallback")]
		public static extern void SetSampleCallback(MulticastDelegate sampleCallbackFunction);

        [DllImport("myGazeAPI.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetEventCallback")]
        public static extern void SetEventCallback(MulticastDelegate eventCallbackFunction);

        [DllImport("myGazeAPI.dll", EntryPoint = "iV_Calibrate")]
        public static extern int Calibrate();

        [DllImport("myGazeAPI.dll", EntryPoint = "iV_Disconnect")]
        public static extern int Disconnect();
    }
}
