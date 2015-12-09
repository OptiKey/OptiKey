using System;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public interface ITriggerSource
    {
        RunningStates State { get; set; }
        IObservable<TriggerSignal> Sequence { get; }
    }
}
