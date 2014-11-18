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
using JuliusSweetland.ETTA.Observables.PointAndKeyValueSources;
using JuliusSweetland.ETTA.Observables.TriggerSignalSources;
using JuliusSweetland.ETTA.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Services
{
    public class InputService : BindableBase, IInputService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IDisposable pointsPerSecondSubscription;
        private IDisposable currentPositionSubscription;
        private IDisposable selectionProgressSubscription;
        private IDisposable selectionTriggerSubscription;
        private IDisposable multiKeySelectionPointsSubscription;
        private CancellationTokenSource mapToDictionaryMatchesCancellationTokenSource;

        private readonly IDictionaryService dictionaryService;
        private readonly IPointAndKeyValueSource pointAndKeyValueSource;
        private readonly ITriggerSignalSource keySelectionTriggerSource;
        private readonly ITriggerSignalSource pointSelectionTriggerSource;
        
        private KeyEnabledStates keyEnabledStates;
        
        private event EventHandler<int> pointsPerSecondEvent;
        private event EventHandler<Tuple<Point?, KeyValue?>> currentPositionEvent;
        private event EventHandler<Tuple<PointAndKeyValue?, double>> selectionProgressEvent;
        private event EventHandler<PointAndKeyValue> selectionEvent;
        private event EventHandler<Tuple<List<Point>, FunctionKeys?, string, List<string>>> selectionResultEvent;

        #endregion

        #region Ctor

        public InputService(
            IDictionaryService dictionaryService,
            IPointAndKeyValueSource pointAndKeyValueSource,
            ITriggerSignalSource keySelectionTriggerSource,
            ITriggerSignalSource pointSelectionTriggerSource)
        {
            this.dictionaryService = dictionaryService;
            this.pointAndKeyValueSource = pointAndKeyValueSource;
            this.keySelectionTriggerSource = keySelectionTriggerSource;
            this.pointSelectionTriggerSource = pointSelectionTriggerSource;
        }

        #endregion

        #region Properties

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            set
            {
                Log.Debug("PointToKeyValueMap property changed (setter called).");

                if (pointAndKeyValueSource != null)
                {
                    pointAndKeyValueSource.PointToKeyValueMap = value;
                }

                DiscardMultiKeySelection();
            }
        }

        public KeyEnabledStates KeyEnabledStates 
        {
            get { return keyEnabledStates; }
            set
            {
                keyEnabledStates = value;

                //Fixation key triggers also need the enabled state info
                var fixationTrigger = keySelectionTriggerSource as IFixationTriggerSource;
                if (fixationTrigger != null)
                {
                    fixationTrigger.KeyEnabledStates = value;
                }
            }
        }

        private SelectionModes selectionMode;
        public SelectionModes SelectionMode
        {
            get { return selectionMode; }
            set
            {
                if (SetProperty(ref selectionMode, value))
                {
                    Log.Debug(string.Format("SelectionMode changed to {0}", value));

                    if (selectionProgressSubscription != null)
                    {
                        Log.Debug("Disposing of selection progress subscription");
                        selectionTriggerSubscription.Dispose();
                    }

                    if (selectionTriggerSubscription != null)
                    {
                        Log.Debug("Disposing of selection trigger subscription");
                        selectionTriggerSubscription.Dispose();
                    }

                    if (multiKeySelectionPointsSubscription != null)
                    {
                        Log.Debug("Disposing of multi-key selection points subscription");
                        multiKeySelectionPointsSubscription.Dispose();
                    }

                    if (selectionProgressEvent != null)
                    {
                        Log.Debug("SelectionProgress event has at least one listener - subscribing to relevent selection progress source");
                        CreateSelectionProgressSubscription(value);
                    }

                    if (selectionEvent != null 
                        || selectionResultEvent != null)
                    {
                        Log.Debug("Selection event has at least one listener - subscribing to relevent selection trigger source");
                        CreateSelectionSubscriptions(value);
                    }
                }
            }
        }

        private bool capturingMultiKeySelection;
        public bool CapturingMultiKeySelection
        {
            get { return capturingMultiKeySelection; }
            set { SetProperty(ref capturingMultiKeySelection, value); }
        }

        #endregion

        #region Events

        #region Points Per Second

        public event EventHandler<int> PointsPerSecond
        {
            add
            {
                if (pointsPerSecondEvent == null)
                {
                    Log.Info("PointsPerSecond event has first subscriber.");
                }

                pointsPerSecondEvent += value;

                if (pointsPerSecondSubscription == null)
                {
                    CreatePointsPerSecondSubscription();
                }
            }
            remove
            {
                pointsPerSecondEvent -= value;

                if (pointsPerSecondEvent == null)
                {
                    Log.Info("Last listener of PointsPerSecond event has unsubscribed. Disposing of pointsPerSecondSubscription.");

                    if (pointsPerSecondSubscription != null)
                    {
                        pointsPerSecondSubscription.Dispose();
                    }
                }
            }
        }

        #endregion

        #region Current Position

        public event EventHandler<Tuple<Point?, KeyValue?>> CurrentPosition
        {
            add
            {
                if (currentPositionEvent == null)
                {
                    Log.Info("CurrentPosition event has first subscriber.");
                }

                currentPositionEvent += value;

                if (currentPositionSubscription == null)
                {
                    CreateCurrentPositionSubscription();
                }
            }
            remove
            {
                currentPositionEvent -= value;

                if (pointsPerSecondEvent == null)
                {
                    Log.Info("Last listener of CurrentPosition event has unsubscribed. Disposing of currentPositionSubscription.");

                    if (currentPositionSubscription != null)
                    {
                        currentPositionSubscription.Dispose();
                    }
                }
            }
        }

        #endregion

        #region Selection Progress

        public event EventHandler<Tuple<PointAndKeyValue?, double>> SelectionProgress
        {
            add
            {
                if (selectionProgressEvent == null)
                {
                    Log.Info("SelectionProgress event has first subscriber.");
                }

                selectionProgressEvent += value;
                
                if (selectionProgressSubscription == null)
                {
                    CreateSelectionProgressSubscription(SelectionMode);
                }
            }
            remove
            {
                selectionProgressEvent -= value;
                
                if (selectionProgressEvent == null)
                {
                    Log.Info("Last subscriber of SelectionProgress event has unsubscribed. Disposing of selectionProgressSubscription.");
                    
                    if (selectionProgressSubscription != null)
                    {
                        selectionProgressSubscription.Dispose();
                    }
                }
            }
        }

        #endregion

        #region Selection

        public event EventHandler<PointAndKeyValue> Selection
        {
            add
            {
                if (selectionEvent == null)
                {
                    Log.Info("Selection event has first subscriber.");
                }

                selectionEvent += value;

                if (selectionTriggerSubscription == null)
                {
                    CreateSelectionSubscriptions(SelectionMode);
                }
            }
            remove
            {
                selectionEvent -= value;

                if (selectionEvent == null)
                {
                    Log.Info("Last subscriber of Selection event has unsubscribed.");

                    if (selectionResultEvent == null)
                    {
                        DisposeSelectionSubscriptions();
                    }
                }
            }
        }

        #endregion

        #region Selection Result

        public event EventHandler<Tuple<List<Point>, FunctionKeys?, string, List<string>>> SelectionResult
        {
            add
            {
                if (selectionResultEvent == null)
                {
                    Log.Info("Selection Result event has first subscriber.");
                }

                selectionResultEvent += value;

                if (selectionTriggerSubscription == null)
                {
                    CreateSelectionSubscriptions(SelectionMode);
                }
            }
            remove
            {
                selectionResultEvent -= value;

                if (selectionResultEvent == null)
                {
                    Log.Info("Last subscriber of SelectionResult event has unsubscribed.");

                    if (selectionEvent == null)
                    {
                        DisposeSelectionSubscriptions();
                    }
                }
            }
        }

        #endregion

        public event EventHandler<Exception> Error;

        #endregion

        #region Publish Events

        #region Publish Points Per Second

        private void PublishPointsPerSecond(int pointsPerSecond)
        {
            if (pointsPerSecondEvent != null)
            {
                Log.Debug(string.Format("Publishing PointsPerSecond event: {0}", pointsPerSecond));

                pointsPerSecondEvent(this, pointsPerSecond);
            }
        }

        #endregion

        #region Publish Current Position

        private void PublishCurrentPosition(Tuple<Point?, KeyValue?> currentPosition)
        {
            if (currentPositionEvent != null)
            {
                Log.Debug(string.Format("Publishing CurrentPosition event with Point:{0} KeyValue:{1}", currentPosition.Item1, currentPosition.Item2));

                currentPositionEvent(this, currentPosition);
            }
        }

        #endregion

        #region Publish Selection Progress

        private void PublishSelectionProgress(Tuple<PointAndKeyValue?, double> selectionProgress)
        {
            if (selectionProgressEvent != null)
            {
                Log.Debug(string.Format("Publishing SelectionProgress event: {0}=>{1}", selectionProgress.Item1, selectionProgress.Item2));

                selectionProgressEvent(this, selectionProgress);
            }
        }

        #endregion

        #region Publish Selection

        private void PublishSelection(PointAndKeyValue selection)
        {
            if (selectionEvent != null)
            {
                Log.Debug(string.Format("Publishing Selection event with PointAndKeyValue:{0}", selection));

                selectionEvent(this, selection);
            }
        }

        #endregion

        #region Publish Selection Result

        private void PublishSelectionResult(Tuple<List<Point>, FunctionKeys?, string, List<string>> selectionResult)
        {
            if (selectionResultEvent != null)
            {
                Log.Debug(string.Format("Publishing Selection Result event with {0} points, FunctionKey:{1}, String:{2}, Match {3} (count:{4})",
                        selectionResult.Item1 != null ? selectionResult.Item1.Count : (int?)null,
                        selectionResult.Item2, selectionResult.Item3,
                        selectionResult.Item4 != null ? selectionResult.Item4.First() : null,
                        selectionResult.Item4 != null ? selectionResult.Item4.Count : (int?)null));

                selectionResultEvent(this, selectionResult);
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            if (Error != null)
            {
                Log.Error("Publishing Error event", ex);

                Error(sender, ex);
            }
        }

        #endregion

        #endregion

        #region Manage Subscriptions

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
                TriggerSignal? stopTriggerSignal;

                selectionTriggerSubscription = selectionTriggerSource.Sequence
                    .ObserveOnDispatcher()
                    .Subscribe(ts =>
                    {
                        if (ts.Signal >= 1
                            && !CapturingMultiKeySelection)
                        {
                            //We are not currently capturing a multikey selection and have received a high (start) trigger signal
                            if (ts.PointAndKeyValue != null)
                            {
                                Log.Debug("Selection trigger signal (with relevent PointAndKeyValue) detected.");

                                if (SelectionMode == SelectionModes.Key
                                    && ts.PointAndKeyValue.Value.KeyValue != null
                                    && (KeyEnabledStates == null || KeyEnabledStates[ts.PointAndKeyValue.Value.KeyValue.Value]))
                                {
                                    Log.Debug("Selection mode is KEY and the key on which the trigger occurred is enabled.");

                                    if (Settings.Default.MultiKeySelectionEnabled
                                        && ts.PointAndKeyValue.Value.StringIsLetter)
                                    {
                                        Log.Debug("Multi-key selection is currently enabled and the key on which the trigger occurred is a letter. Publishing the selection and beginning a new multi-key selection capture.");

                                        //Multi-key selection is allowed and the trigger occurred on a letter - start a capture
                                        PublishSelection(ts.PointAndKeyValue.Value);

                                        //Set up start and stop trigger signals
                                        var startTriggerSignal = ts;
                                        stopTriggerSignal = null;

                                        CapturingMultiKeySelection = true;

                                        multiKeySelectionPointsSubscription =
                                            Observable.Create<IList<Timestamped<PointAndKeyValue>>>(observer =>
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
                                                    .TakeWhile(tp => stopTriggerSignal == null)
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
                                            })
                                            .ObserveOnDispatcher()
                                            .Subscribe(points =>
                                            {
                                                Log.Debug(string.Format("Multi-key selection capture has returned a set of '{0}' PointAndKeyValues.", points.Count));

                                                if (points.Any())
                                                {
                                                    var timeSpan = points.Last().Timestamp.Subtract(points.First().Timestamp);

                                                    var sequenceThreshold = (int)Math.Round(
                                                        ((double)points.Count / (double)timeSpan.TotalMilliseconds)
                                                        * Settings.Default.MultiKeySelectionFixationMinDwellTime.TotalMilliseconds);

                                                    Log.Debug(string.Format(
                                                        "Multi-key selection capture lasted {0}ms. Minimum dwell time is {1}ms, or {2} points.",
                                                        timeSpan.TotalMilliseconds,
                                                        Settings.Default.MultiKeySelectionFixationMinDwellTime.TotalMilliseconds,
                                                        sequenceThreshold));

                                                    var keyValues = points
                                                        .Where(tp => tp.Value.KeyValue != null)
                                                        .Select(tp => tp.Value.KeyValue.Value)
                                                        .ToList();

                                                    string reliableFirstLetter =
                                                        startTriggerSignal.PointAndKeyValue != null
                                                        && startTriggerSignal.PointAndKeyValue.Value.StringIsLetter
                                                            ? startTriggerSignal.PointAndKeyValue.Value.String
                                                            : null;

                                                    Log.Debug(string.Format(
                                                        "First letter ('{0}') of multi-key selection capture {1} reliable.", 
                                                        reliableFirstLetter,
                                                        reliableFirstLetter != null ? "IS" : "IS NOT"));

                                                    //If we are using a fixation trigger and the stop trigger has
                                                    //occurred on a letter then it is reliable - use it
                                                    string reliableLastLetter = selectionTriggerSource is IFixationTriggerSource
                                                        && stopTriggerSignal != null
                                                        && stopTriggerSignal.Value.PointAndKeyValue != null
                                                        && stopTriggerSignal.Value.PointAndKeyValue.Value.StringIsLetter
                                                            ? stopTriggerSignal.Value.PointAndKeyValue.Value.String
                                                            : null;

                                                    Log.Debug(string.Format(
                                                            "Last letter ('{0}') of multi-key selection capture {1} reliable.",
                                                            reliableLastLetter,
                                                            reliableLastLetter != null ? "IS" : "IS NOT"));

                                                    if (reliableLastLetter != null)
                                                    {
                                                        Log.Debug("Publishing selection event on last letter of multi-key selection capture.");

                                                        PublishSelection(stopTriggerSignal.Value.PointAndKeyValue.Value);
                                                    }

                                                    var reducedSequence = keyValues.ReduceToSequentiallyDistinctLetters(
                                                            sequenceThreshold, reliableFirstLetter, reliableLastLetter);

                                                    if (string.IsNullOrEmpty(reducedSequence))
                                                    {
                                                        //No useful selection
                                                        Log.Debug("Multi-key selection capture reduces to nothing useful.");

                                                        PublishSelectionResult(
                                                            new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                                                new List<Point> { ts.PointAndKeyValue.Value.Point },
                                                                null, null, null));
                                                    }
                                                    else if (reducedSequence.Length == 1)
                                                    {
                                                        //The user fixated on one letter - output it
                                                        Log.Debug("Multi-key selection capture reduces to a single letter.");

                                                        PublishSelectionResult(
                                                            new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                                                points.Select(tp => tp.Value.Point).ToList(),
                                                                null, reducedSequence, null));
                                                    }
                                                    else
                                                    {
                                                        //The user fixated on multiple letters - map to dictionary word
                                                        Log.Debug(string.Format("Multi-key selection capture reduces to '{0}'", reducedSequence));

                                                        //Remove diacritics and make uppercase
                                                        reducedSequence = reducedSequence.RemoveDiacritics().ToUpper();

                                                        Log.Debug(string.Format("Removing diacritics leaves us with '{0}'", reducedSequence));

                                                        //Reduce again - by removing diacritics we might now have adjacent 
                                                        //letters which are the same (the dictionary hashes do not)
                                                        var hash = new List<Char>();
                                                        foreach (char c in reducedSequence)
                                                        {
                                                            if (!hash.Any() || !hash[hash.Count - 1].Equals(c))
                                                            {
                                                                hash.Add(c);
                                                            }
                                                        }

                                                        var hashAsString = new string(hash.ToArray());

                                                        Log.Debug(string.Format("Reducing the sequence again leaves us with '{0}'", hashAsString));

                                                        List<string> dictionaryMatches =
                                                            hashAsString.MapToDictionaryMatches(
                                                                true, reliableLastLetter != null, dictionaryService,
                                                                ref mapToDictionaryMatchesCancellationTokenSource,
                                                                exception => PublishError(this, exception));

                                                        PublishSelectionResult(
                                                            new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                                                points.Select(tp => tp.Value.Point).ToList(),
                                                                null, null, dictionaryMatches));
                                                    }
                                                }
                                            },
                                            (exception =>
                                            {
                                                PublishError(this, exception);

                                                stopTriggerSignal = null;
                                                CapturingMultiKeySelection = false;
                                            }),
                                            () =>
                                            {
                                                Log.Debug("Multi-key selection capture has completed.");

                                                stopTriggerSignal = null;
                                                CapturingMultiKeySelection = false;
                                            });
                                    }
                                    else
                                    {
                                        PublishSelection(ts.PointAndKeyValue.Value);

                                        PublishSelectionResult(new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                            new List<Point> { ts.PointAndKeyValue.Value.Point },
                                            ts.PointAndKeyValue.Value.KeyValue.Value.FunctionKey,
                                            ts.PointAndKeyValue.Value.KeyValue.Value.String,
                                            null));
                                    }
                                }
                                else if (SelectionMode == SelectionModes.Point)
                                {
                                    PublishSelection(ts.PointAndKeyValue.Value);

                                    PublishSelectionResult(new Tuple<List<Point>, FunctionKeys?, string, List<string>>(
                                        new List<Point> { ts.PointAndKeyValue.Value.Point }, null, null, null));
                                }
                            }
                            else
                            {
                                Log.Error("TriggerSignal.Signal==1, but TriggerSignal.PointAndKeyValue is null. "
                                        + "Discarding trigger request as point source is down, or producing stale points.");
                            }
                        }
                        else if (CapturingMultiKeySelection)
                        {
                            //We are capturing and may have received the stop capturing signal
                            if ((ts.Signal >= 1 && Settings.Default.SelectionTriggerStopSignal == TriggerStopSignals.NextHigh)
                                || (ts.Signal >= -1 && Settings.Default.SelectionTriggerStopSignal == TriggerStopSignals.NextLow))
                            {
                                //If we are using a fixation trigger source then the stop signal must occur on a letter
                                if (!(selectionTriggerSource is IFixationTriggerSource)
                                    || (ts.PointAndKeyValue != null && ts.PointAndKeyValue.Value.StringIsLetter))
                                {
                                    Log.Debug("Trigger signal to stop the current multi-key selection capture detected.");

                                    stopTriggerSignal = ts;
                                }
                            }
                        }
                    });
            }
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

            if (multiKeySelectionPointsSubscription != null)
            {
                multiKeySelectionPointsSubscription.Dispose();
            }
        }

        #endregion

        #endregion
        
        #region Discard Multi Key Selection

        private void DiscardMultiKeySelection()
        {
            Log.Debug("DiscardMultiKeySelection called.");

            if (multiKeySelectionPointsSubscription != null)
            {
                multiKeySelectionPointsSubscription.Dispose();
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
