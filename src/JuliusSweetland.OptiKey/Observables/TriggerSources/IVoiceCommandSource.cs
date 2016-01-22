using System;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Services;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    /// <summary>
    /// Observable source issuing voice detected command.
    /// </summary>
    public interface IVoiceCommandSource: ITriggerSource, INotifyErrors { }
}
