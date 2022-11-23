// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Models;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public interface IFixationTriggerSource : ITriggerSource
    {
        KeyEnabledStates KeyEnabledStates { set; }
        IDictionary<KeyValue, TimeSpanOverrides> OverrideTimesByKey { get; }
        bool AllowPointsOverKeys { get; set; }
    }
}
