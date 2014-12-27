using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Observables.PointAndKeyValueSources;
using JuliusSweetland.ETTA.Observables.TriggerSignalSources;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;
using log4net;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Alpha = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.Alpha;
using AlternativeAlpha1 = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.AlternativeAlpha1;
using AlternativeAlpha2 = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.AlternativeAlpha2;
using AlternativeAlpha3 = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.AlternativeAlpha3;
using Currencies1 = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.Currencies1;
using Currencies2 = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.Currencies2;
using Menu = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.Menu;
using NumericAndSymbols1 = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.NumericAndSymbols1;
using NumericAndSymbols2 = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.NumericAndSymbols2;
using PhysicalKeys = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.PhysicalKeys;
using SettingCategories = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.SettingCategories;
using YesNoQuestion = JuliusSweetland.ETTA.UI.ViewModels.Keyboards.YesNoQuestion;

namespace JuliusSweetland.ETTA.UI.ViewModels
{
    public class MainViewModel : BindableBase, ICapturingStateManager, ICalibrateStateManager
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly InteractionRequest<Notification> notificationRequest; 
        private readonly InteractionRequest<Notification> errorNotificationRequest;
        private readonly InteractionRequest<CalibrationResult> calibrateRequest;
        
        private SelectionModes selectionMode;
        private Point? currentPositionPoint;
        private KeyValue? currentPositionKey;
        private Tuple<Point, double> pointSelectionProgress;
        private Dictionary<Rect, KeyValue> pointToKeyValueMap;

        private IAudioService audioService;
        private ICalibrationService calibrationService;
        private IDictionaryService dictionaryService;
        private IPublishService publishService;
        private IKeyboardService keyboardService;
        private ISuggestionService suggestionService;
        private IInputService inputService;
        private IOutputService outputService;
        
        #endregion

        #region Ctor

        public MainViewModel()
        {
            Log.Debug("Ctor called.");

            notificationRequest = new InteractionRequest<Notification>();
            errorNotificationRequest = new InteractionRequest<Notification>();
            calibrateRequest = new InteractionRequest<CalibrationResult>();

            SelectionMode = SelectionModes.Key;
            Keyboard = new Alpha();
        }

        #endregion

        #region Events

        public event EventHandler<KeyValue> KeySelection;

        #endregion

        #region Properties

        public IInputService InputService
        {
            get { return inputService; }
            set { SetProperty(ref inputService, value); }
        }

        public IOutputService OutputService
        {
            get { return outputService; }
            set { SetProperty(ref outputService, value); }
        }

        public IKeyboardService KeyboardService
        {
            get { return keyboardService; }
            set { SetProperty(ref keyboardService, value); }
        }

        public ISuggestionService SuggestionService
        {
            get { return suggestionService; }
            set { SetProperty(ref suggestionService, value); }
        }

        public ICalibrationService CalibrationService
        {
            get { return calibrationService; }
            set { SetProperty(ref calibrationService, value); }
        }

