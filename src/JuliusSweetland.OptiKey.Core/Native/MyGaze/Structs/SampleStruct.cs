// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;

namespace JuliusSweetland.OptiKey.Native.MyGaze.Structs
{
    public struct SampleStruct
    {
        public Int64 timestamp;
        public EyeDataStruct leftEye;
        public EyeDataStruct rightEye;
    };
}
