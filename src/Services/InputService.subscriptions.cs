using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Observables.TriggerSignalSources;
using JuliusSweetland.ETTA.Properties;

namespace JuliusSweetland.ETTA.Services
{
    public partial class InputService
    {
        #region Fields

        private IDisposable pointsPerSecondSubscription;
        private IDisposable currentPositionSubscription;
        private IDisposable selectionProgressSubscription;
        private IDisposable selectionTriggerSubscription;
        private IDisposable multiKeySelectionSubscription;
        private CancellationTokenSource mapToDictionaryMatchesCancellationTokenSource;

        private ITriggerSignalSource selectionTriggerSource;

        private TriggerSignal? startMultiKeySelectionTriggerSignal;
        private TriggerSignal? stopMultiKeySelectionTriggerSignal;

        #endregion

        #region Create Points Per Second Subscription

        private void CreatePointsPerSecondSubscription()
        {
            Log.Info("Creating subscription to PointAndKeyValueSource for points per second.");

            pointsPerSecondSubscription = pointAndKeyValueSource.Sequence
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
            Log.Info("Creating subscription to PointAndKeyValueSource for current position.");

            currentPositionSubscription = pointAndKeyValueSource.Sequence
                .Select(tp => new Tuple<Point?, KeyValue?>(
                    tp.Value != null && SelectionMode == SelectionModes.Point 
                        ? tp.Value.Value.Point 
                        : (Point?)null,
                    tp.Value != null && SelectionMode == SelectionModes.Key 
                        ? tp.Value.Value.KeyValue 
                        : null))
                .DistinctUntilChanged()
                .ObserveOnDispatcher() //Subscribe on UI thread
                .Subscribe(PublishCurrentPosition);
        }

        #endregion

        #region Create Selection Progress Subscription

