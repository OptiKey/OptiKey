using System;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    public interface ITriggerSignalSource
    {
        IObservable<TriggerSignal> Sequence { get; }
    }
}
