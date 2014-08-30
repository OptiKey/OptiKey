using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    public class KeyFixationSource : ITriggerSignalSource, IFixationTriggerSource
    {
        #region Fields

        private readonly int detectFixationBufferSize;
        private readonly TimeSpan fixationTriggerTime;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public KeyFixationSource(
            int detectFixationBufferSize,
            TimeSpan fixationTriggerTime,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.detectFixationBufferSize = detectFixationBufferSize;
            this.fixationTriggerTime = fixationTriggerTime;
            this.pointAndKeyValueSource = pointAndKeyValueSource;
        }

        #endregion

        #region Properties

        public KeyEnabledStates KeyEnabledStates { get; set; }

        public IObservable<TriggerSignal> Sequence
        {
            get
            {
                if (sequence == null)
                { 
                    sequence = Observable.Create<TriggerSignal>(observer =>
                    {
                        bool disposed = false;

                        Timestamped<PointAndKeyValue>? latestPointAndKeyValue;
                        PointAndKeyValue? fixationCentrePointAndKeyValue = null;
                        DateTimeOffset fixationStart = DateTimeOffset.Now;

                        Action disposeAllSubscriptions = null;

                        var pointAndKeyValueSubscription = pointAndKeyValueSource
                            .Where(_ => disposed == false)
                            .Buffer(detectFixationBufferSize, 1) //Sliding buffer that moves by 1 value at a time
                            .Subscribe(nullableTps =>
                            {
                                //If any of the PointAndKeyValues received are null then the points feed is considered stale - reset
                                if (nullableTps.Any(tp => tp.Value == null))
                                {
                                    fixationCentrePointAndKeyValue = null;
                                    observer.OnNext(new TriggerSignal(null, 0, null));
                                    return;
                                }

                                //We know all buffered points are non-null now
                                var tps = nullableTps
                                    .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.Value, tp.Timestamp))
                                    .ToList();

                                latestPointAndKeyValue = tps.Last(); //Store latest timeStampedPointAndKeyValue

                                if (fixationCentrePointAndKeyValue == null) //We don't have a fixation - check if the buffered points are eligable to initiate a fixation:
                                {
                                    if (tps.All(t => t.Value.KeyValue != null)
                                        && tps.Select(t => t.Value.KeyValue).Distinct().Count() == 1
                                        && (KeyEnabledStates == null || KeyEnabledStates[tps.First().Value.KeyValue.Value.Key]))
                                    {
                                        //All buffered tps have the same key value and that key is enabled
                                        var centrePoint = tps.Select(t => t.Value.Point).ToList().CalculateCentrePoint();
                                        var keyValue = tps.First().Value.KeyValue;
                                        fixationCentrePointAndKeyValue = new PointAndKeyValue(centrePoint, keyValue);
                                        fixationStart = tps.First().Timestamp;
                                    }
                                }
                                else
                                {
                                    //We are building a fixation based on a key value and the latest pointAndKeyValue is not on that key, or the key is now disabled
                                    if (fixationCentrePointAndKeyValue.Value.KeyValue != null
                                        && (latestPointAndKeyValue.Value.Value.KeyValue == null
                                            || !fixationCentrePointAndKeyValue.Value.KeyValue.Equals(latestPointAndKeyValue.Value.Value.KeyValue)
                                            || (KeyEnabledStates != null && !KeyEnabledStates[latestPointAndKeyValue.Value.Value.KeyValue.Value.Key])))
                                    {
                                        fixationCentrePointAndKeyValue = null;
                                        observer.OnNext(new TriggerSignal(null, 0, null));
                                        return;
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
