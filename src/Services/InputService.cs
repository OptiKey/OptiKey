using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Observables.PointAndKeyValueSources;
using JuliusSweetland.ETTA.Observables.TriggerSignalSources;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Services
{
    public partial class InputService : BindableBase, IInputService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IKeyboardStateManager keyboardStateManager;
        private readonly IDictionaryService dictionaryService;
        private readonly IAudioService audioService;
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
            IKeyboardStateManager keyboardStateManager,
            IDictionaryService dictionaryService,
            IAudioService audioService,
            IPointAndKeyValueSource pointAndKeyValueSource,
            ITriggerSignalSource keySelectionTriggerSource,
            ITriggerSignalSource pointSelectionTriggerSource)
        {
            this.keyboardStateManager = keyboardStateManager;
            this.dictionaryService = dictionaryService;
            this.audioService = audioService;
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

                DisposeMultiKeySelection();
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
                Log.Debug(string.Format("Publishing Selection Result event with {0} point(s), FunctionKey:'{1}', String:'{2}', Best match '{3}', Suggestion count:{4}",
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
    }
}
