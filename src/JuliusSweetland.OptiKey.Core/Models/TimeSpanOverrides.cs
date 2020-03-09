// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Models
{
    public class TimeSpanOverrides
    {
        public TimeSpan LockOnTime { get; set; }
        public List<string> CompletionTimes { get; set; }
    }
}