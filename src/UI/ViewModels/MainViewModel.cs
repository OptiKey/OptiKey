using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Static;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards;
using log4net;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels
{
    public partial class MainViewModel : BindableBase
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAudioService audioService;
        private readonly ICalibrationService calibrationService;
        private readonly IDictionaryService dictionaryService;
        private readonly IKeyboardService keyboardService;
        private readonly ISuggestionStateService suggestionService;
        private readonly ICapturingStateManager capturingStateManager;
        private readonly ILastMouseActionStateManager lastMouseActionStateManager;
        private readonly IInputService inputService;
        private readonly IOutputService outputService;
        private readonly IWindowManipulationService mainWindowManipulationService;
        private readonly List<INotifyErrors> errorNotifyingServices; 

        private readonly InteractionRequest<NotificationWithCalibrationResult> calibrateRequest;
        
        private SelectionModes selectionMode;
        private Point? currentPositionPoint;
        private KeyValue? currentPositionKey;
        private Tuple<Point, double> pointSelectionProgress;
        private Dictionary<Rect, KeyValue> pointToKeyValueMap;
        private bool showCursor;
        private Action<Point> nextPointSelectionAction;
        private Point? magnifyAtPoint;
        private Action<Point?> magnifiedPointSelectionAction;

        #endregion

        #region Ctor

        public MainViewModel(
            IAudioService audioService,
            ICalibrationService calibrationService,
            IDictionaryService dictionaryService,
            IKeyboardService keyboardService,
            ISuggestionStateService suggestionService,
            ICapturingStateManager capturingStateManager,
            ILastMouseActionStateManager lastMouseActionStateManager,
            IInputService inputService,
            IOutputService outputService,
            IWindowManipulationService mainWindowManipulationService,
            List<INotifyErrors> errorNotifyingServices)
        {
            Log.Debug("Ctor called.");

            this.audioService = audioService;
            this.calibrationService = calibrationService;
            this.dictionaryService = dictionaryService;
            this.keyboardService = keyboardService;
            this.suggestionService = suggestionService;
            this.capturingStateManager = capturingStateManager;
            this.lastMouseActionStateManager = lastMouseActionStateManager;
            this.inputService = inputService;
            this.outputService = outputService;
            this.mainWindowManipulationService = mainWindowManipulationService;
            this.errorNotifyingServices = errorNotifyingServices;

            calibrateRequest = new InteractionRequest<NotificationWithCalibrationResult>();
            
            SelectionMode = SelectionModes.Key;
            Keyboard = new Alpha();

            SelectKeyboardOnKeyboardSetChanges();
            AttachScratchpadEnabledListener();

            HandleFunctionKeySelectionResult(KeyValues.LeftShiftKey); //Set initial shift state to on
        }

        #endregion

        #region Events

        public event EventHandler<NotificationEventArgs> ToastNotification;
        public event EventHandler<KeyValue> KeySelection;
        public event EventHandler<Point> PointSelection;

        #endregion

        #region Properties

        public IInputService InputService { get { return inputService; } }
        public ICapturingStateManager CapturingStateManager { get { return capturingStateManager; } }
        public IOutputService OutputService { get { return outputService; } }
        public IKeyboardService KeyboardService { get { return keyboardService; } }
        public ISuggestionStateService SuggestionService { get { return suggestionService; } }
        public ICalibrationService CalibrationService { get { return calibrationService; } }

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

                    inputService.PointToKeyValueMap = value;
                    SelectionResultPoints = null; //The last selection result points cannot be valid if this has changed (window has moved or resized)
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
                    Log.DebugFormat("SelectionMode changed to {0}", value);

                    ResetSelectionProgress();

                    if (inputService != null)
                    {
                        inputService.SelectionMode = value;
                    }
                }
            }
        }

        public bool ShowCursor
        {
            get { return showCursor; }
            set { SetProperty(ref showCursor, value); }
        }

        public Point? MagnifyAtPoint
        {
            get { return magnifyAtPoint; }
            set { SetProperty(ref magnifyAtPoint, value); }
        }

        public Action<Point?> MagnifiedPointSelectionAction
        {
            get { return magnifiedPointSelectionAction; }
            set { SetProperty(ref magnifiedPointSelectionAction, value); }
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
            private set { SetProperty(ref pointSelectionProgress, value); }
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
        
        public string ApplicationAndSystemInfo
        {
            get
            {
                return string.Format("v:{0} | P:{1} {2} | OS:{3} {4} ({5})",
                    DiagnosticInfo.AssemblyVersion,
                    DiagnosticInfo.ProcessBitness,
                    DiagnosticInfo.IsProcessElevated ? "(Elevated)" : "(Not Elevated)",
                    DiagnosticInfo.OperatingSystemBitness,
                    DiagnosticInfo.OperatingSystemVersion,
                    DiagnosticInfo.OperatingSystemServicePack);
            }
        }

        private bool scratchpadIsDisabled;
        public bool ScratchpadIsDisabled
        {
            get { return scratchpadIsDisabled; }
            set { SetProperty(ref scratchpadIsDisabled, value); }
        }

        public InteractionRequest<NotificationWithCalibrationResult> CalibrateRequest { get { return calibrateRequest; } }
        
        #endregion

        #region Methods

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
                    Log.DebugFormat("No new words or phrases found in output service's Text: '{0}'.", outputService.Text);

                    inputService.State = RunningStates.Paused;
                    audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                    RaiseToastNotification("Hmm", "It doesn't look like the scratchpad contains any words or phrases that don't already exist in the dictionary.", 
                        NotificationTypes.Normal, () => { inputService.State = RunningStates.Running; });
                }
            }
            else
            {
                Log.DebugFormat("No possible words or phrases found in output service's Text: '{0}'.", outputService.Text);

                inputService.State = RunningStates.Paused;
                audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                RaiseToastNotification("Hmm", "It doesn't look like the scratchpad contains any words or phrases that could be added to the dictionary.", 
                    NotificationTypes.Normal, () => { inputService.State = RunningStates.Running; });
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

                if (candidate.Any(Char.IsUpper))
                {
                    prompt = string.Concat(prompt, "\n(The new dictionary entry will contain capital letters)");
                }

                var similarEntries = dictionaryService.GetAllEntries()
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
                        inputService.State = RunningStates.Paused;
                        nextAction();

                        RaiseToastNotification("Added", string.Format("Great stuff. '{0}' has been added to the dictionary.", candidate),
                            NotificationTypes.Normal, () => { inputService.State = RunningStates.Running; });
                    },
                    () => nextAction());
            }
        }

        private void HandleYesNoQuestionResult(bool yesResult)
        {
            Log.DebugFormat("YesNoQuestion result of '{0}' received.", yesResult ? "YES" : "NO");

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

        private void AttachScratchpadEnabledListener()
        {
            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                keyboardService.KeyDownStates[kv].OnPropertyChanges(s => s.Value)
                    .Subscribe(value => CalculateScratchpadIsDisabled()));

            CalculateScratchpadIsDisabled();
        }

        private void CalculateScratchpadIsDisabled()
        {
            ScratchpadIsDisabled = KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => 
                keyboardService.KeyDownStates[kv].Value.IsDownOrLockedDown());
        }

        private void SelectKeyboardOnKeyboardSetChanges()
        {
            Settings.Default.OnPropertyChanges(s => s.KeyboardSet).Subscribe(visualMode =>
            {
                //Listen to KeyboardSet changes and reset keyboard to Alpha if mode changed to SpeechOnly
                if (visualMode == KeyboardsSets.SpeechOnly)
                {
                    Keyboard = new Alpha();
                }
            });
        }

        private void ResetSelectionProgress()
        {
            PointSelectionProgress = null;

            if (keyboardService != null)
            {
                keyboardService.KeySelectionProgress.Clear();
            }
        }

        internal void RaiseToastNotification(string title, string content, NotificationTypes notificationType, Action callback)
        {
            if (ToastNotification != null)
            {
                ToastNotification(this, new NotificationEventArgs(title, content, notificationType, callback));
            }
        }

        #endregion
    }
}