        private void CreateSelectionProgressSubscription(SelectionModes mode)
        {
            Log.Info(string.Format("Creating subscription to {0} SelectionTriggerSource for progress info.", SelectionMode));

            ITriggerSignalSource selectionTriggerSource = null;

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
                        PublishSelectionProgress(new Tuple<PointAndKeyValue?, double>(ts.PointAndKeyValue, ts.Progress.Value));
                    });
            }
        }

        #endregion

        #region Create Selection Subscriptions

        private void CreateSelectionSubscriptions(SelectionModes mode)
        {
            Log.Info(string.Format("Creating subscription to {0} SelectionTriggerSource for selections & results.", SelectionMode));

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
                        if (triggerSignal.PointAndKeyValue.Value.KeyValue != null
                            && (keyboardService.KeyEnabledStates == null || keyboardService.KeyEnabledStates[triggerSignal.PointAndKeyValue.Value.KeyValue.Value]))
                        {
                            Log.Debug("Selection mode is KEY and the key on which the trigger occurred is enabled.");

                            if (keyboardService.KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value.IsDownOrLockedDown()
                                && triggerSignal.PointAndKeyValue.Value.StringIsLetter)
                            {
                                Log.Debug("Multi-key selection is currently enabled and the key on which the trigger occurred is a letter. Publishing the selection and beginning a new multi-key selection capture.");

                                //Multi-key selection is allowed and the trigger occurred on a letter - start a capture
                                startMultiKeySelectionTriggerSignal = triggerSignal;
                                stopMultiKeySelectionTriggerSignal = null;

                                CapturingMultiKeySelection = true;

                                PublishSelection(triggerSignal.PointAndKeyValue.Value);

                                multiKeySelectionSubscription =
                                    CreateMultiKeySelectionSubscription()
                                    .ObserveOnDispatcher()
                                    .Subscribe(
                                        pointsAndKeyValues => ProcessMultiKeySelectionResult(pointsAndKeyValues, triggerSignal),
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
                                        });
                            }
                            else
                            {
                                PublishSelection(triggerSignal.PointAndKeyValue.Value);

                                PublishSelectionResult(new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                    new List<Point> { triggerSignal.PointAndKeyValue.Value.Point },
                                    triggerSignal.PointAndKeyValue.Value.KeyValue.Value.FunctionKey,
                                    triggerSignal.PointAndKeyValue.Value.KeyValue.Value.String,
                                    null));
                            }
                        }
                        else
                        {
                            Log.Debug("Selection mode is KEY, but the trigger occurred off a key or over a disabled key.");
                            audioService.PlaySound(Settings.Default.ErrorSoundFile);
                        }
                    }
                    else if (SelectionMode == SelectionModes.Point)
                    {
                        PublishSelection(triggerSignal.PointAndKeyValue.Value);

                        PublishSelectionResult(new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                            new List<Point> { triggerSignal.PointAndKeyValue.Value.Point }, null, null, null));
                    }
                }
                else
                {
                    Log.Error("TriggerSignal.Signal==1, but TriggerSignal.PointAndKeyValue is null. "
                            + "Discarding trigger as point source is down, or producing stale points. "
                            + "Publishing error instead.");

                    PublishError(this, new ApplicationException("I can not detect where you are directing your attention. Is there a problem with the input device? Is it connected, turned on, calibrated, etc?"));
                }
            }
            else if (CapturingMultiKeySelection)
            {
                //We are capturing and may have received the stop capturing signal
                if ((triggerSignal.Signal >= 1 && Settings.Default.SelectionTriggerStopSignal == TriggerStopSignals.NextHigh)
                    || (triggerSignal.Signal >= -1 && Settings.Default.SelectionTriggerStopSignal == TriggerStopSignals.NextLow))
                {
                    //If we are using a fixation trigger source then the stop signal must occur on a letter
                    if (!(selectionTriggerSource is IFixationTriggerSource)
                        || (triggerSignal.PointAndKeyValue != null && triggerSignal.PointAndKeyValue.Value.StringIsLetter))
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

                var pointAndKeyValueSubscription = pointAndKeyValueSource.Sequence
                    .Where(tp => tp.Value != null) //Filter out stale indicators
                    .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.Value, tp.Timestamp))
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

        private void ProcessMultiKeySelectionResult(IList<Timestamped<PointAndKeyValue>> pointsAndKeyValues, TriggerSignal startSelectionTriggerSignal)
        {
            Log.Debug(string.Format("Multi-key selection captured a set of '{0}' PointAndKeyValues.", pointsAndKeyValues.Count));

            //keyboardService.KeyEnabledStates.DisableAll = true;

            //try
            //{
                if (pointsAndKeyValues.Any())
                {
                    var timeSpan = pointsAndKeyValues.Last().Timestamp.Subtract(pointsAndKeyValues.First().Timestamp);

                    var sequenceThreshold = (int)Math.Round(
                        ((double)pointsAndKeyValues.Count / (double)timeSpan.TotalMilliseconds)
                        * Settings.Default.MultiKeySelectionFixationMinDwellTime.TotalMilliseconds);

                    Log.Debug(string.Format(
                        "Multi-key selection capture lasted {0}ms. Minimum dwell time is {1}ms, or {2} points.",
                        timeSpan.TotalMilliseconds,
                        Settings.Default.MultiKeySelectionFixationMinDwellTime.TotalMilliseconds,
                        sequenceThreshold));

                    string reliableFirstLetter =
                        startMultiKeySelectionTriggerSignal != null
                        && startMultiKeySelectionTriggerSignal.Value.PointAndKeyValue != null
                        && startMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.Value.StringIsLetter
                            ? startMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.Value.String
                            : null;

                    Log.Debug(string.Format(
                        "First letter ('{0}') of multi-key selection capture {1} reliable.",
                        reliableFirstLetter,
                        reliableFirstLetter != null ? "IS" : "IS NOT"));

                    //If we are using a fixation trigger and the stop trigger has
                    //occurred on a letter then it is reliable - use it
                    string reliableLastLetter = selectionTriggerSource is IFixationTriggerSource
                        && stopMultiKeySelectionTriggerSignal != null
                        && stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue != null
                        && stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.Value.StringIsLetter
                            ? stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.Value.String
                            : null;

                    Log.Debug(string.Format(
                            "Last letter ('{0}') of multi-key selection capture {1} reliable.",
                            reliableLastLetter,
                            reliableLastLetter != null ? "IS" : "IS NOT"));

                    if (reliableLastLetter != null)
                    {
                        Log.Debug("Publishing selection event on last letter of multi-key selection capture.");

                        PublishSelection(stopMultiKeySelectionTriggerSignal.Value.PointAndKeyValue.Value);
                    }

                    var reducedSequence = pointsAndKeyValues
                        .Where(tp => tp.Value.KeyValue != null)
                        .Select(tp => tp.Value.KeyValue.Value)
                        .ToList()
                        .ReduceToSequentiallyDistinctLetters(sequenceThreshold, reliableFirstLetter, reliableLastLetter);

                    if (string.IsNullOrEmpty(reducedSequence))
                    {
                        //No useful selection
                        Log.Debug("Multi-key selection capture reduces to nothing useful.");

                        PublishSelectionResult(
                            new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                new List<Point> { startSelectionTriggerSignal.PointAndKeyValue.Value.Point },
                                null, null, null));
                    }
                    else if (reducedSequence.Length == 1)
                    {
                        //The user fixated on one letter - output it
                        Log.Debug("Multi-key selection capture reduces to a single letter.");

                        PublishSelectionResult(
                            new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                pointsAndKeyValues.Select(tp => tp.Value.Point).ToList(),
                                null, reducedSequence, null));
                    }
                    else
                    {
                        //The user fixated on multiple letters - map to dictionary word
                        Log.Debug(string.Format("Multi-key selection capture reduces to multiple letters '{0}'", reducedSequence));

                        List<string> dictionaryMatches =
                            dictionaryService.MapCaptureToEntries(
                                pointsAndKeyValues.ToList(), reducedSequence,
                                true, reliableLastLetter != null,
                                ref mapToDictionaryMatchesCancellationTokenSource,
                                exception => PublishError(this, exception));

                        PublishSelectionResult(
                            new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                pointsAndKeyValues.Select(tp => tp.Value.Point).ToList(),
                                null, null, dictionaryMatches));
                    }
                }
            //}
            //finally
            //{
            //    keyboardService.KeyEnabledStates.DisableAll = false;
            //}
        }

        #endregion

        #region Dispose Selection Subscriptions

        private void DisposeSelectionSubscriptions()
        {
            Log.Info("Disposing of subscriptions to SelectionTriggerSource for selections & results.");

            if (selectionTriggerSubscription != null)
            {
                selectionTriggerSubscription.Dispose();
            }

            if (multiKeySelectionSubscription != null)
            {
                multiKeySelectionSubscription.Dispose();
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
