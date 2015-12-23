using System;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    //TODO: Should this become a service? i.e. renamed and moved to .Services\IKeyboardBoardService? -LC
    public interface IKeyboardHookListener
    {
        IObservable<KeyDirection> KeyMovements(System.Windows.Forms.Keys triggerKey);
    }
}