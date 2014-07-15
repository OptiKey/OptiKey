using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using JuliusSweetland.ETTA.Model;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    public class MouseButtonDownUpSource : ITriggerSignalSource
    {
        #region Fields

        private readonly MouseButtons triggerButton;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;
        private readonly MouseHookListener mouseHookListener;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public MouseButtonDownUpSource(
            MouseButtons triggerButton,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.triggerButton = triggerButton;
            this.pointAndKeyValueSource = pointAndKeyValueSource;

            mouseHookListener = new MouseHookListener(new GlobalHooker())
            {
                Enabled = true
            };
        }

        #endregion

        #region Properties

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
                        .Publish()
                        .RefCount();
                }

                return sequence;
            }
        }

        #endregion
    }
}