        private IKeyboard keyboard;
        public IKeyboard Keyboard
        {
            get { return keyboard; }
            set { SetProperty(ref keyboard, value); }
        }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            set
            {
                if (pointToKeyValueMap != value)
                {
                    pointToKeyValueMap = value;

                    if (inputService != null) //This can be called before Initialise()
                    {
                        inputService.PointToKeyValueMap = value;
                    }

                    //The last selection result points cannot be valid if this has changed (window has moved or resized)
                    SelectionResultPoints = null;
                }
            }
        }

        public SelectionModes SelectionMode
        {
            get { return selectionMode; }
            set
            {
                if (SetProperty(ref selectionMode, value))
                {
                    Log.Debug(string.Format("SelectionMode changed to {0}", value));

                    ResetSelectionProgress();

                    if (inputService != null)
                    {
                        inputService.SelectionMode = value;
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
                if (SetProperty(ref capturingMultiKeySelection, value))
                {
                    Log.Debug(string.Format("CapturingMultiKeySelection changed to {0}", value));

                    audioService.PlaySound(value
                        ? Settings.Default.MultiKeySelectionCaptureStartSoundFile
                        : Settings.Default.MultiKeySelectionCaptureEndSoundFile);
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
                    //if (value != null)
                    //{
                    //    Debug.Print("Point:{0}, Progrss:{1}", value.Item1, value.Item2);
                    //}
                    
                    throw new NotImplementedException("Handling of PointSelection progress has not been implemented yet");
                }
            }
        }

        private List<Point> selectionResultPoints;
        public List<Point> SelectionResultPoints
        {
            get { return selectionResultPoints; }
            set { SetProperty(ref selectionResultPoints, value); }
        }

        private int pointsPerSecond;
        public int PointsPerSecond
        {
            get { return pointsPerSecond; }
            set { SetProperty(ref pointsPerSecond, value); }
        }

        private bool scratchpadIsDisabled;
        public bool ScratchpadIsDisabled
        {
            get { return scratchpadIsDisabled; }
            set { SetProperty(ref scratchpadIsDisabled, value); }
        }

        public InteractionRequest<Notification> NotificationRequest { get { return notificationRequest; } }
        public InteractionRequest<Notification> ErrorNotificationRequest { get { return errorNotificationRequest; } }
        public InteractionRequest<CalibrationResult> CalibrateRequest { get { return calibrateRequest; } }

        #endregion

        #region Methods

        public void Initialise()
        {
            Log.Debug("Initialise called.");

            audioService = new AudioService();
            dictionaryService = new DictionaryService();
            publishService = new PublishService();
            SuggestionService = new SuggestionService();
            keyboardService = new KeyboardService(suggestionService, this, this);
            InputService = CreateInputService();
            OutputService = new OutputService(keyboardService, suggestionService, publishService, dictionaryService);

            audioService.Error += HandleServiceError;
            dictionaryService.Error += HandleServiceError;
            publishService.Error += HandleServiceError;
            inputService.Error += HandleServiceError;

            inputService.KeyEnabledStates = keyboardService.KeyEnabledStates;

            inputService.OnPropertyChanges(i => i.CapturingMultiKeySelection)
                        .Subscribe(value => CapturingMultiKeySelection = value);

            inputService.PointsPerSecond += (o, value) => { PointsPerSecond = value; };

            inputService.CurrentPosition += (o, tuple) =>
            {
                CurrentPositionPoint = tuple.Item1;
                CurrentPositionKey = tuple.Item2;
            };

            inputService.SelectionProgress += (o, progress) =>
            {
                if (progress.Item1 == null
                    && progress.Item2 == 0)
                {
                    ResetSelectionProgress(); //Reset all keys
                }
                else if (progress.Item1 != null)
                {
                    if (SelectionMode == SelectionModes.Key
                        && progress.Item1.Value.KeyValue != null)
                    {
                        keyboardService.KeySelectionProgress[progress.Item1.Value.KeyValue.Value] =
                            new NotifyingProxy<double>(progress.Item2);
                    }
                    else if (SelectionMode == SelectionModes.Point)
                    {
                        PointSelectionProgress = new Tuple<Point, double>(progress.Item1.Value.Point, progress.Item2);
                    }
                }
            };

            inputService.Selection += (o, value) =>
            {
                Log.Debug("Selection event received from InputService.");

                SelectionResultPoints = null; //Clear captured points from previous SelectionResult event

                if (!CapturingMultiKeySelection)
                {
                    audioService.PlaySound(Settings.Default.SelectionSoundFile);
                }

                if (SelectionMode == SelectionModes.Key
                    && value.KeyValue != null)
                {
                    if (KeySelection != null)
                    {
                        Log.Debug(string.Format("Firing KeySelection event with KeyValue '{0}'", value.KeyValue.Value));
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
                Log.Debug("SelectionResult event received from InputService.");

                var points = tuple.Item1;
                var singleKeyValue = tuple.Item2 != null || tuple.Item3 != null
                    ? new KeyValue { FunctionKey = tuple.Item2, String = tuple.Item3 }
                    : (KeyValue?)null;
                var multiKeySelection = tuple.Item4;

                SelectionResultPoints = points; //Store captured points from SelectionResult event (displayed for debugging)

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

            inputService.PointToKeyValueMap = pointToKeyValueMap;
            inputService.SelectionMode = SelectionMode;

            AttachScratchpadEnabledListener();

            HandleFunctionKeySelectionResult(KeyValues.LeftShiftKey); //Set initial shift state to on

            ReleaseKeysOnApplicationExit();
        }

        private IInputService CreateInputService()
        {
            Log.Debug("Creating InputService.");

            //Instantiate point source
            IPointAndKeyValueSource pointSource;
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.GazeTracker:
                    pointSource = new GazeTrackerSource(
                        Settings.Default.PointTtl,
                        Settings.Default.GazeTrackerUdpPort,
                        new Regex(Settings.Default.GazeTrackerUdpRegex));
                    break;

                case PointsSources.TheEyeTribe:
                    var eyeTribePointsService = new TheEyeTribePointService();
                    eyeTribePointsService.Error += HandleServiceError;
                    pointSource = new TheEyeTribeSource(
                        Settings.Default.PointTtl, 
                        eyeTribePointsService);
                    break;

                case PointsSources.MousePosition:
                    pointSource = new MousePositionSource(
                        Settings.Default.PointTtl);
                    break;

                default:
                    throw new ArgumentException(
                        "'PointsSource' settings is missing or not recognised! Please correct and restart ETTA.");
            }

            //Instantiate calibrate service
            switch (Settings.Default.PointsSource)
            {
                case PointsSources.TheEyeTribe:
                    CalibrationService = new TheEyeTribeCalibrationService();
                    break;
            }

            //Instantiate key trigger source
            ITriggerSignalSource keySelectionTriggerSource;
            switch (Settings.Default.KeySelectionTriggerSource)
            {
                case TriggerSources.Fixations:
                    keySelectionTriggerSource = new KeyFixationSource(
                       Settings.Default.KeySelectionTriggerFixationStartTime,
                       Settings.Default.KeySelectionTriggerFixationCompleteTime,
                       Settings.Default.KeySelectionTriggerIncompleteFixationTtl,
                       pointSource.Sequence);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    keySelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.SelectionTriggerKeyboardKeyDownUpKey,
                        pointSource.Sequence);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    keySelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.SelectionTriggerMouseDownUpButton,
                        pointSource.Sequence);
                    break;

                default:
                    throw new ArgumentException(
                        "'KeySelectionTriggerSource' setting is missing or not recognised! Please correct and restart ETTA.");
            }

            //Instantiate point trigger source
            ITriggerSignalSource pointSelectionTriggerSource;
            switch (Settings.Default.PointSelectionTriggerSource)
            {
                case TriggerSources.Fixations:
                    pointSelectionTriggerSource = new PointFixationSource(
                        Settings.Default.PointSelectionTriggerFixationStartTime,
                        Settings.Default.PointSelectionTriggerFixationCompleteTime,
                        Settings.Default.PointSelectionTriggerFixationRadius,
                        pointSource.Sequence);
                    break;

                case TriggerSources.KeyboardKeyDownsUps:
                    pointSelectionTriggerSource = new KeyboardKeyDownUpSource(
                        Settings.Default.SelectionTriggerKeyboardKeyDownUpKey,
                        pointSource.Sequence);
                    break;

                case TriggerSources.MouseButtonDownUps:
                    pointSelectionTriggerSource = new MouseButtonDownUpSource(
                        Settings.Default.SelectionTriggerMouseDownUpButton,
                        pointSource.Sequence);
                    break;

                default:
                    throw new ArgumentException(
                        "'PointSelectionTriggerSource' setting is missing or not recognised! "
                        + "Please correct and restart ETTA.");
            }

            return new InputService(keyboardService, dictionaryService, audioService,
                pointSource, keySelectionTriggerSource, pointSelectionTriggerSource);
        }

        private void AttachScratchpadEnabledListener()
        {
            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                keyboardService.KeyDownStates[kv].OnPropertyChanges(s => s.Value)
                    .Subscribe(value => CalculateScratchpadIsDisabled()));

            CalculateScratchpadIsDisabled();
        }

        private void ReleaseKeysOnApplicationExit()
        {
            Application.Current.Exit += (o, args) =>
            {
                if (keyboardService.KeyDownStates[KeyValues.PublishKey].Value.IsDownOrLockedDown())
                {
                    publishService.ReleaseAllDownKeys();
                }
            };
        }
        
        private void KeySelectionResult(KeyValue? singleKeyValue, List<string> multiKeySelection)
        {
            //Single key string
            if (singleKeyValue != null
                && !string.IsNullOrEmpty(singleKeyValue.Value.String))
            {
                Log.Debug(string.Format("KeySelectionResult received with string value '{0}'", singleKeyValue.Value.String.ConvertEscapedCharsToLiterals()));
                outputService.ProcessSingleKeyText(singleKeyValue.Value.String);
            }

            //Single key function key
            if (singleKeyValue != null
                && singleKeyValue.Value.FunctionKey != null)
            {
                Log.Debug(string.Format("KeySelectionResult received with function key value '{0}'", singleKeyValue.Value.FunctionKey));
                HandleFunctionKeySelectionResult(singleKeyValue.Value);
            }

            //Multi key selection
            if (multiKeySelection != null
                && multiKeySelection.Any())
            {
                Log.Debug(string.Format("KeySelectionResult received with '{0}' multiKeySelection results", multiKeySelection.Count));
                outputService.ProcessMultiKeyTextAndSuggestions(multiKeySelection);
            }
        }

        private void HandleFunctionKeySelectionResult(KeyValue singleKeyValue)
        {
            if (singleKeyValue.FunctionKey != null)
            {
                keyboardService.ProgressKeyDownState(singleKeyValue);

                switch (singleKeyValue.FunctionKey.Value)
                {
                    case FunctionKeys.AddToDictionary:
                        AddTextToDictionary();
                        break;

                    case FunctionKeys.AlphaKeyboard:
                        Log.Debug("Changing keyboard to Alpha.");
                        Keyboard = new Alpha();
                        break;

                    case FunctionKeys.AlternativeAlpha1Keyboard:
                        Log.Debug("Changing keyboard to AlternativeAlpha1.");
                        Keyboard = new AlternativeAlpha1();
                        break;

                    case FunctionKeys.AlternativeAlpha2Keyboard:
                        Log.Debug("Changing keyboard to AlternativeAlpha2.");
                        Keyboard = new AlternativeAlpha2();
                        break;

                    case FunctionKeys.AlternativeAlpha3Keyboard:
                        Log.Debug("Changing keyboard to AlternativeAlpha3.");
                        Keyboard = new AlternativeAlpha3();
                        break;

                    case FunctionKeys.BackFromKeyboard:
                        Log.Debug("Navigating back from keyboard.");
                        var navigableKeyboard = Keyboard as INavigableKeyboard;
                        Keyboard = navigableKeyboard != null && navigableKeyboard.Back != null
                            ? navigableKeyboard.Back
                            : new Alpha();
                        break;

                    case FunctionKeys.Calibrate:
                        if (CalibrationService != null)
                        {
                            Log.Debug("Calibrate requested.");

                            var previousKeyboard = Keyboard;

                            Keyboard = new YesNoQuestion(
                                "Are you sure you would like to re-calibrate?",
                                () =>
                                {
                                    keyboardService.KeyEnabledStates.DisableAll = true;

                                    CalibrateRequest.Raise(new CalibrationResult(), calibrationResult =>
                                    {
                                        if (calibrationResult.Success)
                                        {
                                            NotificationRequest.Raise(new Notification
                                            {
                                                Title = "Success",
                                                Content = calibrationResult.Message
                                            }, __ => { keyboardService.KeyEnabledStates.DisableAll = false; });

                                            audioService.PlaySound(Settings.Default.InfoSoundFile);
                                        }
                                        else
                                        {
                                            if (calibrationResult.Exception != null)
                                            {
                                                keyboardService.KeyEnabledStates.DisableAll = true;

                                                ErrorNotificationRequest.Raise(new Notification
                                                {
                                                    Title = "Uh-oh!",
                                                    Content = calibrationResult.Exception.Message
                                                }, notification => { keyboardService.KeyEnabledStates.DisableAll = false; });

                                                audioService.PlaySound(Settings.Default.ErrorSoundFile);
                                            }
                                        }
                                    });

                                    Keyboard = previousKeyboard;
                                },
                                () =>
                                {
                                    Keyboard = previousKeyboard;
                                });
                        }
                        break;

                    case FunctionKeys.Currencies1Keyboard:
                        Log.Debug("Changing keyboard to Currencies1.");
                        Keyboard = new Currencies1();
                        break;

                    case FunctionKeys.Currencies2Keyboard:
                        Log.Debug("Changing keyboard to Currencies2.");
                        Keyboard = new Currencies2();
                        break;

                    case FunctionKeys.MenuKeyboard:
                        Log.Debug("Changing keyboard to Menu.");
                        Keyboard = new Menu(Keyboard);
                        break;

                    case FunctionKeys.NoQuestionResult:
                        HandleYesNoQuestionResult(false);
                        break;

                    case FunctionKeys.NumericAndSymbols1Keyboard:
                        Log.Debug("Changing keyboard to NumericAndSymbols1.");
                        Keyboard = new NumericAndSymbols1();
                        break;

                    case FunctionKeys.NextSuggestions:
                        Log.Debug("Incrementing suggestions page.");

                        if (suggestionService.Suggestions != null
                            && (suggestionService.Suggestions.Count > (suggestionService.SuggestionsPage + 1) * SuggestionService.SuggestionsPerPage))
                        {
                            suggestionService.SuggestionsPage++;
                        }
                        break;

                    case FunctionKeys.PreviousSuggestions:
                        Log.Debug("Decrementing suggestions page.");

                        if (suggestionService.SuggestionsPage > 0)
                        {
                            suggestionService.SuggestionsPage--;
                        }
                        break;

                    case FunctionKeys.PhysicalKeysKeyboard:
                        Log.Debug("Changing keyboard to PhysicalKeys.");
                        Keyboard = new PhysicalKeys();
                        break;

                    case FunctionKeys.Speak:
                        audioService.Speak(
                            outputService.Text,
                            Settings.Default.SpeechVolume,
                            Settings.Default.SpeechRate,
                            Settings.Default.SpeechVoice);
                        break;

                    case FunctionKeys.SettingCategoriesKeyboard:
                        Log.Debug("Changing keyboard to SettingCategories.");
                        Keyboard = new SettingCategories(Keyboard);
                        break;

                    case FunctionKeys.NumericAndSymbols2Keyboard:
                        Log.Debug("Changing keyboard to NumericAndSymbols2.");
                        Keyboard = new NumericAndSymbols2();
                        break;

                    case FunctionKeys.NumericAndSymbols3Keyboard:
                        Log.Debug("Changing keyboard to Symbols3.");
                        Keyboard = new Symbols3();
                        break;

                    case FunctionKeys.YesQuestionResult:
                        HandleYesNoQuestionResult(true);
                        break;
                }

                outputService.ProcessFunctionKey(singleKeyValue.FunctionKey.Value);
            }
        }

        private void AddTextToDictionary()
        {
            Log.Debug("AddTextToDictionary called.");

            var possibleEntries = outputService.Text.ExtractWordsAndLines();

            if (possibleEntries != null)
            {
                var candidates = possibleEntries.Where(pe => !dictionaryService.ExistsInDictionary(pe)).ToList();

                if (candidates.Any())
                {
                    PromptToAddCandidatesToDictionary(candidates, Keyboard);
                }
                else
                {
                    Log.Debug(string.Format("No new words or phrases found in output service's Text: '{0}'.", outputService.Text));

                    keyboardService.KeyEnabledStates.DisableAll = true;

                    NotificationRequest.Raise(new Notification
                    {
                        Title = "Hmm",
                        Content = "It doesn't look like the scratchpad contains any words or phrases that don't already exist in the dictionary."
                    }, notification => { keyboardService.KeyEnabledStates.DisableAll = false; });

                    audioService.PlaySound(Settings.Default.InfoSoundFile);
                }
            }
            else
            {
                Log.Debug(string.Format("No possible words or phrases found in output service's Text: '{0}'.", outputService.Text));

                keyboardService.KeyEnabledStates.DisableAll = true; 

                NotificationRequest.Raise(new Notification
                {
                    Title = "Hmm",
                    Content = "It doesn't look like the scratchpad contains any words or phrases that could be added to the dictionary."
                }, notification => { keyboardService.KeyEnabledStates.DisableAll = false; });

                audioService.PlaySound(Settings.Default.InfoSoundFile);
            }
        }

        private void PromptToAddCandidatesToDictionary(List<string> candidates, IKeyboard originalKeyboard)
        {
            if (candidates.Any())
            {
                var candidate = candidates.First();

                var prompt = candidate.Contains(' ')
                    ? string.Format("Would you like to add the phrase '{0}' to the dictionary with shortcut '{1}'?", 
                        candidate, candidate.CreateDictionaryEntryHash(log: true))
                    : string.Format("Would you like to add the word '{0}' to the dictionary?", candidate);

                var similarEntries = dictionaryService.GetAllEntriesWithUsageCounts()
                    .Where(de => string.Equals(de.Entry, candidate, StringComparison.InvariantCultureIgnoreCase))
                    .Select(de => de.Entry)
                    .ToList();

                if (similarEntries.Any())
                {
                    string similarEntriesPrompt = string.Format("(FYI some similar entries are already in the dictionary: {0})", 
                        string.Join(", ", similarEntries.Select(se => string.Format("'{0}'", se))));

                    prompt = string.Concat(prompt, "\n\n", similarEntriesPrompt);
                }

                Action nextAction = candidates.Count > 1
                        ? (Action)(() => PromptToAddCandidatesToDictionary(candidates.Skip(1).ToList(), originalKeyboard))
                        : (Action)(() => Keyboard = originalKeyboard);

                Keyboard = new YesNoQuestion(
                    prompt,
                    () =>
                    {
                        dictionaryService.AddNewEntryToDictionary(candidate);

                        keyboardService.KeyEnabledStates.DisableAll = true;

                        NotificationRequest.Raise(new Notification
                        {
                            Title = "Added",
                            Content = string.Format("Great stuff. '{0}' has been added to the dictionary.", candidate)
                        }, notification => { keyboardService.KeyEnabledStates.DisableAll = false; });

                        nextAction();
                    },
                    () => nextAction());
            }
        }

        private void HandleYesNoQuestionResult(bool yesResult)
        {
            Log.Debug(string.Format("YesNoQuestion result of '{0}' received.", yesResult ? "YES" : "NO"));

            var yesNoQuestion = Keyboard as YesNoQuestion;
            if (yesNoQuestion != null)
            {
                if (yesResult)
                {
                    yesNoQuestion.YesAction();
                }
                else
                {
                    yesNoQuestion.NoAction();
                }
            }
        }
        
        private void CalculateScratchpadIsDisabled()
        {
            ScratchpadIsDisabled = KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => 
                keyboardService.KeyDownStates[kv].Value.IsDownOrLockedDown());
        }

        private void ResetSelectionProgress()
        {
            PointSelectionProgress = null;

            if (keyboardService != null)
            {
                keyboardService.KeySelectionProgress.Clear();
            }
        }

        private void HandleServiceError(object sender, Exception exception)
        {
            Log.Error("Error event received from service. Raising ErrorNotificationRequest and playing ErrorSoundFile (from settings)", exception);

            keyboardService.KeyEnabledStates.DisableAll = true;

            ErrorNotificationRequest.Raise(new Notification
            {
                Title = "Uh-oh!",
                Content = exception.Message
            }, notification => { keyboardService.KeyEnabledStates.DisableAll = false; });

            audioService.PlaySound(Settings.Default.ErrorSoundFile);
        }

        #endregion
    }
}
