using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public class MouseButtonDownUpSource : ITriggerSource
    {
        #region Fields

        private readonly MouseButtons triggerButton;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;
        private readonly MouseHookListener mouseHookListener;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public MouseButtonDownUpSource(
            Enums.MouseButtons triggerButton,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.triggerButton = (System.Windows.Forms.MouseButtons)triggerButton; //Cast to the Windows.Forms.MouseButtons enum
            this.pointAndKeyValueSource = pointAndKeyValueSource;

            mouseHookListener = new MouseHookListener(new GlobalHooker())
            {
                Enabled = true
            };
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
                    var buttonDowns = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => new MouseEventHandler(handler),
                        h => mouseHookListener.MouseDown += h,
                        h => mouseHookListener.MouseDown -= h)
                        .Where(ep => ep.EventArgs.Button == triggerButton)
                        .Select(_ => true);

                    var buttonUps = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => new MouseEventHandler(handler),
                        h => mouseHookListener.MouseUp += h,
                        h => mouseHookListener.MouseUp -= h)
                        .Where(ep => ep.EventArgs.Button == triggerButton)
                        .Select(_ => false);

                    sequence = buttonDowns.Merge(buttonUps)
                        .DistinctUntilChanged()
                        .SkipWhile(b => b == false) //Ensure the first value we hit is a true, i.e. a mouse down
                        .CombineLatest(pointAndKeyValueSource, (b, point) => new TriggerSignal(b ? 1 : -1, null, point.Value))
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
