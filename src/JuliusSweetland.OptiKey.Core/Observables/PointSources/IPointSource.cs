// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Observables.PointSources
{
    public interface IPointSource
    {
        RunningStates State { get; set; }
        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        IObservable<Timestamped<PointAndKeyValue>> Sequence { get; }
    }
}
