using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Services;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IInputService inputService;

        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SelectionModes selectionMode;
        private Point? currentPositionPoint;
        private KeyValue? currentPositionKey;
        private Tuple<Point, double> pointSelectionProgress;
        private readonly NotifyingConcurrentDictionary<double> keySelectionProgress = new NotifyingConcurrentDictionary<double>();
        private readonly NotifyingConcurrentDictionary<KeyDownStates> keyDownStates = new NotifyingConcurrentDictionary<KeyDownStates>();

        #endregion

        #region Ctor

        public MainViewModel(IInputService inputService)
        {
            this.inputService = inputService;

            SelectionMode = SelectionModes.Key;
            
            inputService.PointsPerSecond += (o, value) =>
            {
                //TODO: Display debugging info such as points per second and the position of the points in a selection
                //Log.Debug(string.Format("PointsPerSecond event... FPS:{0}", value));
            };
            inputService.CurrentPosition += (o, tuple) =>
            {
                CurrentPositionPoint = tuple.Item1;
                CurrentPositionKey = tuple.Item2;

            };
            inputService.SelectionProgress += (o, progress) =>
            {
                if (progress.Item2 == 0) 
                {
                    ResetSelectionProperties();
                }
                else if (progress.Item1 != null)
                {
                    if (SelectionMode == SelectionModes.Key
                        && progress.Item1.Value.KeyValue != null)
                    {
                        KeySelectionProgress[progress.Item1.Value.KeyValue.Value.Key] = new NotifyingProxy<double>(progress.Item2 * 100);
                    }
                    else if (SelectionMode == SelectionModes.Point)
                    {
                        PointSelectionProgress = new Tuple<Point, double>(progress.Item1.Value.Point, progress.Item2 * 100);
                    }
                }
            };
            inputService.Selection += (o, value) =>
            {
                if (SelectionMode == SelectionModes.Key)
                {
                    KeySelection = value.KeyValue;
                }
                else if (SelectionMode == SelectionModes.Point)
                {
                    PointSelection = value.Point;
                }
            };
            inputService.SelectionResult += (o, tuple) =>
            {
                //TODO: Handle selection result, i.e. the actual thing to use
                //Log.Debug(string.Format("SelectionResult event... Points(count):{0}, FunctionKey:{1}, Char:{2}, String:{3}, Matches(first/count):{4}/{5}", 
                //    tuple.Item1 != null ? tuple.Item1.Count : (int?)null, tuple.Item2, tuple.Item3, tuple.Item4,
                //    tuple.Item5 != null ? tuple.Item5.First() : null,
                //    tuple.Item5 != null ? tuple.Item5.Count : (int?)null));
            };
            inputService.Error += (o, exception) =>
            {
                //TODO: Handle errors
                //Log.Debug(string.Format("ERROR. Exception message:{0}", exception.Message));
            };
        }

        #endregion

        #region Properties

        public IInputService InputService { get { return inputService; } }

        public IKeyboard Keyboard { get { return null; } }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            set { inputService.PointToKeyValueMap = value; }
        }

        public SelectionModes SelectionMode
        {
            get { return selectionMode; }
            set
            {
                if (SetProperty(ref selectionMode, value))
                {
                    ResetSelectionProperties();
                    InputService.SelectionMode = value;
                }
            }
        }

        public Point? CurrentPositionPoint
        {
            get { return currentPositionPoint; }
            set { SetProperty(ref currentPositionPoint, value); }
        }

        public KeyValue? CurrentPositionKey
        {
            get { return currentPositionKey; }
            set { SetProperty(ref currentPositionKey, value); }
        }

        public Tuple<Point, double> PointSelectionProgress
        {
            get { return pointSelectionProgress; }
            private set
            {
                if (SetProperty(ref pointSelectionProgress, value))
                {
                    throw new NotImplementedException("Pushing point selection progress to the output service has not been implemented yet");
                }
            }
        }

        public NotifyingConcurrentDictionary<double> KeySelectionProgress
        {
            get { return keySelectionProgress; }
        }

        private Point? pointSelection;
        public Point? PointSelection
        {
            get { return pointSelection; }
            set { SetProperty(ref pointSelection, value); }
        }
        
        private KeyValue? keySelection;
        public KeyValue? KeySelection
        {
            get { return keySelection; }
            set { SetProperty(ref keySelection, value); }
        }

        public NotifyingConcurrentDictionary<KeyDownStates> KeyDownStates
        {
            get { return keyDownStates; }
        }

        #endregion

        #region Methods

        private void ResetSelectionProperties()
        {
            PointSelectionProgress = null;
            KeySelectionProgress.Clear();
            PointSelection = null;
            KeySelection = null;
        }

        #endregion
    }
}
