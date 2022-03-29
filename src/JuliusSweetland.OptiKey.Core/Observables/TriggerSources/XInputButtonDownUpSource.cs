// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Models.Gamepads;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using SharpDX.XInput;
using static JuliusSweetland.OptiKey.Models.Gamepads.XInputListener;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public class XInputButtonDownUpSource : ITriggerSource
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly GamepadButtonFlags triggerButton;
        private readonly XInputListener xinputListener;
        private readonly UserIndex userIndex;

        private IPointSource pointSource;
        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public XInputButtonDownUpSource(
            UserIndex userIndex,
            GamepadButtonFlags triggerButton,
            IPointSource pointSource)
        {
            Log.Info("Creating XInputButtonDownUpSource");
            this.triggerButton = triggerButton;
            this.pointSource = pointSource;
            this.userIndex = userIndex;

            xinputListener = XInputListener.Instance;
        }

        #endregion

        #region Properties

        public RunningStates State { get; set; }

        /// <summary>
        /// Change the point and key value source. N.B. After setting this any existing subscription 
        /// to the sequence must be disposed and the getter called again to recreate the sequence again.
        /// </summary>
        public IPointSource PointSource
        {
            get { return pointSource; }
            set { pointSource = value; }
        }

        public IObservable<TriggerSignal> Sequence
        {
            get
            {
                if (sequence == null)
                {
                    //need to add controller userindex to event
                    //and sign up for only the appropriate one here

                    //or consider adding a "watch" for a particular controller via the static Instance

                    var keyDowns = Observable.FromEventPattern<XInputButtonDownEventHandler, XInputButtonEventArgs>(
                            handler => new XInputButtonDownEventHandler(handler),
                            h => xinputListener.ButtonDown += h,
                            h => xinputListener.ButtonDown -= h)
                        .Do(ep => {
                            Log.InfoFormat("gamepad button {0} DOWN [{1}]", ep.EventArgs.button, this.GetHashCode());
                        })
                        .Where(ep => {
                            return ((userIndex == UserIndex.Any || ep.EventArgs.userIndex == userIndex)
                                     && ep.EventArgs.button == triggerButton);                            
                         })
                        .Select(ep => ep.EventArgs)
                        .Do(_ => Log.DebugFormat("Trigger key down detected ({0}) [{1}]", triggerButton, this.GetHashCode()));

                    var keyUps = Observable.FromEventPattern<XInputButtonUpEventHandler, XInputButtonEventArgs>(
                            handler => new XInputButtonUpEventHandler(handler),
                            h => xinputListener.ButtonUp += h,
                            h => xinputListener.ButtonUp -= h)
                        .Do(ep => {
                            Log.InfoFormat("gamepad button {0} UP [{1}]", ep.EventArgs.button, this.GetHashCode());
                        })
                        .Where(ep => {
                            return (ep.EventArgs.button == triggerButton);
                        })
                        .Select(ep => ep.EventArgs)
                        .Do(_ => Log.DebugFormat("Trigger key up detected ({0}) [{1}]", triggerButton, this.GetHashCode()));

                    sequence = keyDowns.Merge(keyUps)
                        .DistinctUntilChanged()
                        .SkipWhile(b => b.eventType == EventType.DOWN) //Ensure the first value we hit is a true, i.e. a key down
                        .CombineLatest(pointSource.Sequence, (b, point) => new TriggerSignal(b.eventType == EventType.DOWN ? 1 : -1, null, point.Value))
                        .DistinctUntilChanged(signal => signal.Signal) //Combining latest will output a trigger signal for every change in BOTH sequences - only output when the trigger signal changes
                        .Where(_ => State == RunningStates.Running)
                        .Publish()
                        .RefCount()
                        .Finally(() => {
                            sequence = null;
                        });

                }

                return sequence;
            }
        }

        #endregion
    }
}
