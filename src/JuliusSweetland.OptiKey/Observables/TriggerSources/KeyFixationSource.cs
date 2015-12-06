using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    /// <summary>
    /// Aggregates fixations on keys, allowing you to move away and return to complete a key fixation.
    /// Point fixations have to be completed in one go, i.e. if you move away from a point fixation it is lost.
    /// </summary>
    public class KeyFixationSource : ITriggerSource, IFixationTriggerSource
    {
        #region Fields

        private readonly TimeSpan lockOnTime;
        private readonly bool resumeRequiresLockOn;
        private readonly TimeSpan timeToCompleteTrigger;
        private readonly TimeSpan incompleteFixationTtl;
        private readonly IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource;
        private readonly ConcurrentDictionary<KeyValue, long> incompleteFixationProgress;
        private readonly ConcurrentDictionary<KeyValue, IDisposable> incompleteFixationTimeouts;

        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public KeyFixationSource(
            TimeSpan lockOnTime,
            bool resumeRequiresLockOn,
            TimeSpan timeToCompleteTrigger,
            TimeSpan incompleteFixationTtl,
            IObservable<Timestamped<PointAndKeyValue?>> pointAndKeyValueSource)
        {
            this.lockOnTime = lockOnTime;
            this.resumeRequiresLockOn = resumeRequiresLockOn;
            this.timeToCompleteTrigger = timeToCompleteTrigger;
            this.incompleteFixationTtl = incompleteFixationTtl;
            this.pointAndKeyValueSource = pointAndKeyValueSource;

            incompleteFixationProgress = new ConcurrentDictionary<KeyValue, long>();
            incompleteFixationTimeouts = new ConcurrentDictionary<KeyValue, IDisposable>();
        }

        #endregion

        #region Properties

        public RunningStates State { get; set; }

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
                            .Where(_ => State == RunningStates.Running)
                            .Where(tp => tp.Value != null) //Filter out stale indicators - the fixation progress is not reset by the points sequence being stale
                            .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.Value, tp.Timestamp))
                            .Buffer(2, 1) //Sliding buffer of 2 (last & current) that moves by 1 value at a time
                            .Subscribe(tps =>
                            {
                                Timestamped<PointAndKeyValue> latestPointAndKeyValue = tps.Last(); //Store latest timeStampedPointAndKeyValue

                                if (fixationCentrePointAndKeyValue == null
                                    && !resumeRequiresLockOn)
                                {
                                    //Does the latest point continue an incomplete fixation? Continue it immediately, i.e. don't require a lock on again
                                    long storedProgress;
                                    if (latestPointAndKeyValue.Value.KeyValue != null
                                        && incompleteFixationProgress.TryGetValue(latestPointAndKeyValue.Value.KeyValue.Value, out storedProgress))
                                    {
                                        fixationStart = latestPointAndKeyValue.Timestamp;
                                        fixationCentrePointAndKeyValue = new PointAndKeyValue(latestPointAndKeyValue.Value.Point, latestPointAndKeyValue.Value.KeyValue);
                                        lockOnStart = null;
                                    }
                                }

                                if (fixationCentrePointAndKeyValue == null)
                                {
                                    //No fixation in progress - we are in the "lock on" phase
                                    if (lockOnStart == null)
                                    {
                                        //We have no current lock-on - start a new one
                                        if (latestPointAndKeyValue.Value.KeyValue != null
                                            && (KeyEnabledStates == null || KeyEnabledStates[latestPointAndKeyValue.Value.KeyValue.Value]))
                                        {
                                            lockOnStart = latestPointAndKeyValue;
                                        }
                                    }
                                    else
                                    {
                                        //Lock-on in progress, but latest point breaks the lock-on
                                        if (latestPointAndKeyValue.Value.KeyValue == null
                                            || !lockOnStart.Value.Value.KeyValue.Equals(latestPointAndKeyValue.Value.KeyValue)
                                            || (KeyEnabledStates != null && !KeyEnabledStates[latestPointAndKeyValue.Value.KeyValue.Value]))
                                        {
                                            lockOnStart = null;
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
                                }
                                else
                                {
                                    //We have a current fixation
                                    if (latestPointAndKeyValue.Value.KeyValue == null
                                        || !fixationCentrePointAndKeyValue.Value.KeyValue.Equals(latestPointAndKeyValue.Value.KeyValue)
                                        || (KeyEnabledStates != null && !KeyEnabledStates[latestPointAndKeyValue.Value.KeyValue.Value]))
                                    {
                                        //Latest pointAndKeyValue is not over the fixation key, or the key is now disabled
                                        if (tps.Count > 1)
                                        {
                                            //We have a buffer which contains the previous (over fixation key) point
                                            Timestamped<PointAndKeyValue>? previousPointAndKeyValue = tps[tps.Count - 2];

                                            //Calculate the span of the fixation up to this point and store the aggregate progress (so that we can resume progress later)
                                            var fixationSpan = previousPointAndKeyValue.Value.Timestamp.Subtract(fixationStart);
                                            incompleteFixationProgress.AddOrUpdate(fixationCentrePointAndKeyValue.Value.KeyValue.Value,
                                                    _ => fixationSpan.Ticks,
                                                    (_, existingProgress) => existingProgress + fixationSpan.Ticks);

                                            //Dispose of existing incomplete fixation timeout for this key
                                            IDisposable timeout;
                                            if (incompleteFixationTimeouts.TryGetValue(fixationCentrePointAndKeyValue.Value.KeyValue.Value, out timeout))
                                            {
                                                if (timeout != null)
                                                {
                                                    timeout.Dispose();
                                                }
                                            }

                                            //Setup new incomplete fixation timeout
                                            PointAndKeyValue fixationCentrePointAndKeyValueCopy = fixationCentrePointAndKeyValue.Value; //Access to modified closure
                                            incompleteFixationTimeouts[fixationCentrePointAndKeyValue.Value.KeyValue.Value] =
                                                Observable.Timer(incompleteFixationTtl).Subscribe(_ =>
                                                {
                                                    long removedProgress;
                                                    IDisposable removedTimeout;
                                                    incompleteFixationProgress.TryRemove(fixationCentrePointAndKeyValueCopy.KeyValue.Value, out removedProgress);
                                                    incompleteFixationTimeouts.TryRemove(fixationCentrePointAndKeyValueCopy.KeyValue.Value, out removedTimeout);
                                                    observer.OnNext(new TriggerSignal(null, 0, fixationCentrePointAndKeyValueCopy));
                                                });
                                        }

                                        //Clear the current fixation
                                        fixationCentrePointAndKeyValue = null;
                                    }
                                    else
                                    {
                                        //We have created or added to a fixation - update state vars, publish our progress and reset if necessary
                                        var fixationSpan = latestPointAndKeyValue.Timestamp.Subtract(fixationStart);

                                        long storedProgress;
                                        incompleteFixationProgress.TryGetValue(fixationCentrePointAndKeyValue.Value.KeyValue.Value, out storedProgress);

                                        //Dispose of the timeout for the current fixation as it is in progress again
                                        IDisposable timeout;
                                        if (incompleteFixationTimeouts.TryGetValue(fixationCentrePointAndKeyValue.Value.KeyValue.Value, out timeout))
                                        {
                                            if (timeout != null)
                                            {
                                                timeout.Dispose();
                                            }
                                        }

                                        var progress = (((double)(storedProgress + fixationSpan.Ticks)) / (double)timeToCompleteTrigger.Ticks);

                                        //Publish a high signal if progress is 1 (100%), otherwise just publish progress (filter out 0 as this is a progress reset signal)
                                        if (progress > 0)
                                        {
                                            observer.OnNext(new TriggerSignal(
                                                progress >= 1 ? 1 : (double?)null, progress >= 1 ? 1 : progress, fixationCentrePointAndKeyValue));
                                        }

                                        //Reset if we've just published a high signal
                                        if (progress >= 1)
                                        {
                                            foreach (var t in incompleteFixationTimeouts.Values)
                                            {
                                                t.Dispose();
                                            }

                                            fixationCentrePointAndKeyValue = null;
                                            incompleteFixationProgress.Clear();
                                            incompleteFixationTimeouts.Clear();
                                            lockOnStart = null;

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
