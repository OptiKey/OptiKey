// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;

namespace JuliusSweetland.OptiKey.Models
{
    public class TimeSpanOverrides
    {
        public TimeSpanOverrides() { }
        public TimeSpanOverrides(TimeSpan lockOnTime, TimeSpan completionTime, TimeSpan repeatDelay, TimeSpan repeatRate)
        {
            this.LockOnTime = lockOnTime;
            this.CompletionTime = completionTime;
            this.RepeatDelay = repeatDelay;
            this.RepeatRate = repeatRate;
        }

        public TimeSpan LockOnTime { get; set; }
        public TimeSpan CompletionTime { get; set; }
        public TimeSpan RepeatDelay { get; set; }
        public TimeSpan RepeatRate { get; set; }
    }
}