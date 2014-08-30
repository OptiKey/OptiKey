using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    public interface IFixationTriggerSource : ITriggerSignalSource
    {
        KeyEnabledStates KeyEnabledStates { set; }
    }
}
