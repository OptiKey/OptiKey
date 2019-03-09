// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public interface IFixationTriggerSource : ITriggerSource
    {
        KeyEnabledStates KeyEnabledStates { set; }
    }
}
