// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public interface ITriggerSource
    {
        RunningStates State { get; set; }
        IObservable<TriggerSignal> Sequence { get; }
        IPointSource PointSource { get; set; }
        IDictionary<KeyValue, TimeSpanOverrides> OverrideTimesByKey { get; }
    }
}
