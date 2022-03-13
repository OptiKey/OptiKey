// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Models.Gamepads;
using JuliusSweetland.OptiKey.Observables.PointSources;
using log4net;
using static JuliusSweetland.OptiKey.Models.Gamepads.DirectInputListener;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public class DirectInputButtonDownUpSource : ITriggerSource
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int triggerButtonIdx;
        private readonly DirectInputListener directInputListener;
        private readonly Guid controllerGuid;

        private IPointSource pointSource;
        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public DirectInputButtonDownUpSource(
            Guid controllerGuid,
            int triggerButtonIdx,
            IPointSource pointSource)
        {
            this.triggerButtonIdx = triggerButtonIdx;
            this.pointSource = pointSource;
            this.controllerGuid = controllerGuid;

            directInputListener = DirectInputListener.Instance;
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
                    var keyDowns = Observable.FromEventPattern<DirectInputButtonDownEventHandler, DirectInputButtonEventArgs>(
                            handler => new DirectInputButtonDownEventHandler(handler),
                            h => directInputListener.ButtonDown += h,
                            h => directInputListener.ButtonDown -= h)
                        .Do(ep => {
                            Log.InfoFormat("gamepad button {0} UP", ep.EventArgs.button);
                        })
                        .Where(ep => {
                            return ( (controllerGuid == Guid.Empty || ep.EventArgs.controller == controllerGuid) 
                                     && ep.EventArgs.button == triggerButtonIdx);
                         })
                        .Select(ep => ep.EventArgs)
                        .Do(_ => Log.DebugFormat("Trigger key down detected ({0})", triggerButtonIdx));

                    var keyUps = Observable.FromEventPattern<DirectInputButtonUpEventHandler, DirectInputButtonEventArgs>(
                            handler => new DirectInputButtonUpEventHandler(handler),
                            h => directInputListener.ButtonUp += h,
                            h => directInputListener.ButtonUp -= h)
                        .Do(ep => {
                            Log.InfoFormat("gamepad button {0} UP", ep.EventArgs.button);
                        })
                        .Where(ep => {
                            return (ep.EventArgs.button == triggerButtonIdx);
                        })
                        .Select(ep => ep.EventArgs)
                        .Do(_ => Log.DebugFormat("Trigger key up detected ({0})", triggerButtonIdx));

                    sequence = keyDowns.Merge(keyUps)
                        .DistinctUntilChanged()
                        .SkipWhile(b => b.eventType == EventType.DOWN) //Ensure the first value we hit is a true, i.e. a key down
                        .CombineLatest(pointSource.Sequence, (b, point) => new TriggerSignal(b.eventType == EventType.DOWN ? 1 : -1, null, point.Value))
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
