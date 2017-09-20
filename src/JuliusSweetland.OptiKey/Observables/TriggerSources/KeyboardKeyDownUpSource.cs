using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using Keys = System.Windows.Forms.Keys;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public class KeyboardKeyDownUpSource : ITriggerSource
    {
        #region Fields

        private readonly Keys triggerKey;
        private readonly IPointSource pointSource;
        private readonly KeyboardHookListener keyboardHookListener;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public KeyboardKeyDownUpSource(
            Enums.Keys triggerKey,
            IPointSource pointSource)
        {
            this.triggerKey = (System.Windows.Forms.Keys)triggerKey; //Cast to the Windows.Forms.Keys enum
            this.pointSource = pointSource;

            keyboardHookListener = new KeyboardHookListener(new GlobalHooker())
            {
                Enabled = true
            };

            /*
             * Keys: http://msdn.microsoft.com/en-GB/library/system.windows.forms.keys.aspx
             * KeyDown: happens when the person presses a key (when the keyboard first detects a finger on a key, this happens when the key is pressed down).
             * KeyPress: happens when a key is pressed and then released.
             * KeyUp: happens when the key is released
             */
        }

        #endregion

        #region Properties

        public RunningStates State { get; set; }

        public IObservable<TriggerSignal> Sequence
        {
            get
            {
                if (sequence == null)
                {
                    var keyDowns = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                            handler => new KeyEventHandler(handler),
                            h => keyboardHookListener.KeyDown += h,
                            h => keyboardHookListener.KeyDown -= h)
                        .Where(ep => ep.EventArgs.KeyCode == triggerKey)
                        .Select(_ => true);

                    var keyUps = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                            handler => new KeyEventHandler(handler),
                            h => keyboardHookListener.KeyUp += h,
                            h => keyboardHookListener.KeyUp -= h)
                        .Where(ep => ep.EventArgs.KeyCode == triggerKey)
                        .Select(_ => false);

                    sequence = keyDowns.Merge(keyUps)
                        .DistinctUntilChanged()
                        .SkipWhile(b => b == false) //Ensure the first value we hit is a true, i.e. a key down
                        .CombineLatest(pointSource.Sequence, (b, point) => new TriggerSignal(b ? 1 : -1, null, point.Value))
                        .DistinctUntilChanged(signal => signal.Signal) //Combining latest will output a trigger signal for every change in BOTH sequences - only output when the trigger signal changes
                        .Where(_ => State == RunningStates.Running)
                        .Publish()
                        .RefCount();
                }

                return sequence;
            }
        }

        #endregion
    }
}
