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
            bool allowRepeats,
            int firstRepeatMs,
            int nextRepeatMs,
            IPointSource pointSource)
        {
            this.triggerButtonIdx = triggerButtonIdx;
            this.pointSource = pointSource;
            this.controllerGuid = controllerGuid;

            directInputListener = DirectInputListener.Instance;
            directInputListener.AllowRepeats(allowRepeats, firstRepeatMs, nextRepeatMs);
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

                    var combined = keyDowns
                                    .Merge(keyUps)
                                    .SkipWhile(b => b.eventType == EventType.UP) //Ensure the first value we hit is a key down
                                    .CombineLatest(pointSource.Sequence, (buttonEvent, point) => new RepeatableTriggerSignal(
                                        new TriggerSignal(buttonEvent.eventType == EventType.DOWN ? 1 : -1, null, point.Value), buttonEvent.isRepeat))
                                    ;

                    // Keep track of keyvalue at original keydown, to allow repeats for this keyvalue only
                    var startKey = combined
                                    .DistinctUntilChanged(e => e.triggerSignal.Signal)
                                    .Where(e => (e.triggerSignal.Signal == 1.0) && !e.isRepeat)
                                    .Select(e => e.triggerSignal.PointAndKeyValue.KeyValue);

                    // Use startKeyValue to flag up repeats that are not permitted
                    var combinedWithRepeatSuppression = startKey.CombineLatest(combined, (kv, rts) => {
                        rts.isRepeatAllowed = kv == rts.triggerSignal.PointAndKeyValue.KeyValue;
                        return rts;
                    });

                    // Filter out bad repeats
                    var final = combinedWithRepeatSuppression
                                .Where(rts => rts.isRepeatAllowed)
                                .DistinctUntilChanged(rts => rts.triggerSignal.Signal)
                                .Select(rts => rts.triggerSignal)
                                ;

                    sequence = final
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
