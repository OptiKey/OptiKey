using System;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.PointAndKeyValueSources
{
    public interface IPointAndKeyValueSource
    {
        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        IObservable<Timestamped<PointAndKeyValue?>> Sequence { get; }
    }
}
