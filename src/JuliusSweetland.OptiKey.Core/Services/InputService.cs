// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Observables.TriggerSources;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public partial class InputService : BindableBase, IInputService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IKeyStateService keyStateService;
        private readonly IDictionaryService dictionaryService;
        private readonly IAudioService audioService;
        private readonly ICapturingStateManager capturingStateManager;
        private readonly ITriggerSource eyeGestureTriggerSource;
        private readonly ITriggerSource keySelectionTriggerSource;
        private readonly ITriggerSource pointSelectionTriggerSource;
        private readonly object suspendRequestLock = new object();
        private IPointSource pointSource;
        private int suspendRequestCount;

        private event EventHandler<int> pointsPerSecondEvent;
        private event EventHandler<Tuple<Point, KeyValue>> currentPositionEvent;
        private event EventHandler<Tuple<TriggerTypes, PointAndKeyValue, double>> selectionProgressEvent;
        private event EventHandler<Tuple<TriggerTypes, PointAndKeyValue>> selectionEvent;
        private event EventHandler<Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>> selectionResultEvent;

        #endregion

        #region Ctor

        public InputService(
            IKeyStateService keyStateService,
            IDictionaryService dictionaryService,
            IAudioService audioService,
            ICapturingStateManager capturingStateManager,
            IPointSource pointSource,
            ITriggerSource eyeGestureTriggerSource,
            ITriggerSource keySelectionTriggerSource,
            ITriggerSource pointSelectionTriggerSource)
        {
            this.keyStateService = keyStateService;
            this.dictionaryService = dictionaryService;
            this.audioService = audioService;
            this.capturingStateManager = capturingStateManager;
            this.pointSource = pointSource;
            this.eyeGestureTriggerSource = eyeGestureTriggerSource;
            this.keySelectionTriggerSource = keySelectionTriggerSource;
            this.pointSelectionTriggerSource = pointSelectionTriggerSource;

            this.activeSelectionTriggerSubscriptions = new List<IDisposable>();
            this.activeSelectionProgressSubscriptions = new List<IDisposable>();

            //Fixation key triggers also need the enabled state info and override times
            if (keySelectionTriggerSource is IFixationTriggerSource fixationTrigger)
            {
                fixationTrigger.KeyEnabledStates = keyStateService.KeyEnabledStates;
                OverrideTimesByKey = fixationTrigger.OverrideTimesByKey;
            }
        }

        #endregion

        #region Properties

        public IDictionary<KeyValue, TimeSpanOverrides> OverrideTimesByKey { get; }

        public IPointSource PointSource
        {
            get { return pointSource; }
            set
            {
                pointSource = value;

                //Replace point source on key and point trigger sources as they publish the selection location from the point source
                eyeGestureTriggerSource.PointSource = pointSource;
                keySelectionTriggerSource.PointSource = pointSource;
                pointSelectionTriggerSource.PointSource = pointSource;
            }
        }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            set
            {
                Log.DebugFormat("PointToKeyValueMap property changed (setter called with {0} map).", value != null ? "non-null" : "null");

                if (pointSource != null)
                {
                    pointSource.PointToKeyValueMap = value;
                }

                DisposeMultiKeySelection();
            }
        }

        private SelectionModes selectionMode;
        public SelectionModes SelectionMode
        {
            get => selectionMode;
            set
            {
                if (SetProperty(ref selectionMode, value))
                {
                    Log.DebugFormat("SelectionMode changed to {0}", value);

                    Log.Debug("Disposing of selection progress subscriptions");
                    foreach (IDisposable sub in activeSelectionProgressSubscriptions)
                        sub.Dispose(); 
                    activeSelectionProgressSubscriptions.RemoveAll(s => true);

                    Log.Debug("Disposing of selection trigger subscriptions");
                    foreach (IDisposable sub in activeSelectionTriggerSubscriptions)
                        sub.Dispose();
                    activeSelectionTriggerSubscriptions.RemoveAll(s => true);                    

                    if (selectionProgressEvent != null)
                    {
                        Log.Debug("SelectionProgress event has at least one listener - subscribing to relevant selection progress source");
                        CreateSelectionProgressSubscription(value);
                    }

                    if (selectionEvent != null 
                        || selectionResultEvent != null)
                    {
                        Log.Debug("Selection event has at least one listener - subscribing to relevant selection trigger source");
                        CreateSelectionSubscriptions(value);
                    }
                }
            }
        }

        public bool MultiKeySelectionSupported { get; set; }

        private bool capturingMultiKeySelection;
        public bool CapturingMultiKeySelection
        {
            get => capturingMultiKeySelection;
            private set
            {
                SetProperty(ref capturingMultiKeySelection, value);
                capturingStateManager.CapturingMultiKeySelection = value;
            }
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
                        pointsPerSecondSubscription = null;
                    }
                }
            }
        }

        #endregion

        #region Current Position

        public event EventHandler<Tuple<Point, KeyValue>> CurrentPosition
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

                if (currentPositionEvent == null)
                {
                    Log.Info("Last listener of CurrentPosition event has unsubscribed. Disposing of currentPositionSubscription.");

                    if (currentPositionSubscription != null)
                    {
                        currentPositionSubscription.Dispose();
                        currentPositionSubscription = null;
                    }
                }
            }
        }

        #endregion

        #region Selection Progress

        public event EventHandler<Tuple<TriggerTypes, PointAndKeyValue, double>> SelectionProgress
        {
            add
            {
                if (selectionProgressEvent == null)
                {
                    Log.Info("SelectionProgress event has first subscriber.");
                }

                selectionProgressEvent += value;
                
                if (activeSelectionProgressSubscriptions.Count == 0)
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
                    
                    if (activeSelectionProgressSubscriptions.Count > 0)
                    {
                        foreach (var sub in activeSelectionProgressSubscriptions)
                        {
                            sub.Dispose();                            
                        }
                        activeSelectionProgressSubscriptions.RemoveAll(s => true);
                    }
                }
            }
        }

        #endregion

        #region Selection

        public event EventHandler<Tuple<TriggerTypes, PointAndKeyValue>> Selection
        {
            add
            {
                if (selectionEvent == null)
                {
                    Log.Info("Selection event has first subscriber.");
                }

                selectionEvent += value;

                if (activeSelectionTriggerSubscriptions.Count == 0)
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

        public event EventHandler<Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>> SelectionResult
        {
            add
            {
                if (selectionResultEvent == null)
                {
                    Log.Info("Selection Result event has first subscriber.");
                }

                selectionResultEvent += value;

                if (activeSelectionTriggerSubscriptions.Count == 0)
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
                Log.DebugFormat("Publishing PointsPerSecond event: {0}", pointsPerSecond);

                pointsPerSecondEvent(this, pointsPerSecond);
            }
        }

        #endregion

        #region Publish Current Position

        private void PublishCurrentPosition(Tuple<Point, KeyValue> currentPosition)
        {
            if (currentPositionEvent != null)
            {
                Log.DebugFormat("Publishing CurrentPosition event with Point:{0} KeyValue:{1}", currentPosition.Item1, currentPosition.Item2);

                currentPositionEvent(this, currentPosition);
            }
        }

        #endregion

        #region Publish Selection Progress

        private void PublishSelectionProgress(Tuple<TriggerTypes, PointAndKeyValue, double> selectionProgress)
        {
            if (selectionProgressEvent != null)
            {
                if ((selectionProgress.Item3 < 0.1) || (selectionProgress.Item3 - 0.5) < 0.1 || (selectionProgress.Item3 - 1) < 0.1)
                {
                    Log.DebugFormat("Publishing SelectionProgress event: {0} : {1} : {2}",
                        selectionProgress.Item1, selectionProgress.Item2, selectionProgress.Item3);
                }

                selectionProgressEvent(this, selectionProgress);
            }
        }

        #endregion

        #region Publish Selection

        private void PublishSelection(TriggerTypes mode, PointAndKeyValue selection)
        {
            if (selectionEvent != null)
            {
                Log.DebugFormat("Publishing Selection event with PointAndKeyValue:{0}", selection);                
                selectionEvent(this, new Tuple<TriggerTypes, PointAndKeyValue>(mode, selection));
            }
        }

        #endregion

        #region Publish Selection Result

        private void PublishSelectionResult(Tuple<TriggerTypes, List<Point>, KeyValue, List<string>> selectionResult)
        {
            if (selectionResultEvent != null)
            {
                Log.DebugFormat("Publishing Selection Result event with {1} point(s), Mode: '{0}', FunctionKey:'{2}', String:'{3}', Best match '{4}', Suggestion count:{5}",
                        selectionResult.Item2?.Count,
                        selectionResult.Item3 != null ? selectionResult.Item3.FunctionKey : null,  
                        selectionResult.Item3 != null ? selectionResult.Item3.String.ToPrintableString() : "",
                        selectionResult.Item4 != null && selectionResult.Item4.Any() ? selectionResult.Item4.First() : null,
                        selectionResult.Item4?.Count);

                selectionResultEvent(this, selectionResult);
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            Error?.Invoke(sender, ex);
        }

        #endregion

        #endregion

        #region Methods

        public void RequestSuspend()
        {
            Log.InfoFormat("RequestSuspend received. SuspendRequestCount={0} before it is incremented.", suspendRequestCount);
            lock (suspendRequestLock)
            {
                suspendRequestCount++;
                if (eyeGestureTriggerSource.State == RunningStates.Running)
                {
                    eyeGestureTriggerSource.State = RunningStates.Paused;
                }
                if (keySelectionTriggerSource.State == RunningStates.Running)
                {
                    keySelectionTriggerSource.State = RunningStates.Paused;
                }
                if (pointSelectionTriggerSource.State == RunningStates.Running)
                {
                    pointSelectionTriggerSource.State = RunningStates.Paused;
                }
                if (pointSource.State == RunningStates.Running)
                {
                    pointSource.State = RunningStates.Paused;
                }
            }
        }

        public void RequestResume()
        {
            Log.InfoFormat("RequestResume received. SuspendRequestCount={0} before it is decremented.", suspendRequestCount);
            lock (suspendRequestLock)
            {
                suspendRequestCount--;
                if (suspendRequestCount == 0)
                {
                    if (eyeGestureTriggerSource != null)
                    {
                        eyeGestureTriggerSource.State = RunningStates.Running;
                    }
                    if (keySelectionTriggerSource != null)
                    {
                        keySelectionTriggerSource.State = RunningStates.Running;
                    }
                    if (pointSelectionTriggerSource != null)
                    {
                        pointSelectionTriggerSource.State = RunningStates.Running;
                    }
                    if (pointSource != null)
                    {
                        pointSource.State = RunningStates.Running;
                    }
                }
            }

            if (suspendRequestCount < 0)
            {
                Log.WarnFormat("InputService suspend request counter is below zero. Current value:{0}", suspendRequestCount);
            }
        }

        #endregion
    }
}
