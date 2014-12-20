using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    public class PointFixationSource : IFixationTriggerSource
    {
        #region Fields

        private readonly TimeSpan timeToStartTrigger;
        private readonly TimeSpan timeToCompleteTrigger;
        private readonly double fixationRadiusSquared;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public PointFixationSource(
            TimeSpan timeToStartTrigger,
            TimeSpan timeToCompleteTrigger,
            double fixationRadius,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.timeToStartTrigger = timeToStartTrigger;
            this.timeToCompleteTrigger = timeToCompleteTrigger;
            this.fixationRadiusSquared = fixationRadius * fixationRadius;
            this.pointAndKeyValueSource = pointAndKeyValueSource;
        }

        #endregion

        #region Properties

        public KeyEnabledStates KeyEnabledStates { get; set; } //Irrelevent on point trigger

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
                        PointAndKeyValue? fixationCentrePointAndKeyValue = null;
                        DateTimeOffset fixationStart = DateTimeOffset.Now;

                        Action disposeAllSubscriptions = null;
                        
                        var pointAndKeyValueSubscription = pointAndKeyValueSource
                            .Where(_ => disposed == false)
                            .Subscribe(point =>
                            {
                                //If any of the points received are null then the points feed is considered stale - reset
                                if (point.Value == null)
                                {
                                    fixationCentrePointAndKeyValue = null;
                                    buffer.Clear();
                                    observer.OnNext(new TriggerSignal(null, 0, null));
                                    return;
                                }

                                //Maintain a buffer which contains points which fill the timeToStartTrigger 
                                buffer.Add(new Timestamped<PointAndKeyValue>(point.Value.Value, point.Timestamp));
                                var startTriggerTime = point.Timestamp.Subtract(timeToStartTrigger);
                                int? bufferUsefulLimit = null;
                                for (int index = buffer.Count - 1; index >= 0; index--)
                                {
                                    if (buffer[index].Timestamp < startTriggerTime)
                                    {
                                        bufferUsefulLimit = index;
                                        break;
                                    }
                                }

                                if (bufferUsefulLimit != null)
                                {
                                    buffer.RemoveRange(0, bufferUsefulLimit.Value);
                                }
                                
                                if (fixationCentrePointAndKeyValue == null) //We don't have a fixation - check if the buffered points are eligable to initiate a fixation
                                {
                                    if (bufferUsefulLimit != null) //The buffer is useful, i.e. it contains at least 1 point which predates the time to start trigger
                                    {
                                        //All the buffered points are within an acceptable radius of their centre point?
                                        var centrePoint = buffer.Select(t => t.Value.Point).ToList().CalculateCentrePoint();
                                        if (buffer.All(t =>
                                        {
                                            //Bit of right-angled triangle maths: a squared + b squared = c squared
                                            var xDiff = centrePoint.X - t.Value.Point.X;
                                            var yDiff = centrePoint.Y - t.Value.Point.Y;
                                            return ((xDiff * xDiff) + (yDiff * yDiff)) <= fixationRadiusSquared;
                                        }))
                                        {
                                            fixationCentrePointAndKeyValue = new PointAndKeyValue(centrePoint, null);
                                            fixationStart = point.Timestamp;
                                        }
                                    }
                                }
                                else
                                {
                                    var xDiff = fixationCentrePointAndKeyValue.Value.Point.X - point.Value.Value.Point.X;
                                    var yDiff = fixationCentrePointAndKeyValue.Value.Point.Y - point.Value.Value.Point.Y;

                                    //We are building a fixation based on a centre point and the latest pointAndKeyValue falls outside the acceptable radius
                                    if (((xDiff*xDiff) + (yDiff*yDiff)) > fixationRadiusSquared)
                                    {
                                        fixationCentrePointAndKeyValue = null;
                                        observer.OnNext(new TriggerSignal(null, 0, null));
                                        return;
                                    }
                                }

                                if (fixationCentrePointAndKeyValue != null)
                                {
                                    //We have created or added to a fixation - update state vars, publish our progress and reset if necessary
                                    var fixationSpan = point.Timestamp.Subtract(fixationStart);
                                    var progress = (double)fixationSpan.Ticks / (double)timeToCompleteTrigger.Ticks;

                                    //Publish a high signal if progress is 1 (100%), otherwise just publish progress
                                    observer.OnNext(new TriggerSignal(
                                        progress >= 1 ? 1 : (double?)null, progress >= 1 ? 1 : progress, fixationCentrePointAndKeyValue));

                                    //Reset if we've just published a high signal
                                    if (progress >= 1)
                                    {
                                        fixationCentrePointAndKeyValue = null;
                                        buffer.Clear();
                                        observer.OnNext(new TriggerSignal(null, 0, null));
                                        return;
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
