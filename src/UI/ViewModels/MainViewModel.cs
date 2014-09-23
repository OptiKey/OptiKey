using System;
using System.Collections.Generic;
using System.Linq;
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
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IInputService inputService;
        private readonly NotifyingConcurrentDictionary<double> keySelectionProgress;
        private readonly NotifyingConcurrentDictionary<KeyDownStates> keyDownStates;
        private readonly KeyEnabledStates keyEnabledStates;

        private SelectionModes selectionMode;
        private Point? currentPositionPoint;
        private KeyValue? currentPositionKey;
        private Tuple<Point, double> pointSelectionProgress;
        
        #endregion

        #region Ctor

        public MainViewModel(IInputService inputSvc)
        {
            //TESTING START
            Suggestions = new List<string>
            {
                "Suggestion1", "AnotherOne", "OneMore", "Why not another", "And a final one", "Wait, one more"
            };

            //Observable.Interval(TimeSpan.FromSeconds(3))
            //    .SubscribeOnDispatcher()
            //    .Subscribe(i =>
            //    {
            //        Settings.Default.PublishingKeys = !Settings.Default.PublishingKeys;
            //    });
            //TESTING END

            //Init readonly fields
            inputService = inputSvc;
            keySelectionProgress = new NotifyingConcurrentDictionary<double>();
            keyDownStates = new NotifyingConcurrentDictionary<KeyDownStates>();
            keyEnabledStates = new KeyEnabledStates(this);
            
            //Init state properties
            SelectionMode = SelectionModes.Key;
            Keyboard = new Alpha();
            //KeyDownStates[new KeyValue { FunctionKey = FunctionKeys.Shift }.Key].Value = Enums.KeyDownStates.On;
            
            //Apply settings and subscribe to setting changes
            KeyDownStates[new KeyValue { FunctionKey = FunctionKeys.TogglePublish }.Key].Value =
                Settings.Default.PublishingKeys ? Enums.KeyDownStates.Lock : Enums.KeyDownStates.Off;

            Settings.Default.OnPropertyChanges(s => s.PublishingKeys)
                .Subscribe(pk => 
                    KeyDownStates[new KeyValue { FunctionKey = FunctionKeys.TogglePublish }.Key].Value =
                        pk ? Enums.KeyDownStates.Lock : Enums.KeyDownStates.Off);

            KeyDownStates[new KeyValue { FunctionKey = FunctionKeys.ToggleMultiKeySelectionSupported }.Key].Value =
                Settings.Default.MultiKeySelectionSupported ? Enums.KeyDownStates.Lock : Enums.KeyDownStates.Off;

            Settings.Default.OnPropertyChanges(s => s.MultiKeySelectionSupported)
                .Subscribe(mkss =>
                    KeyDownStates[new KeyValue { FunctionKey = FunctionKeys.ToggleMultiKeySelectionSupported }.Key].Value =
                        mkss ? Enums.KeyDownStates.Lock : Enums.KeyDownStates.Off);
            
            //Init input service properties
            inputService.KeyEnabledStates = keyEnabledStates;

            inputService.OnPropertyChanges(i => i.CapturingMultiKeySelection)
                .Subscribe(cmks => CapturingMultiKeySelection = cmks);

            inputService.PointsPerSecond += (o, value) =>
            {
                //TODO: Display debugging points per second
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
                        KeySelectionProgress[progress.Item1.Value.KeyValue.Value.Key] = new NotifyingProxy<double>(progress.Item2);
                    }
                    else if (SelectionMode == SelectionModes.Point)
                    {
                        PointSelectionProgress = new Tuple<Point, double>(progress.Item1.Value.Point, progress.Item2);
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
                var points = tuple.Item1;
                var singleKeyValue = tuple.Item2 != null || tuple.Item3 != null
                    ? new KeyValue {FunctionKey = tuple.Item2, String = tuple.Item3}
                    : (KeyValue?)null;
                var multiKeySelection = tuple.Item4;

                //TODO: Display debugging set of points

                if (SelectionMode == SelectionModes.Key
                    && (singleKeyValue != null || (multiKeySelection != null && multiKeySelection.Any())))
                {
                    KeySelectionResult(singleKeyValue, multiKeySelection);
                }
                else if (SelectionMode == SelectionModes.Point)
                {
                    //TODO: Handle point selection result
                }
            };

            inputService.Error += (o, exception) =>
            {
                //TODO: Handle errors
                //Log.Debug(string.Format("ERROR. Exception message:{0}", exception.Message));
            };
        }

        private void KeySelectionResult(KeyValue? singleKeyValue, List<string> multiKeySelection)
        {
            if (singleKeyValue != null
                && singleKeyValue.Value.FunctionKey != null)
            {
                switch (singleKeyValue.Value.FunctionKey)
                {
                    case FunctionKeys.AlphaKeyboard:
                        Keyboard = new Alpha();
                        break;

                    case FunctionKeys.NumericAndSymbols1Keyboard:
                        Keyboard = new NumericAndSymbols1();
                        break;

                    case FunctionKeys.Symbols2Keyboard:
                        Keyboard = new Symbols2();
                        break;

                    case FunctionKeys.PublishKeyboard:
                        Keyboard = new Publish();
                        break;

                    case FunctionKeys.TogglePublish:
                        Settings.Default.PublishingKeys = !Settings.Default.PublishingKeys;
                        break;

                    case FunctionKeys.ToggleMultiKeySelectionSupported:
                        Settings.Default.MultiKeySelectionSupported = !Settings.Default.MultiKeySelectionSupported;
                        break;

                    case FunctionKeys.Shift:
                        var shiftKey = new KeyValue { FunctionKey = FunctionKeys.Shift }.Key;
                        KeyDownStates[shiftKey].Value =
                            KeyDownStates[shiftKey].Value == Enums.KeyDownStates.Off
                                ? KeyDownStates[shiftKey].Value = Enums.KeyDownStates.On
                                : KeyDownStates[shiftKey].Value == Enums.KeyDownStates.On
                                    ? KeyDownStates[shiftKey].Value = Enums.KeyDownStates.Lock
                                    : KeyDownStates[shiftKey].Value = Enums.KeyDownStates.Off;
                        break;
                }
            }

            //TODO: Call NotifyStateChanged() at appropriate place to notify that key states have changed

        }

        #endregion

        #region Events

        public event EventHandler<KeyValue> KeySelection;

        #endregion

        #region Properties

        public IInputService InputService { get { return inputService; } }

        private IKeyboard keyboard;
        public IKeyboard Keyboard
        {
            get { return keyboard; }
            set { SetProperty(ref keyboard, value); }
        }

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

        private bool capturingMultiKeySelection;
        public bool CapturingMultiKeySelection
        {
            get { return capturingMultiKeySelection; }
            set { SetProperty(ref capturingMultiKeySelection, value); }
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
