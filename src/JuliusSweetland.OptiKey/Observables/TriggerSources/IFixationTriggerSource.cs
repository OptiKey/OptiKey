using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public interface IFixationTriggerSource : ITriggerSource
    {
        KeyEnabledStates KeyEnabledStates { set; }
        IPointSource PointSource { get; set; }
    }
}
