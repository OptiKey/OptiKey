// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.TriggerSources;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Services
{
    public partial class InputService
    {
        #region Fields

        private IDisposable pointsPerSecondSubscription;
        private IDisposable currentPositionSubscription;
        private IDisposable eyeGestureTriggerSubscription;
        private IDisposable multiKeySelectionSubscription;
        private CancellationTokenSource mapToDictionaryMatchesCancellationTokenSource;

        private List<IDisposable> activeSelectionTriggerSubscriptions;
        private List<IDisposable> activeSelectionProgressSubscriptions;

        private TriggerSignal? startMultiKeySelectionTriggerSignal;
        private TriggerSignal? stopMultiKeySelectionTriggerSignal;

        #endregion

        #region Create Points Per Second Subscription

        private void CreatePointsPerSecondSubscription()
        {
            Log.Debug("Creating subscription to PointSource for points per second.");

            pointsPerSecondSubscription = pointSource.Sequence
                .Where(tp => tp.Value != null) //Filter out stale indicators - we only want 'live'/useful points in our count
                .Buffer(new TimeSpan(0, 0, 0, 1))
                .Select(points => points.Count)
                .ObserveOnDispatcher() //Subscribe on UI thread
                .Subscribe(PublishPointsPerSecond);
        }

        #endregion

        #region Create Current Position Subscription

        private void CreateCurrentPositionSubscription()
        {
            Log.Debug("Creating subscription to PointSource for current position.");

            currentPositionSubscription = pointSource.Sequence
                .Where(tp => tp.Value != null)
                .Select(tp => new Tuple<Point, KeyValue>(
                    tp.Value.Point, tp.Value.KeyValue))
                .ObserveOnDispatcher() //Subscribe on UI thread
                .Subscribe(PublishCurrentPosition);
        }

        #endregion

        #region Create Selection Progress Subscription        

        private void DisposeAllAndClear(List<IDisposable> list)
        {
            foreach (IDisposable disp in list)
            {
                disp.Dispose();
            }
            list.RemoveAll(s => true);
        }

        private void CreateSelectionProgressSubscription(SelectionModes mode)
        {
            Log.DebugFormat("Creating subscription to {0} SelectionTriggerSource for progress info.", SelectionMode);
            
            if (eyeGestureTriggerSource != null && eyeGestureTriggerSubscription == null)
            {
                eyeGestureTriggerSubscription = eyeGestureTriggerSource.Sequence
                    .ObserveOnDispatcher()
                    .Subscribe(ProcessGestureTrigger);
            }
              
            DisposeAllAndClear(activeSelectionProgressSubscriptions);

            activeSelectionProgressSubscriptions.Add(keySelectionTriggerSource.Sequence
                        .Where(ts => ts.Progress != null)
                        .DistinctUntilChanged()
                        .ObserveOnDispatcher()
                        .Subscribe(ts =>
                        {
                            PublishSelectionProgress(new Tuple<TriggerTypes, PointAndKeyValue, double>(
                                TriggerTypes.Key, ts.PointAndKeyValue, ts.Progress.Value));
                        }));

            if (pointSelectionTriggerSource is IFixationTriggerSource)
            {
                IFixationTriggerSource fixationTriggerSource = pointSelectionTriggerSource as IFixationTriggerSource;
                fixationTriggerSource.AllowPointsOverKeys = selectionMode == SelectionModes.SinglePoint;
            }


            // Point selection active if any mouse actions underway
            if (mode == SelectionModes.SinglePoint || mode == SelectionModes.ContinuousPoints)
            {
                activeSelectionProgressSubscriptions.Add(pointSelectionTriggerSource.Sequence
                        .Where(ts => ts.Progress != null)
                        .DistinctUntilChanged()
                        .ObserveOnDispatcher()
                        .Subscribe(ts =>
                        {
                            PublishSelectionProgress(new Tuple<TriggerTypes, PointAndKeyValue, double>(
                                TriggerTypes.Point, ts.PointAndKeyValue, ts.Progress.Value));
                        }));
            }
        }

        #endregion

        #region Create Selection Subscriptions

        private void CreateSelectionSubscriptions(SelectionModes mode)
        {
            Log.DebugFormat("Creating subscription to {0} SelectionTriggerSource for selections & results.", SelectionMode);
           
            if (eyeGestureTriggerSource != null && eyeGestureTriggerSubscription == null)
            {
                eyeGestureTriggerSubscription = eyeGestureTriggerSource.Sequence
                    .ObserveOnDispatcher()
                    .Subscribe(ProcessGestureTrigger);
            }

            DisposeAllAndClear(activeSelectionTriggerSubscriptions);

            // Key selection is always enabled, but in the case of single point selection we will only allow
            // the current "mouse action" key to be selected, to allow for locking. 
            activeSelectionTriggerSubscriptions.Add(keySelectionTriggerSource.Sequence
                .Where(ts => ts.Signal != null)
                .ObserveOnDispatcher()
                .Subscribe(ProcessKeySelectionTrigger));

            // Point selection active if any mouse actions underway
            if (mode == SelectionModes.SinglePoint || mode == SelectionModes.ContinuousPoints)
                activeSelectionTriggerSubscriptions.Add(pointSelectionTriggerSource.Sequence
                   .Where(ts => ts.Signal != null)
                   .ObserveOnDispatcher()
                   .Subscribe(ProcessPointSelectionTrigger));

        }

        private async void ProcessKeySelectionTrigger(TriggerSignal triggerSignal)
        {
            if (triggerSignal.Signal >= 1
                && !CapturingMultiKeySelection)
            {
                //We are not currently capturing a multikey selection and have received a high (start) trigger signal
                if (triggerSignal.PointAndKeyValue != null)
                {
                    Log.Debug("Key selection trigger signal (with relevant PointAndKeyValue) detected.");
                    
                    if (triggerSignal.PointAndKeyValue.KeyValue != null
                        && (keyStateService.KeyEnabledStates == null || keyStateService.KeyEnabledStates[triggerSignal.PointAndKeyValue.KeyValue]))
                    {
                        Log.Debug("Selection mode is KEY and the key on which the trigger occurred is enabled.");

                        if (MultiKeySelectionSupported
                            && keyStateService.KeyEnabledStates[KeyValues.MultiKeySelectionIsOnKey] //It is possible for MultiKeySelectionIsOnKey to be down/locked down even though it is disabled - check for this
                            && keyStateService.KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value.IsDownOrLockedDown()
                            && triggerSignal.PointAndKeyValue.KeyValue != null
                            && KeyValues.MultiKeySelectionKeys.Contains(triggerSignal.PointAndKeyValue.KeyValue)
                            && !KeyValues.CombiningKeys.Any(key => keyStateService.KeyDownStates[key].Value.IsDownOrLockedDown())) //Do not start if any combining ("dead") keys are down
                        {
                            Log.Debug("Multi-key selection is currently enabled and the key on which the trigger occurred is a letter. Publishing the selection and beginning a new multi-key selection capture.");

                            //Multi-key selection is allowed and the trigger occurred on a letter - start a capture
                            startMultiKeySelectionTriggerSignal = triggerSignal;
                            stopMultiKeySelectionTriggerSignal = null;

                            CapturingMultiKeySelection = true;

                            PublishSelection(TriggerTypes.Key, triggerSignal.PointAndKeyValue);

                            //Set the key's IsHighlighted property in order to show the green border
                            keyStateService.KeyHighlightStates[triggerSignal.PointAndKeyValue.KeyValue].Value = true;

                            multiKeySelectionSubscription =
                                CreateMultiKeySelectionSubscription()
                                    .ObserveOnDispatcher()
                                    .Subscribe(
                                        async pointsAndKeyValues => await ProcessMultiKeySelectionResult(pointsAndKeyValues, triggerSignal),
                                        (exception =>
                                        {
                                            PublishError(this, exception);

                                            stopMultiKeySelectionTriggerSignal = null;
                                            CapturingMultiKeySelection = false;
                                        }),
                                        () =>
                                        {
                                            Log.Debug("Multi-key selection capture has completed.");

                                            stopMultiKeySelectionTriggerSignal = null;
                                            CapturingMultiKeySelection = false;

                                            //Set the key's IsHighlighted to false in order to remove the green border
                                            keyStateService.KeyHighlightStates[triggerSignal.PointAndKeyValue.KeyValue].Value = false;
                                        });
                        }
                        else
                        {
                            PublishSelection(TriggerTypes.Key, triggerSignal.PointAndKeyValue);

                            await Task.Delay(20); //Add a short delay to give time for the selection animation 

                            PublishSelectionResult(new Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>(
                                TriggerTypes.Key,
                                new List<Point> { triggerSignal.PointAndKeyValue.Point },
                                triggerSignal.PointAndKeyValue.KeyValue,
                                null));
                        }
                    }
                    else
                    {
                        Log.Debug("Selection mode is KEY, but the trigger occurred away from a key or over a disabled key.");
                    }                    
                }
                else
                {
                    Log.Error("TriggerSignal.Signal==1, but TriggerSignal.PointAndKeyValue is null. "
                            + "Discarding trigger as point source is down, or producing stale points. "
                            + "Publishing error instead.");

                    if (!Settings.Default.SuppressTriggerWithoutPositionError)
                    {
                        PublishError(this, new ApplicationException(Resources.TRIGGER_WITHOUT_POSITION_ERROR));
                    }
                }
            }
            else if (CapturingMultiKeySelection)
            {
                //We are capturing and may have received the stop capturing signal
                if ((triggerSignal.Signal >= 1 && Settings.Default.MultiKeySelectionTriggerStopSignal == TriggerStopSignals.NextHigh)
                    || (triggerSignal.Signal <= -1 && Settings.Default.MultiKeySelectionTriggerStopSignal == TriggerStopSignals.NextLow))
                {
                    //If we are using a fixation trigger source then the stop signal must occur on a letter                    
                    if (!(activeSelectionTriggerSubscriptions.Any((s) => s is KeyFixationSource)) // equivalent to "if the key selection source is a fixation one"
                        || (triggerSignal.PointAndKeyValue != null && triggerSignal.PointAndKeyValue.StringIsLetter))
                    {
                        Log.Debug("Trigger signal to stop the current multi-key selection capture detected.");

                        stopMultiKeySelectionTriggerSignal = triggerSignal;
                    }
                }
            }
        }
        private void ProcessPointSelectionTrigger(TriggerSignal triggerSignal)
        {
            //FIXME: Is it okay to permit a point selection when a multikey selection is underway?  We need to test if this results
            // in undefined behaviour or poor UX
            // In vanilla OK keyboards it's unlikely to happen as keys get disabled while multikey selection in progress
            if (triggerSignal.Signal >= 1)
            {
                if (triggerSignal.PointAndKeyValue != null)
                {
                    Log.Debug("Point selection trigger signal (with relevant PointAndKeyValue) detected.");
                   
                    PublishSelection(TriggerTypes.Point, triggerSignal.PointAndKeyValue);

                    PublishSelectionResult(new Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>(
                        TriggerTypes.Point,
                        new List<Point> { triggerSignal.PointAndKeyValue.Point }, null, null));
                    
                }
                else
                {
                    Log.Error("TriggerSignal.Signal==1, but TriggerSignal.PointAndKeyValue is null. "
                            + "Discarding trigger as point source is down, or producing stale points. "
                            + "Publishing error instead.");

                    if (!Settings.Default.SuppressTriggerWithoutPositionError)
                    {
                        PublishError(this, new ApplicationException(Resources.TRIGGER_WITHOUT_POSITION_ERROR));
                    }
                }
            }            
        }

        private async void ProcessGestureTrigger(TriggerSignal triggerSignal)
        { 
            // TODO: Consider what states we might want gestures to *not* trigger in
            // e.g. should it be allowed in "Point selection" mode? I don't think it would have been processed previously,
            // but if we are allowing continuous / repeat mouse actions then it is probably important gestures can come through too.
            // Do we want to allow a gesture to trigger a disabled key? 
            if (CapturingMultiKeySelection)
                return;

            if (SelectionMode != SelectionModes.Keys)
            {
                Log.InfoFormat("Ignoring gesture during point selection");
                return;
            }

            if (triggerSignal.Signal >= 1)            
            {
                if (triggerSignal.PointAndKeyValue != null)
                {
                    Log.Debug("Gesture trigger signal (with relevant PointAndKeyValue) detected.");

                    {
                        if (triggerSignal.PointAndKeyValue.KeyValue != null
                            && (keyStateService.KeyEnabledStates == null || keyStateService.KeyEnabledStates[triggerSignal.PointAndKeyValue.KeyValue]))
                        {                            
                            PublishSelection(TriggerTypes.Gesture, triggerSignal.PointAndKeyValue);

                            await Task.Delay(20); //Add a short delay to give time for the selection animation 

                            PublishSelectionResult(new Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>(
                                TriggerTypes.Gesture, 
                                new List<Point> { triggerSignal.PointAndKeyValue.Point },
                                triggerSignal.PointAndKeyValue.KeyValue,
                                null));
                            
                        }
                        else
                        {
                            Log.Debug("Selection mode is KEY, but the trigger occurred away from a key or over a disabled key.");
                        }
                    }                    
                }
                else
                {
                    Log.Error("TriggerSignal.Signal==1, but TriggerSignal.PointAndKeyValue is null. "
                            + "Discarding trigger as point source is down, or producing stale points. "
                            + "Publishing error instead.");

                    if (!Settings.Default.SuppressTriggerWithoutPositionError)
                    {
                        PublishError(this, new ApplicationException(Resources.TRIGGER_WITHOUT_POSITION_ERROR));
                    }
                }
            }            
        }

        private IObservable<IList<Timestamped<PointAndKeyValue>>> CreateMultiKeySelectionSubscription()
        {
            return Observable.Create<IList<Timestamped<PointAndKeyValue>>>(observer =>
            {
                bool disposed = false;

                Action disposeAllSubscriptions = null;

                var intervalSubscription =
                    Observable.Interval(Settings.Default.MultiKeySelectionMaxDuration)
                        .ObserveOnDispatcher()
                        .Where(_ => disposed == false)
                        .Subscribe(i =>
                        {
                            keyStateService.ClearKeyHighlightStates();
                            observer.OnError(new TimeoutException("Multi-key capture has exceeded the maximum duration"));
                        });

                var pointAndKeyValueSubscription = pointSource.Sequence
                    .Where(tp => tp.Value != null) //Filter out stale indicators
                    .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value, tp.Timestamp))
                    .TakeWhile(tp => stopMultiKeySelectionTriggerSignal == null)
                    .ToList()
                    .Subscribe(points =>
                    {
                        observer.OnNext(points);
                        observer.OnCompleted();
                    });

                disposeAllSubscriptions = () =>
                {
                    disposed = true;

                    if (intervalSubscription != null)
                    {
                        intervalSubscription.Dispose();
                        intervalSubscription = null;
                    }

                    if (pointAndKeyValueSubscription != null)
                    {
                        pointAndKeyValueSubscription.Dispose();
                        pointAndKeyValueSubscription = null;
                    }
                };

                return disposeAllSubscriptions;
            });
        }

        private async Task ProcessMultiKeySelectionResult(
            IList<Timestamped<PointAndKeyValue>> pointsAndKeyValues, 
            TriggerSignal startSelectionTriggerSignal)
        {
            Log.DebugFormat("Multi-key selection captured a set of '{0}' PointAndKeyValues.", pointsAndKeyValues.Count);

            RequestSuspend(); //Pause everything (i.e. processing new points) while we perform the (CPU bound) word matching

            try
            {
                if (pointsAndKeyValues.Any())
                {
                    var timeSpan = pointsAndKeyValues.Last().Timestamp.Subtract(pointsAndKeyValues.First().Timestamp);

                    var sequenceThreshold = (int)Math.Round(
                        ((double)pointsAndKeyValues.Count / (double)timeSpan.TotalMilliseconds)
                        * Settings.Default.MultiKeySelectionFixationMinDwellTime.TotalMilliseconds);

                    Log.DebugFormat(
                        "Multi-key selection capture lasted {0}ms. Minimum dwell time is {1}ms, or {2} points.",
                        timeSpan.TotalMilliseconds,
                        Settings.Default.MultiKeySelectionFixationMinDwellTime.TotalMilliseconds,
                        sequenceThreshold);

                    //Always assume the start trigger is reliable if it occurs on a letter
                    string reliableFirstLetter =
                        startMultiKeySelectionTriggerSignal != null
                        && startMultiKeySelectionTriggerSignal.Value.PointAndKeyValue != null
                        && startMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.StringIsLetter
                            ? startMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.String
                            : null;

                    Log.DebugFormat(
                        "First letter ('{0}') of multi-key selection capture {1} reliable.",
                        reliableFirstLetter,
                        reliableFirstLetter != null ? "IS" : "IS NOT");

                    //If we are using a fixation trigger and the stop trigger has occurred on a letter then it is reliable - use it
                    string reliableLastLetter = keySelectionTriggerSource is KeyFixationSource
                        && stopMultiKeySelectionTriggerSignal != null
                        && stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue != null
                        && stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.StringIsLetter
                            ? stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.String
                            : null;

                    Log.DebugFormat(
                            "Last letter ('{0}') of multi-key selection capture {1} reliable.",
                            reliableLastLetter,
                            reliableLastLetter != null ? "IS" : "IS NOT");

                    if (reliableLastLetter != null)
                    {
                        Log.Debug("Publishing selection event on last letter of multi-key selection capture.");

                        PublishSelection(TriggerTypes.Key, stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue);
                    }

                    //Why am I wrapping this call in a Task.Run? Internally the MapCaptureToEntries method uses PLINQ which also blocks the UI thread - this frees it up.
                    //This cannot be done inside the MapCaptureToEntries method as the method takes a ref param, which cannot be used inside an anonymous delegate or lambda.
                    //The method cannot be made awaitable as async/await also does not support ref params.
                    Tuple<List<Point>, KeyValue, List<string>> result = null;
                    await Task.Run(() =>
                    {
                        result = dictionaryService.MapCaptureToEntries(
                            pointsAndKeyValues.ToList(), sequenceThreshold,
                            reliableFirstLetter, reliableLastLetter,
                            ref mapToDictionaryMatchesCancellationTokenSource,
                            exception => PublishError(this, exception));
                    });

                    if (result != null)
                    {
                        if (result.Item2 == null && 
                            (result.Item3 == null || !result.Item3.Any()))
                        {
                            //Nothing useful in the result - play error message. Publish anyway as the points can be rendered in debugging mode.
                            audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
                        }

                        PublishSelectionResult(new Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>(
                            TriggerTypes.Key, result.Item1, result.Item2, result.Item3));
                    }
                }
            }
            finally
            {
                RequestResume();
            }
        }

        #endregion

        #region Dispose Selection Subscriptions

        private void DisposeSelectionSubscriptions()
        {
            Log.Debug("Disposing of subscriptions to SelectionTriggerSource for selections & results.");

            if (eyeGestureTriggerSubscription != null)
            {
                eyeGestureTriggerSubscription.Dispose();
                eyeGestureTriggerSubscription = null;
            }

            DisposeAllAndClear(activeSelectionTriggerSubscriptions);
            DisposeAllAndClear(activeSelectionProgressSubscriptions);

            if (multiKeySelectionSubscription != null)
            {
                multiKeySelectionSubscription.Dispose();
                multiKeySelectionSubscription = null;
            }
        }

        #endregion
        
        #region Dispose Multi Key Selection

        private void DisposeMultiKeySelection()
        {
            Log.Debug("DisposeMultiKeySelection called.");

            if (multiKeySelectionSubscription != null)
            {
                multiKeySelectionSubscription.Dispose();
            }

            if (mapToDictionaryMatchesCancellationTokenSource != null)
            {
                mapToDictionaryMatchesCancellationTokenSource.Cancel();
            }

            CapturingMultiKeySelection = false;
        }

        #endregion
    }
}
