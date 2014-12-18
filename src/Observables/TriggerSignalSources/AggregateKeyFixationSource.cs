using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSignalSources
{
    /// <summary>
    /// Aggregates fixations on keys, allowing you to move away and return to complete a key fixation.
    /// Point fixations have to be completed in one go, i.e. if you move away from a point fixation it is lost.
    /// </summary>
    public class AggregateKeyFixationSource : ITriggerSignalSource, IFixationTriggerSource
    {
        #region Fields

        private readonly int detectFixationBufferSize;
        private readonly TimeSpan fixationTriggerTime;
        private readonly TimeSpan pointTtl;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;
        private readonly Dictionary<KeyValue, long> keyAggregateFixationTicks;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public AggregateKeyFixationSource(
            int detectFixationBufferSize,
            TimeSpan fixationTriggerTime,
            TimeSpan pointTtl,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.detectFixationBufferSize = detectFixationBufferSize;
            this.fixationTriggerTime = fixationTriggerTime;
            this.pointTtl = pointTtl;
            this.pointAndKeyValueSource = pointAndKeyValueSource;
            
            keyAggregateFixationTicks = new Dictionary<KeyValue, long>();
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
                            .Where(tp => tp.Value != null) //Filter out stale indicators - the fixation progress is not reset by the points sequence being stale
                            .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.Value, tp.Timestamp))
                            .Buffer(detectFixationBufferSize, 1) //Sliding buffer that moves by 1 value at a time
                            .Subscribe(tps =>
                            {
                                latestPointAndKeyValue = tps.Last(); //Store latest timeStampedPointAndKeyValue
                                    
                                if (fixationCentrePointAndKeyValue == null)
                                {
                                    //We don't have a fixation - check if the buffered points meet the criteria to start a new one
                                    if (tps.All(t => t.Value.KeyValue != null)
                                        && tps.Select(t => t.Value.KeyValue).Distinct().Count() == 1
                                        && (KeyEnabledStates == null || KeyEnabledStates[tps.First().Value.KeyValue.Value]))
                                    {
                                        //All the pointAndKeyValues have the same key value and the key is enabled (or we don't know the key states)
                                        var centrePoint = tps.Select(t => t.Value.Point).ToList().CalculateCentrePoint();
                                        var keyValue = tps.Last().Value.KeyValue;
                                        fixationCentrePointAndKeyValue = new PointAndKeyValue(centrePoint, keyValue);
                                        fixationStart = tps.Last().Timestamp;
                                    }
                                }
                                else
                                {
                                    //We are building a fixation and the latest pointAndKeyValue is not over the same key, or the key is now disabled
                                    if (latestPointAndKeyValue.Value.Value.KeyValue == null
                                        || !fixationCentrePointAndKeyValue.Value.KeyValue.Equals(latestPointAndKeyValue.Value.Value.KeyValue)
                                        || (KeyEnabledStates != null && !KeyEnabledStates[latestPointAndKeyValue.Value.Value.KeyValue.Value]))
                                    {
                                        //Get the last point which was part of the current fixation, i.e. over the previously fixated key
                                        Timestamped<PointAndKeyValue>? previousPointAndKeyValue = tps.Count > 1
                                                ? tps[tps.Count - 2]
                                                : (Timestamped<PointAndKeyValue>?)null;

                                        if (previousPointAndKeyValue != null)
                                        {
                                            //Calculate the span of the fixation up to this point and store the aggregate progress (so that we can resume progress later)
                                            var fixationSpan = previousPointAndKeyValue.Value.Timestamp.Subtract(fixationStart);
                                            
                                            if (keyAggregateFixationTicks.ContainsKey(fixationCentrePointAndKeyValue.Value.KeyValue.Value))
                                            {
                                                keyAggregateFixationTicks[fixationCentrePointAndKeyValue.Value.KeyValue.Value] = 
                                                    keyAggregateFixationTicks[fixationCentrePointAndKeyValue.Value.KeyValue.Value] + fixationSpan.Ticks;
                                            }
                                            else
                                            {
                                                keyAggregateFixationTicks.Add(fixationCentrePointAndKeyValue.Value.KeyValue.Value, fixationSpan.Ticks);
                                            }
                                        }

                                        //Clear the current fixation and return
                                        fixationCentrePointAndKeyValue = null;
                                        return;
                                    }
                                }

                                if (fixationCentrePointAndKeyValue != null)
                                {
                                    //We have created or added to a fixation - update state vars, publish our progress and reset if necessary
                                    var fixationSpan = latestPointAndKeyValue.Value.Timestamp.Subtract(fixationStart);

                                    var storedProgress =
                                        keyAggregateFixationTicks.ContainsKey(fixationCentrePointAndKeyValue.Value.KeyValue.Value)
                                            ? keyAggregateFixationTicks[fixationCentrePointAndKeyValue.Value.KeyValue.Value]
                                            : 0;

                                    var progress = (((double)(storedProgress + fixationSpan.Ticks)) / (double)fixationTriggerTime.Ticks);

                                    //Publish a high signal if progress is 1 (100%), otherwise just publish progress (if > 0 as 0 is a reset signal for progress across all keys)
                                    if(progress > 0)
                                    {
                                        observer.OnNext(new TriggerSignal(
                                            progress >= 1 ? 1 : (double?)null, progress >= 1 ? 1 : progress, fixationCentrePointAndKeyValue));
                                    }

                                    //Reset if we've just published a high signal
                                    if (progress >= 1)
                                    {
                                        fixationCentrePointAndKeyValue = null;
                                        keyAggregateFixationTicks.Clear();
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
