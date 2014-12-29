using System;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSources
{
    public interface ITriggerSource
    {
        IObservable<TriggerSignal> Sequence { get; }
    }
}
