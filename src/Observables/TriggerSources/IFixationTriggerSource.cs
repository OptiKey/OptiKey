using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSources
{
    public interface IFixationTriggerSource : ITriggerSource
    {
        KeyEnabledStates KeyEnabledStates { set; }
    }
}
