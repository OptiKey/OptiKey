using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Model;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    public class PointFixationSource : ITriggerSignalSource, IFixationSource
    {
        #region Fields

        private readonly int detectFixationBufferSize;
        private readonly TimeSpan fixationTriggerTime;
        private readonly double fixationRadiusSquared;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public PointFixationSource(
            int detectFixationBufferSize,
            double fixationRadius,
            TimeSpan fixationTriggerTime,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.detectFixationBufferSize = detectFixationBufferSize;
            this.fixationRadiusSquared = fixationRadius * fixationRadius;
            this.fixationTriggerTime = fixationTriggerTime;
            this.pointAndKeyValueSource = pointAndKeyValueSource;
        }

        #endregion

        #region Properties

        public IObservable<TriggerSignal> Sequence
        {
            get
            {
                if (sequence == null)
                { 
                    sequence = Observable.Create<TriggerSignal>(observer =>
                    {
                        bool disposed = false;

                        Timestamped<PointAndKeyValue>? latestPointAndKeyValue = null;
                        PointAndKeyValue? fixationCentrePointAndKeyValue = null;
                        DateTimeOffset fixationStart = DateTimeOffset.Now;

                        Action disposeAllSubscriptions = null;
                        
                        var pointAndKeyValueSubscription = pointAndKeyValueSource
                            .Where(_ => disposed == false)
                            .Buffer(detectFixationBufferSize, 1) //Sliding buffer that moves by 1 value at a time
                            .Subscribe(nullablePoints =>
                            {
                                //If any of the points received are null then the points feed is considered stale - reset
                                if (nullablePoints.Any(tp => tp.Value == null))
                                {
                                    fixationCentrePointAndKeyValue = null;
                                    observer.OnNext(new TriggerSignal(null, 0, null));
                                    return;
                                }

                                //We know all buffered points are non-null now
                                var points = nullablePoints.Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.Value, tp.Timestamp));

                                latestPointAndKeyValue = points.Last(); //Store latest timeStampedPointAndKeyValue

                                if (fixationCentrePointAndKeyValue == null) //We don't have a fixation - check if the buffered points are eligable to initiate a fixation
                                {
                                    //All the pointAndKeyValues are within an acceptable radius of their centre point?
                                    var centrePoint = points.Select(t => t.Value.Point).ToList().CalculateCentrePoint();
                                    if (points.All(t =>
                                        {
                                            //Right angled triangle hypotenuse (c): c squared = a squared + b squared
                                            var xDiff = centrePoint.X - t.Value.Point.X;
                                            var yDiff = centrePoint.Y - t.Value.Point.Y;
                                            return ((xDiff*xDiff) + (yDiff*yDiff)) <= fixationRadiusSquared;
                                        }))
                                    {
                                        fixationCentrePointAndKeyValue = new PointAndKeyValue(centrePoint, null);
                                        fixationStart = points.First().Timestamp;
                                    }
                                }
                                else
                                {
                                    //We are building a fixation based on a centre point and the latest pointAndKeyValue falls outside the acceptable radius
                                    if (fixationCentrePointAndKeyValue.Value.KeyValue == null)
                                    {
                                        var xDiff = fixationCentrePointAndKeyValue.Value.Point.X - latestPointAndKeyValue.Value.Value.Point.X;
                                        var yDiff = fixationCentrePointAndKeyValue.Value.Point.Y - latestPointAndKeyValue.Value.Value.Point.Y;

                                        if (((xDiff*xDiff) + (yDiff*yDiff)) > fixationRadiusSquared)
                                        {
                                            fixationCentrePointAndKeyValue = null;
                                            observer.OnNext(new TriggerSignal(null, 0, null));
                                            return;
                                        }
                                    }
                                }

                                if (fixationCentrePointAndKeyValue != null)
                                {
                                    //We have created or added to a fixation - update state vars, publish our progress and reset if necessary
                                    var fixationSpan = latestPointAndKeyValue.Value.Timestamp.Subtract(fixationStart);
                                    var progress = (double)fixationSpan.Ticks / (double)fixationTriggerTime.Ticks;

                                    //Publish a high signal if progress is 1 (100%), otherwise just publish progress
                                    observer.OnNext(new TriggerSignal(
                                        progress >= 1 ? 1 : (double?)null, progress >= 1 ? 1 : progress, fixationCentrePointAndKeyValue));

                                    //Reset if we've just published a high signal
                                    if (progress >= 1)
                                    {
                                        fixationCentrePointAndKeyValue = null;
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
