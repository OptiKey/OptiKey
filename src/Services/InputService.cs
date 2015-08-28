using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Observables.TriggerSources;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public partial class InputService : BindableBase, IInputService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IKeyStateService keyStateService;
        private readonly IDictionaryService dictionaryService;
        private readonly IAudioService audioService;
        private readonly ICapturingStateManager capturingStateManager;
        private readonly IPointSource pointSource;
        private readonly ITriggerSource keySelectionTriggerSource;
        private readonly ITriggerSource pointSelectionTriggerSource;
        private readonly object suspendRequestLock = new object();

        private int suspendRequestCount;
        
        private event EventHandler<int> pointsPerSecondEvent;
        private event EventHandler<Tuple<Point, KeyValue?>> currentPositionEvent;
        private event EventHandler<Tuple<PointAndKeyValue?, double>> selectionProgressEvent;
        private event EventHandler<PointAndKeyValue> selectionEvent;
        private event EventHandler<Tuple<List<Point>, FunctionKeys?, string, List<string>>> selectionResultEvent;

        #endregion

        #region Ctor

        public InputService(
            IKeyStateService keyStateService,
            IDictionaryService dictionaryService,
            IAudioService audioService,
            ICapturingStateManager capturingStateManager,
            IPointSource pointSource,
            ITriggerSource keySelectionTriggerSource,
            ITriggerSource pointSelectionTriggerSource)
        {
            this.keyStateService = keyStateService;
            this.dictionaryService = dictionaryService;
            this.audioService = audioService;
            this.capturingStateManager = capturingStateManager;
            this.pointSource = pointSource;
            this.keySelectionTriggerSource = keySelectionTriggerSource;
            this.pointSelectionTriggerSource = pointSelectionTriggerSource;

            //Fixation key triggers also need the enabled state info
            var fixationTrigger = keySelectionTriggerSource as IFixationTriggerSource;
            if (fixationTrigger != null)
            {
                fixationTrigger.KeyEnabledStates = keyStateService.KeyEnabledStates;
            }
        }

        #endregion

        #region Properties
        
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
            get { return selectionMode; }
            set
            {
                if (SetProperty(ref selectionMode, value))
                {
                    Log.DebugFormat("SelectionMode changed to {0}", value);

                    if (selectionProgressSubscription != null)
                    {
                        Log.Debug("Disposing of selection progress subscription");
                        selectionProgressSubscription.Dispose();
                    }

                    if (selectionTriggerSubscription != null)
                    {
                        Log.Debug("Disposing of selection trigger subscription");
                        selectionTriggerSubscription.Dispose();
                    }

                    if (multiKeySelectionSubscription != null)
                    {
                        Log.Debug("Disposing of multi-key selection points subscription");
                        multiKeySelectionSubscription.Dispose();
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
            set
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
                    }
                }
            }
        }

        #endregion

        #region Current Position

        public event EventHandler<Tuple<Point, KeyValue?>> CurrentPosition
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
                Log.DebugFormat("Publishing PointsPerSecond event: {0}", pointsPerSecond);

                pointsPerSecondEvent(this, pointsPerSecond);
            }
        }

        #endregion

        #region Publish Current Position

        private void PublishCurrentPosition(Tuple<Point, KeyValue?> currentPosition)
        {
            if (currentPositionEvent != null)
            {
                Log.DebugFormat("Publishing CurrentPosition event with Point:{0} KeyValue:{1}", currentPosition.Item1, currentPosition.Item2);

                currentPositionEvent(this, currentPosition);
            }
        }

        #endregion

        #region Publish Selection Progress

        private void PublishSelectionProgress(Tuple<PointAndKeyValue?, double> selectionProgress)
        {
            if (selectionProgressEvent != null)
            {
                Log.DebugFormat("Publishing SelectionProgress event: {0}=>{1}", selectionProgress.Item1, selectionProgress.Item2);

                selectionProgressEvent(this, selectionProgress);
            }
        }

        #endregion

        #region Publish Selection

        private void PublishSelection(PointAndKeyValue selection)
        {
            if (selectionEvent != null)
            {
                Log.DebugFormat("Publishing Selection event with PointAndKeyValue:{0}", selection);

                selectionEvent(this, selection);
            }
        }

        #endregion

        #region Publish Selection Result

        private void PublishSelectionResult(Tuple<List<Point>, FunctionKeys?, string, List<string>> selectionResult)
        {
            if (selectionResultEvent != null)
            {
                Log.DebugFormat("Publishing Selection Result event with {0} point(s), FunctionKey:'{1}', String:'{2}', Best match '{3}', Suggestion count:{4}",
                        selectionResult.Item1 != null ? selectionResult.Item1.Count : (int?)null,
                        selectionResult.Item2, 
                        selectionResult.Item3.ConvertEscapedCharsToLiterals(),
                        selectionResult.Item4 != null && selectionResult.Item4.Any() ? selectionResult.Item4.First() : null,
                        selectionResult.Item4 != null ? selectionResult.Item4.Count : (int?)null);

                selectionResultEvent(this, selectionResult);
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion

        #endregion

        #region Methods

        public void RequestSuspend()
        {
            lock (suspendRequestLock)
            {
                suspendRequestCount++;
                if (pointSource.State == RunningStates.Running)
                {
                    pointSource.State = RunningStates.Paused;
                }
            }
        }

        public void RequestResume()
        {
            lock (suspendRequestLock)
            {
                suspendRequestCount--;
                if (suspendRequestCount == 0)
                {
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
