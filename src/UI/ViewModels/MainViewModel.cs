using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels
{
    public class MainViewModel : BindableBase, IKeyboardStateInfo
    {
        private readonly IInputService inputService;

        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SelectionModes selectionMode;
        private Point? currentPositionPoint;
        private KeyValue? currentPositionKey;
        private Tuple<Point, double> pointSelectionProgress;
        private readonly NotifyingConcurrentDictionary<double> keySelectionProgress;
        private readonly NotifyingConcurrentDictionary<KeyDownStates> keyDownStates;
        private readonly KeyEnabledStates keyEnabledStates;

        #endregion

        #region Ctor

        public MainViewModel(IInputService inputService)
        {
            //TESTING START
            Suggestions = new List<string>
            {
                "Suggestion1", "AnotherOne", "OneMore", "Why not another", "And a final one", "Wait, one more"
            };

            Observable.Interval(TimeSpan.FromSeconds(3))
                .SubscribeOnDispatcher()
                .Subscribe(i =>
                {
                    Settings.Default.PublishingKeys = !Settings.Default.PublishingKeys;
                });
            //TESTING END

            this.inputService = inputService;

            SelectionMode = SelectionModes.Key;

            keySelectionProgress = new NotifyingConcurrentDictionary<double>();
            keyDownStates = new NotifyingConcurrentDictionary<KeyDownStates>();
            keyEnabledStates = new KeyEnabledStates(this);

            inputService.KeyEnabledStates = keyEnabledStates;
            
            inputService.PointsPerSecond += (o, value) =>
            {
                //TODO: Display debugging points per second
            };

            inputService.CurrentPosition += (o, tuple) =>
            {
                //It's ok to publish CurrentPosition over an invalid key so don't bother checking for validity
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
                if (SelectionMode == SelectionModes.Key
                    && value.KeyValue != null)
                {
                    if (KeySelection != null)
                    {
                        KeySelection(this, value.KeyValue.Value);
                    }
                }
                else if (SelectionMode == SelectionModes.Point)
                {
                    //TODO: Handle point selection
                }
            };

            inputService.SelectionResult += (o, tuple) =>
            {
                if (tuple.Item2 != null || tuple.Item3 != null)
                {
                    var keyValue = new KeyValue {FunctionKey = tuple.Item2, String = tuple.Item3};
                    //TODO: Display debugging set of points

                    //TODO: Handle selection result, i.e. the actual thing to use

                    //TODO: Call NotifyStateChanged() at appropriate place to notify that key states have changed
                }
            };

            inputService.Error += (o, exception) =>
            {
                //TODO: Handle errors
                //Log.Debug(string.Format("ERROR. Exception message:{0}", exception.Message));
            };
        }

        #endregion

        #region Events

        public event EventHandler<KeyValue> KeySelection;

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
                    throw new NotImplementedException("Handling of PointSelection progress has not been implemented yet");
                }
            }
        }

        public NotifyingConcurrentDictionary<double> KeySelectionProgress
        {
            get { return keySelectionProgress; }
        }

        public NotifyingConcurrentDictionary<KeyDownStates> KeyDownStates
        {
            get { return keyDownStates; }
        }

        public KeyEnabledStates KeyEnabledStates
        {
            get { return keyEnabledStates; }
        }

        private List<string> suggestions;
        public List<string> Suggestions
        {
            get { return suggestions; }
            set { SetProperty(ref suggestions, value); }
        }

        private int suggestionsPage;
        public int SuggestionsPage
        {
            get { return suggestionsPage; }
            set { SetProperty(ref suggestionsPage, value); }
        }

        private int suggestionsPerPage;
        public int SuggestionsPerPage
        {
            get { return suggestionsPerPage; }
            set { SetProperty(ref suggestionsPerPage, value); }
        }

        private string output;
        public string Output
        {
            get { return output; }
            set { SetProperty(ref output, value); }
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
