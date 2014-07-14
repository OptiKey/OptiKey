using System;
using JuliusSweetland.ETTA.Model;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    public interface ITriggerSignalSource
    {
        IObservable<TriggerSignal> Sequence { get; }
    }
}
