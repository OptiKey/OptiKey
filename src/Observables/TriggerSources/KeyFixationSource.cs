using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Observables.TriggerSources
{
    /// <summary>
    /// Aggregates fixations on keys, allowing you to move away and return to complete a key fixation.
    /// Point fixations have to be completed in one go, i.e. if you move away from a point fixation it is lost.
    /// </summary>
    public class KeyFixationSource : ITriggerSource, IFixationTriggerSource
    {
        #region Fields

        private readonly TimeSpan lockOnTime;
        private readonly TimeSpan timeToCompleteTrigger;
        private readonly TimeSpan incompleteFixationTtl;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;
        private readonly Dictionary<KeyValue, long> incompleteFixationProgress;
        private readonly Dictionary<KeyValue, IDisposable> incompleteFixationTimeouts;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public KeyFixationSource(
            TimeSpan lockOnTime,
            TimeSpan timeToCompleteTrigger,
            TimeSpan incompleteFixationTtl,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.lockOnTime = lockOnTime;
            this.timeToCompleteTrigger = timeToCompleteTrigger;
            this.incompleteFixationTtl = incompleteFixationTtl;
            this.pointAndKeyValueSource = pointAndKeyValueSource;
            
            incompleteFixationProgress = new Dictionary<KeyValue, long>();
            incompleteFixationTimeouts = new Dictionary<KeyValue, IDisposable>();
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

                        Timestamped<PointAndKeyValue>? lockOnStart = null; 
                        DateTimeOffset fixationStart = DateTimeOffset.MinValue;
                        PointAndKeyValue? fixationCentrePointAndKeyValue = null;
                        
                        Action disposeAllSubscriptions = null;

                        var pointAndKeyValueSubscription = pointAndKeyValueSource
                            .Where(_ => disposed == false)
                            .Where(tp => tp.Value != null) //Filter out stale indicators - the fixation progress is not reset by the points sequence being stale
                            .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.Value, tp.Timestamp))
                            .Buffer(2, 1) //Sliding buffer of 2 (last & current) that moves by 1 value at a time
                            .Subscribe(tps =>
                            {
                                Timestamped<PointAndKeyValue> latestPointAndKeyValue = tps.Last(); //Store latest timeStampedPointAndKeyValue
                                    
                                if (fixationCentrePointAndKeyValue == null)
                                {
                                    //Lock-on in progress, but latest point breaks the lock-on
                                    if (lockOnStart != null
                                        && (latestPointAndKeyValue.Value.KeyValue == null
                                            || !lockOnStart.Value.Value.KeyValue.Equals(latestPointAndKeyValue.Value.KeyValue)
                                            || (KeyEnabledStates != null && !KeyEnabledStates[latestPointAndKeyValue.Value.KeyValue.Value])))
                                    {
                                        lockOnStart = null;
                                    }

                                    //We have no current lock-on - start a new one
                                    if (lockOnStart == null
                                        && latestPointAndKeyValue.Value.KeyValue != null
                                        && (KeyEnabledStates == null || KeyEnabledStates[latestPointAndKeyValue.Value.KeyValue.Value]))
                                    {
                                        lockOnStart = latestPointAndKeyValue;
                                    }

                                    //Check if the current lock-on is complete - if so start a new fixation
                                    if (lockOnStart != null
                                        && latestPointAndKeyValue.Value.KeyValue != null
                                        && latestPointAndKeyValue.Timestamp.Subtract(lockOnStart.Value.Timestamp) >= lockOnTime)
                                    {
                                        fixationStart = latestPointAndKeyValue.Timestamp;
                                        fixationCentrePointAndKeyValue = new PointAndKeyValue(latestPointAndKeyValue.Value.Point, latestPointAndKeyValue.Value.KeyValue);
                                        lockOnStart = null;
                                    }
                                }
                                else
                                {
                                    //We are building a fixation and the latest pointAndKeyValue is not over the same key, or the key is now disabled
                                    if (latestPointAndKeyValue.Value.KeyValue == null
                                        || !fixationCentrePointAndKeyValue.Value.KeyValue.Equals(latestPointAndKeyValue.Value.KeyValue)
                                        || (KeyEnabledStates != null && !KeyEnabledStates[latestPointAndKeyValue.Value.KeyValue.Value]))
                                    {
                                        //Get the last point which was part of the current fixation, i.e. over the previously fixated key
                                        Timestamped<PointAndKeyValue>? previousPointAndKeyValue = tps.Count > 1
                                                ? tps[tps.Count - 2]
                                                : (Timestamped<PointAndKeyValue>?)null;

                                        if (previousPointAndKeyValue != null)
                                        {
                                            //Calculate the span of the fixation up to this point and store the aggregate progress (so that we can resume progress later)
                                            var fixationSpan = previousPointAndKeyValue.Value.Timestamp.Subtract(fixationStart);
                                            
                                            if (incompleteFixationProgress.ContainsKey(fixationCentrePointAndKeyValue.Value.KeyValue.Value))
                                            {
                                                incompleteFixationProgress[fixationCentrePointAndKeyValue.Value.KeyValue.Value] = 
                                                    incompleteFixationProgress[fixationCentrePointAndKeyValue.Value.KeyValue.Value] + fixationSpan.Ticks;
                                            }
                                            else
                                            {
                                                incompleteFixationProgress.Add(fixationCentrePointAndKeyValue.Value.KeyValue.Value, fixationSpan.Ticks);
                                            }

                                            //Setup incomplete fixation timeout
                                            if (incompleteFixationTimeouts.ContainsKey(fixationCentrePointAndKeyValue.Value.KeyValue.Value))
                                            {
                                                incompleteFixationTimeouts[fixationCentrePointAndKeyValue.Value.KeyValue.Value].Dispose();
                                            }

                                            PointAndKeyValue fixationCentrePointAndKeyValueCopy = fixationCentrePointAndKeyValue.Value; //Access to modified closure
                                            incompleteFixationTimeouts[fixationCentrePointAndKeyValue.Value.KeyValue.Value] =
                                                Observable.Timer(incompleteFixationTtl).Subscribe(_ =>
                                                {
                                                    incompleteFixationProgress[fixationCentrePointAndKeyValueCopy.KeyValue.Value] = 0;
                                                    incompleteFixationTimeouts.Remove(fixationCentrePointAndKeyValueCopy.KeyValue.Value);
                                                    observer.OnNext(new TriggerSignal(null, 0, fixationCentrePointAndKeyValueCopy));
                                                });
                                        }

                                        //Clear the current fixation and return
                                        fixationCentrePointAndKeyValue = null;
                                        return;
                                    }
                                }

                                if (fixationCentrePointAndKeyValue != null)
                                {
                                    //We have created or added to a fixation - update state vars, publish our progress and reset if necessary
                                    var fixationSpan = latestPointAndKeyValue.Timestamp.Subtract(fixationStart);

                                    var storedProgress =
                                        incompleteFixationProgress.ContainsKey(fixationCentrePointAndKeyValue.Value.KeyValue.Value)
                                            ? incompleteFixationProgress[fixationCentrePointAndKeyValue.Value.KeyValue.Value]
                                            : 0;

                                    //Dispose of the expiry timer for the current fixation as it is in progress again
                                    if (incompleteFixationTimeouts.ContainsKey(fixationCentrePointAndKeyValue.Value.KeyValue.Value))
                                    {
                                        incompleteFixationTimeouts[fixationCentrePointAndKeyValue.Value.KeyValue.Value].Dispose();
                                    }

                                    var progress = (((double)(storedProgress + fixationSpan.Ticks)) / (double)timeToCompleteTrigger.Ticks);

                                    //Publish a high signal if progress is 1 (100%), otherwise just publish progress (if > 0 as 0 is a reset signal for progress across all keys)
                                    if(progress > 0)
                                    {
                                        observer.OnNext(new TriggerSignal(
                                            progress >= 1 ? 1 : (double?)null, progress >= 1 ? 1 : progress, fixationCentrePointAndKeyValue));
                                    }

                                    //Reset if we've just published a high signal
                                    if (progress >= 1)
                                    {
                                        foreach (var key in incompleteFixationTimeouts.Keys)
                                        {
                                            incompleteFixationTimeouts[key].Dispose();
                                        }

                                        fixationCentrePointAndKeyValue = null;
                                        incompleteFixationProgress.Clear();
                                        incompleteFixationTimeouts.Clear();
                                        lockOnStart = null;

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
