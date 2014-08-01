using System;
using System.Collections.Generic;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Models.DrWPF.Windows.Data;
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
        private Point currentPositionPoint;
        private KeyValue currentPositionKey;
        private Tuple<Point, double> pointSelectionProgress;
        private readonly ObservableDictionary<KeyValue, double> keySelectionProgress;
        private readonly ObservableDictionary<KeyValue, KeyDownStates> keyDownStates;

        #endregion

        #region Ctor

        public MainViewModel(IInputService inputService)
        {
            this.inputService = inputService;

            SelectionMode = SelectionModes.Key;

            keyDownStates = new ObservableDictionary<KeyValue, KeyDownStates>
            {
                ReturnDefaultValueIfKeyNotFound = true
            };

            keySelectionProgress = new ObservableDictionary<KeyValue, double>
            {
                ReturnDefaultValueIfKeyNotFound = true
            };

            //TESTING...
            currentPositionKey = new KeyValue {String = "U"}; 
            keyDownStates.Add(new KeyValue { String = "W" }, Enums.KeyDownStates.On);
            keyDownStates.Add(new KeyValue { String = "Y" }, Enums.KeyDownStates.Lock);
            //keyDownStates.Add(new KeyValue { FunctionKey = FunctionKeys.Shift}, Enums.KeyDownStates.On);
            keyDownStates.Add(new KeyValue { FunctionKey = FunctionKeys.Ctrl }, Enums.KeyDownStates.Lock);
            keySelection = new KeyValue {String = "P"};

            inputService.PointsPerSecond += (o, value) =>
            {
                //Log.Debug(string.Format("PointsPerSecond event... FPS:{0}", value));
            };
            inputService.CurrentPosition += (o, tuple) =>
            {
                //Log.Debug(string.Format("CurrentPosition event... Point:{0}, KeyValue:{1}", tuple.Item1, tuple.Item2));
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
                        KeySelectionProgress[progress.Item1.Value.KeyValue.Value] = progress.Item2;
                    }
                    else if (SelectionMode == SelectionModes.Point)
                    {
                        PointSelectionProgress = new Tuple<Point, double>(progress.Item1.Value.Point, progress.Item2);
                    }
                }
            };
            inputService.Selection += (o, value) =>
            {
                //Log.Debug(string.Format("Selection event... PointAndKeyValue:{0}", value));
            };
            inputService.SelectionResult += (o, tuple) =>
            {
                //Log.Debug(string.Format("SelectionResult event... Points(count):{0}, FunctionKey:{1}, Char:{2}, String:{3}, Matches(first/count):{4}/{5}", 
                //    tuple.Item1 != null ? tuple.Item1.Count : (int?)null, tuple.Item2, tuple.Item3, tuple.Item4,
                //    tuple.Item5 != null ? tuple.Item5.First() : null,
                //    tuple.Item5 != null ? tuple.Item5.Count : (int?)null));
            };
            inputService.Error += (o, exception) =>
            {
                //Log.Debug(string.Format("ERROR. Exception message:{0}", exception.Message));
            };

            var pointToKeyValueMap = new Dictionary<Rect, KeyValue>
                {
                    {new Rect(0, 0, 100, 100), new KeyValue {String = "Å"}},
                    {new Rect(101, 0, 100, 100), new KeyValue {String = "N"}},
                    {new Rect(201, 0, 100, 100), new KeyValue {String = "G"}},
                    {new Rect(301, 0, 100, 100), new KeyValue {String = "S"}},
                    {new Rect(401, 0, 100, 100), new KeyValue {String = "R"}},
                    {new Rect(501, 0, 100, 100), new KeyValue {String = "ö"}},
                    {new Rect(601, 0, 100, 100), new KeyValue {String = "M"}}
                };
            inputService.PointToKeyValueMap = pointToKeyValueMap;
        }

        #endregion

        #region Properties

        public IInputService InputService { get { return inputService; } }

        public IKeyboard Keyboard { get { return null; } }

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

        public Point CurrentPositionPoint
        {
            get { return currentPositionPoint; }
            set { SetProperty(ref currentPositionPoint, value); }
        }

        public KeyValue CurrentPositionKey
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

        public ObservableDictionary<KeyValue, double> KeySelectionProgress
        {
            get { return keySelectionProgress; }
        }

        private Point pointSelection;
        public Point PointSelection
        {
            get { return pointSelection; }
            set { SetProperty(ref pointSelection, value); }
        }
        
        private KeyValue keySelection;
        public KeyValue KeySelection
        {
            get { return keySelection; }
            set { SetProperty(ref keySelection, value); }
        }

        public ObservableDictionary<KeyValue, KeyDownStates> KeyDownStates
        {
            get { return keyDownStates; }
        }

        #endregion

        #region Methods

        private void ResetSelectionProperties()
        {
            PointSelectionProgress = null;
            KeySelectionProgress.Clear();
        }

        #endregion
    }
}
