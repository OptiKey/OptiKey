using System;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.PointSources
{
    public interface IPointSource
    {
        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        IObservable<Timestamped<PointAndKeyValue?>> Sequence { get; }
    }
}
