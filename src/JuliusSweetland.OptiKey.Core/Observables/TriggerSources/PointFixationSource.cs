// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public class PointFixationSource : IFixationTriggerSource
    {
        #region Fields

        private readonly TimeSpan lockOnTime;
        private readonly TimeSpan timeToCompleteTrigger;
        private readonly double lockOnRadiusSquared;
        private readonly double fixationRadiusSquared;

        private IPointSource pointSource;
        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public PointFixationSource(
            TimeSpan lockOnTime,
            TimeSpan timeToCompleteTrigger,
            double lockOnRadius,
            double fixationRadius,
            IPointSource pointSource)
        {
            this.lockOnTime = lockOnTime;
            this.timeToCompleteTrigger = timeToCompleteTrigger;
            this.lockOnRadiusSquared = lockOnRadius * lockOnRadius;
            this.fixationRadiusSquared = fixationRadius * fixationRadius;
            this.pointSource = pointSource;
            this.AllowPointsOverKeys = true;
        }

        #endregion

        #region Properties

        public RunningStates State { get; set; }

        public bool AllowPointsOverKeys { get; set; } 
        public KeyEnabledStates KeyEnabledStates { get; set; } //Irrelevant on point trigger
        public IDictionary<KeyValue, TimeSpanOverrides> OverrideTimesByKey { get; set; } //Irrelevant on point trigger

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
                    sequence = Observable.Create<TriggerSignal>(observer =>
                    {
                        bool disposed = false;

                        var buffer = new List<Timestamped<PointAndKeyValue>>();
                        DateTimeOffset fixationStart = DateTimeOffset.MinValue;
                        PointAndKeyValue fixationCentrePointAndKeyValue = null;

                        Action disposeAllSubscriptions = null;
                        
                        var pointAndKeyValueSubscription = pointSource.Sequence
                            .Where(_ => disposed == false)
                            .Where(_ => State == RunningStates.Running)
                            .Where(tp => tp.Value != null) //Filter out stale indicators - the fixation progress is not reset by the points sequence being stale
                            .Where(tp => AllowPointsOverKeys || tp.Value.KeyValue == null)
                            .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value, tp.Timestamp))
                            .Subscribe(point =>
                            {
                                //Maintain a buffer which contains points which fill the lockOnTime 
                                buffer.Add(new Timestamped<PointAndKeyValue>(point.Value, point.Timestamp));
                                var lockOnStartTime = point.Timestamp.Subtract(lockOnTime);
                                var bufferFullFromIndex = buffer.FindLastIndex(tpakv => tpakv.Timestamp < lockOnStartTime);
                                bool bufferIsFull = bufferFullFromIndex > -1;
                                if (bufferFullFromIndex > 0) //Trim the buffer - only keep lock-on period + 1 point earlier than limit
                                {
                                    buffer.RemoveRange(0, bufferFullFromIndex);
                                }
                                
                                if (fixationCentrePointAndKeyValue == null)
                                {
                                    //No fixation in progress - we are in the "lock on" phase
                                    if (bufferIsFull)
                                    {
                                        //Test if all buffered points (enough for a lock-on) are within an acceptable lock-on radius of their centre point
                                        var centrePoint = buffer.Select(t => t.Value.Point).ToList().CalculateCentrePoint();
                                        if (buffer.All(t => 
                                            (Math.Pow((centrePoint.X - t.Value.Point.X), 2)
                                            + Math.Pow((centrePoint.Y - t.Value.Point.Y), 2)) 
                                            < lockOnRadiusSquared)) //Bit of right-angled triangle maths: a squared + b squared = c squared
                                        {
                                            //Lock-on complete - start a new fixation from the last (most recent) acceptable point
                                            fixationCentrePointAndKeyValue = new PointAndKeyValue(buffer.Last().Value.Point, null);
                                            fixationStart = point.Timestamp;
                                        }
                                    }
                                }
                                else
                                {
                                    //We have a current fixation
                                    
                                    //Latest point breaks the current fixation (is outside the acceptable radius of the current fixation)
                                    if ((Math.Pow((fixationCentrePointAndKeyValue.Point.X - point.Value.Point.X), 2)
                                        + Math.Pow((fixationCentrePointAndKeyValue.Point.Y - point.Value.Point.Y), 2)) 
                                        > fixationRadiusSquared) //Bit of right-angled triangle maths: a squared + b squared = c squared
                                    {
                                        //Clear the current fixation and reset progress
                                        fixationCentrePointAndKeyValue = null;
                                        observer.OnNext(new TriggerSignal(null, 0, null));
                                    }
                                    else
                                    {
                                        //We have created or added to a fixation - update state vars, publish our progress and reset if necessary
                                        var fixationSpan = point.Timestamp.Subtract(fixationStart);
                                        var progress = (((double)fixationSpan.Ticks) / (double)timeToCompleteTrigger.Ticks);

                                        //Publish a high signal if progress is 1 (100%), otherwise just publish progress (filter out 0 as this is a progress reset signal)
                                        if (progress > 0)
                                        {
                                            fixationCentrePointAndKeyValue = point.Value;
                                            observer.OnNext(new TriggerSignal(
                                                progress >= 1 ? 1 : (double?)null, progress >= 1 ? 1 : progress, fixationCentrePointAndKeyValue));
                                        }

                                        //Reset if we've just published a high signal
                                        if (progress >= 1)
                                        {
                                            fixationCentrePointAndKeyValue = null;
                                            buffer.Clear();
                                            observer.OnNext(new TriggerSignal(null, 0, null));
                                            return;
                                        }
                                    }
                                }
                            },
                            ex =>
                            {
                                observer.OnError(ex);
                                disposeAllSubscriptions();
                            });

                        disposeAllSubscriptions = () =>
                        {
                            disposed = true;

                            if (pointAndKeyValueSubscription != null)
                            {
                                pointAndKeyValueSubscription.Dispose();
                                pointAndKeyValueSubscription = null;
                            }

                            sequence = null;
                        };

                        return disposeAllSubscriptions;
                    })
                    .Publish()
                    .RefCount();
                }

                return sequence;
            }
        }

        #endregion
    }
}
