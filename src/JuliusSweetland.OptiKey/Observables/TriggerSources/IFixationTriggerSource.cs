using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public interface IFixationTriggerSource : ITriggerSource
    {
        KeyEnabledStates KeyEnabledStates { set; }
    }
}
