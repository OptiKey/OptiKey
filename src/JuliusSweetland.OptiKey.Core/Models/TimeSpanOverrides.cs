// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;

namespace JuliusSweetland.OptiKey.Models
{
    public class TimeSpanOverrides
    {
        public TimeSpan LockOnTime { get; set; }
        public TimeSpan CompletionTime { get; set; }
        public TimeSpan RepeatDelay { get; set; }
        public TimeSpan RepeatRate { get; set; }
    }
}