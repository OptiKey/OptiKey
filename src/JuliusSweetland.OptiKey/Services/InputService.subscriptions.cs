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
        private IDisposable livePositionSubscription;
        private IDisposable selectionProgressSubscription;
        private IDisposable selectionTriggerSubscription;
        private IDisposable multiKeySelectionSubscription;
        private CancellationTokenSource mapToDictionaryMatchesCancellationTokenSource;

        private ITriggerSource selectionTriggerSource;

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
                    tp.Value.Point,
                    SelectionMode == SelectionModes.Key 
                        ? tp.Value.KeyValue 
                        : null))
                .DistinctUntilChanged()
                .ObserveOnDispatcher() //Subscribe on UI thread
                .Subscribe(PublishCurrentPosition);
        }

        #endregion

        #region Create Live Position Subscription

        private void CreateLivePositionSubscription()
        {
            Log.Debug("Creating subscription to PointSource for live position.");

            livePositionSubscription = pointSource.Sequence
                .Where(tp => tp.Value != null)
                .Select(tp => tp.Value.Point)
                .ObserveOnDispatcher() //Subscribe on UI thread
                .Subscribe(PublishLivePosition);
        }

        #endregion

        #region Create Selection Progress Subscription

        private void CreateSelectionProgressSubscription(SelectionModes mode)
        {
            Log.DebugFormat("Creating subscription to {0} SelectionTriggerSource for progress info.", SelectionMode);

            ITriggerSource selectionTriggerSource = null;

            switch (mode)
            {
                case SelectionModes.Key:
                    selectionTriggerSource = keySelectionTriggerSource;
                    break;

                case SelectionModes.Point:
                    selectionTriggerSource = pointSelectionTriggerSource;
                    break;
            }

            if (selectionTriggerSource != null)
            {
                selectionProgressSubscription = selectionTriggerSource.Sequence
                    .Where(ts => ts.Progress != null)
                    .DistinctUntilChanged()
                    .ObserveOnDispatcher()
                    .Subscribe(ts =>
                    {
                        PublishSelectionProgress(new Tuple<PointAndKeyValue, double>(ts.PointAndKeyValue, ts.Progress.Value));
                    });
            }
        }

        #endregion

        #region Create Selection Subscriptions

        private void CreateSelectionSubscriptions(SelectionModes mode)
        {
            Log.DebugFormat("Creating subscription to {0} SelectionTriggerSource for selections & results.", SelectionMode);

            switch (mode)
            {
                case SelectionModes.Key:
                    selectionTriggerSource = keySelectionTriggerSource;
                    break;

                case SelectionModes.Point:
                    selectionTriggerSource = pointSelectionTriggerSource;
                    break;
            }

            if (selectionTriggerSource != null)
            {
                selectionTriggerSubscription = selectionTriggerSource.Sequence
                    .ObserveOnDispatcher()
                    .Subscribe(ProcessSelectionTrigger);
            }
        }

        private void ProcessSelectionTrigger(TriggerSignal triggerSignal)
        {
            if (triggerSignal.Signal >= 1
                && !CapturingMultiKeySelection)
            {
                //We are not currently capturing a multikey selection and have received a high (start) trigger signal
                if (triggerSignal.PointAndKeyValue != null)
                {
                    Log.Debug("Selection trigger signal (with relevent PointAndKeyValue) detected.");

                    if (SelectionMode == SelectionModes.Key)
                    {
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

                                PublishSelection(triggerSignal.PointAndKeyValue);

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
                                PublishSelection(triggerSignal.PointAndKeyValue);

                                PublishSelectionResult(new Tuple<List<Point>, KeyValue, List<string>>(
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
                    else if (SelectionMode == SelectionModes.Point)
                    {
                        PublishSelection(triggerSignal.PointAndKeyValue);

                        PublishSelectionResult(new Tuple<List<Point>, KeyValue, List<string>>(
                            new List<Point> { triggerSignal.PointAndKeyValue.Point }, null, null));
                    }
                }
                else
                {
                    Log.Error("TriggerSignal.Signal==1, but TriggerSignal.PointAndKeyValue is null. "
                            + "Discarding trigger as point source is down, or producing stale points. "
                            + "Publishing error instead.");

                    PublishError(this, new ApplicationException(Resources.TRIGGER_WITHOUT_POSITION_ERROR));
                }
            }
            else if (CapturingMultiKeySelection)
            {
                //We are capturing and may have received the stop capturing signal
                if ((triggerSignal.Signal >= 1 && Settings.Default.MultiKeySelectionTriggerStopSignal == TriggerStopSignals.NextHigh)
                    || (triggerSignal.Signal <= -1 && Settings.Default.MultiKeySelectionTriggerStopSignal == TriggerStopSignals.NextLow))
                {
                    //If we are using a fixation trigger source then the stop signal must occur on a letter
                    if (!(selectionTriggerSource is IFixationTriggerSource)
                        || (triggerSignal.PointAndKeyValue != null && triggerSignal.PointAndKeyValue.StringIsLetter))
                    {
                        Log.Debug("Trigger signal to stop the current multi-key selection capture detected.");

                        stopMultiKeySelectionTriggerSignal = triggerSignal;
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
                        .Where(_ => disposed == false)
                        .Subscribe(i => observer.OnError(new TimeoutException("Multi-key capture has exceeded the maximum duration")));

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
                    string reliableLastLetter = selectionTriggerSource is IFixationTriggerSource
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

                        PublishSelection(stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue);
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

                        PublishSelectionResult(result);
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

            if (selectionTriggerSubscription != null)
            {
                selectionTriggerSubscription.Dispose();
                selectionTriggerSubscription = null;
            }

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
