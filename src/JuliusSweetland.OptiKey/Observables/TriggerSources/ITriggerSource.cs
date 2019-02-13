using System;
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
    }
}
