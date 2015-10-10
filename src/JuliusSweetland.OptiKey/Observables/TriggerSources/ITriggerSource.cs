using System;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public interface ITriggerSource
    {
        IObservable<TriggerSignal> Sequence { get; }
    }
}
