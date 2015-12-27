using System;
using System.Reactive.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public sealed class KeyboardKeyDownUpSource : ITriggerSource
    {
        #region Ctor

        public KeyboardKeyDownUpSource(
            Enums.Keys triggerKey,
            IKeyboardHookListener keyboardHookListener,
            IPointSource pointSource)
        {
            var wfTriggerKey = (System.Windows.Forms.Keys)triggerKey; //Cast to the Windows.Forms.Keys enum

            /*
             * Keys: http://msdn.microsoft.com/en-GB/library/system.windows.forms.keys.aspx
             * KeyDown: happens when the person presses a key (when the keyboard first detects a finger on a key, this happens when the key is pressed down).
             * KeyPress: happens when a key is pressed and then released.
             * KeyUp: happens when the key is released
             */

            var keyPresses = keyboardHookListener.KeyMovements(wfTriggerKey);
            var pointAndKeyValueSource = pointSource.Sequence.Select(ts => ts.Value);
            Sequence = keyPresses
                .CombineLatest(pointAndKeyValueSource, (keyDirection, point) => new TriggerSignal(keyDirection.IsKeyDown ? 1 : -1, null, point.Value))
                .DistinctUntilChanged(signal => signal.Signal) //Combining latest will output a trigger signal for every change in BOTH sequences - only output when the trigger signal changes
                .Where(_ => State == RunningStates.Running)
                .Publish()
                .RefCount();
        }

        #endregion

        #region Properties

        public RunningStates State { get; set; }

        public IObservable<TriggerSignal> Sequence { get; }

        #endregion
    }
}
