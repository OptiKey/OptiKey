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
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;
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
        private readonly IKeyStateService keyStateService;
        private readonly ISuggestionStateService suggestionService;
        private readonly ICapturingStateManager capturingStateManager;
        private readonly ILastMouseActionStateManager lastMouseActionStateManager;
        private readonly IInputService inputService;
        private readonly IKeyboardOutputService keyboardOutputService;
        private readonly IMouseOutputService mouseOutputService;
        private readonly IWindowManipulationService mainWindowManipulationService;
        private readonly List<INotifyErrors> errorNotifyingServices; 

        private readonly InteractionRequest<NotificationWithCalibrationResult> calibrateRequest;
        
        private SelectionModes selectionMode;
        private Point currentPositionPoint;
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
            IKeyStateService keyStateService,
            ISuggestionStateService suggestionService,
            ICapturingStateManager capturingStateManager,
            ILastMouseActionStateManager lastMouseActionStateManager,
            IInputService inputService,
            IKeyboardOutputService keyboardOutputService,
            IMouseOutputService mouseOutputService,
            IWindowManipulationService mainWindowManipulationService,
            List<INotifyErrors> errorNotifyingServices)
        {
            Log.Debug("Ctor called.");

            this.audioService = audioService;
            this.calibrationService = calibrationService;
            this.dictionaryService = dictionaryService;
            this.keyStateService = keyStateService;
            this.suggestionService = suggestionService;
            this.capturingStateManager = capturingStateManager;
            this.lastMouseActionStateManager = lastMouseActionStateManager;
            this.inputService = inputService;
            this.keyboardOutputService = keyboardOutputService;
            this.mouseOutputService = mouseOutputService;
            this.mainWindowManipulationService = mainWindowManipulationService;
            this.errorNotifyingServices = errorNotifyingServices;

            calibrateRequest = new InteractionRequest<NotificationWithCalibrationResult>();
            SelectionMode = SelectionModes.Key;
            
            InitialiseKeyboard(mainWindowManipulationService);
            AttachScratchpadEnabledListener();
            AttachKeyboardSupportsCollapsedDockListener(mainWindowManipulationService);
            AttachKeyboardSupportsSimulateKeyStrokesListener();
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
        public IKeyboardOutputService KeyboardOutputService { get { return keyboardOutputService; } }
        public IKeyStateService KeyStateService { get { return keyStateService; } }
        public ISuggestionStateService SuggestionService { get { return suggestionService; } }
        public ICalibrationService CalibrationService { get { return calibrationService; } }

        private IKeyboard keyboard;
        public IKeyboard Keyboard
        {
            get { return keyboard; }
            set { SetProperty(ref keyboard, value); }
        }

        private bool keyboardSupportsCollapsedDock = true;
        public bool KeyboardSupportsCollapsedDock
        {
            get { return keyboardSupportsCollapsedDock; }
            set { SetProperty(ref keyboardSupportsCollapsedDock, value); }
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

        public Point CurrentPositionPoint
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

        public void FireKeySelectionEvent(KeyValue kv)
        {
            if (KeySelection != null)
            {
                KeySelection(this, kv);
            }
        }

        private void InitialiseKeyboard(IWindowManipulationService windowManipulationService)
        {
            if (Settings.Default.ConversationOnlyMode)
            {
                Keyboard = new ConversationAlpha();
                windowManipulationService.Maximise();
            }
            else
            {
                switch (Settings.Default.StartupKeyboard)
                {
                    case Enums.Keyboards.Alpha:
                        Keyboard = new Alpha();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.ConversationAlpha:
                        Keyboard = new ConversationAlpha();
                        windowManipulationService.Maximise();
                        break;

                    case Enums.Keyboards.ConversationNumericAndSymbols:
                        Keyboard = new ConversationNumericAndSymbols();
                        windowManipulationService.Maximise();
                        break;

                    case Enums.Keyboards.Currencies1:
                        Keyboard = new Currencies1();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.Currencies2:
                        Keyboard = new Currencies2();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.Diacritics1:
                        Keyboard = new Diacritics1();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.Diacritics2:
                        Keyboard = new Diacritics2();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.Diacritics3:
                        Keyboard = new Diacritics3();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.Menu:
                        Keyboard = new Menu(() => Keyboard = new Alpha());
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.Minimised:
                        Keyboard = new Minimised(() =>
                        {
                            Keyboard = new Menu(() => Keyboard = new Alpha());
                            windowManipulationService.Restore();
                        });
                        windowManipulationService.Minimise();
                        break;

                    case Enums.Keyboards.Mouse:
                        Keyboard = new Mouse(() => Keyboard = new Menu(() => Keyboard = new Alpha()));
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.NumericAndSymbols1:
                        Keyboard = new NumericAndSymbols1();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.NumericAndSymbols2:
                        Keyboard = new NumericAndSymbols2();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.NumericAndSymbols3:
                        Keyboard = new NumericAndSymbols3();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.PhysicalKeys:
                        Keyboard = new PhysicalKeys();
                        windowManipulationService.Restore();
                        break;

                    case Enums.Keyboards.SizeAndPosition:
                        Keyboard = new SizeAndPosition(() => Keyboard = new Menu(() => Keyboard = new Alpha()));
                        windowManipulationService.Restore();
                        break;
                }
            }
        }

        private void AddTextToDictionary()
        {
            Log.Debug("AddTextToDictionary called.");

            var possibleEntries = keyboardOutputService.Text.ExtractWordsAndLines();

            if (possibleEntries != null)
            {
                var candidates = possibleEntries.Where(pe => !dictionaryService.ExistsInDictionary(pe)).ToList();

                if (candidates.Any())
                {
                    PromptToAddCandidatesToDictionary(candidates, Keyboard);
                }
                else
                {
                    Log.DebugFormat("No new words or phrases found in output service's Text: '{0}'.", keyboardOutputService.Text);

                    inputService.RequestSuspend();
                    audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
                    RaiseToastNotification("Hmm, nothing new here!", "It doesn't look like the scratchpad contains any words or phrases that don't already exist in the dictionary.", 
                        NotificationTypes.Normal, () => { inputService.RequestResume(); });
                }
            }
            else
            {
                Log.DebugFormat("No possible words or phrases found in output service's Text: '{0}'.", keyboardOutputService.Text);
                audioService.PlaySound(Settings.Default.InfoSoundFile, Settings.Default.InfoSoundVolume);
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
                        inputService.RequestSuspend();
                        nextAction();

                        RaiseToastNotification("Added", string.Format("Great stuff. '{0}' has been added to the dictionary.", candidate),
                            NotificationTypes.Normal, () => { inputService.RequestResume(); });
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
            Action calculateScratchpadIsDisabled = () =>
                ScratchpadIsDisabled = KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                        keyStateService.KeyDownStates[kv].Value.IsDownOrLockedDown());

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                keyStateService.KeyDownStates[kv].OnPropertyChanges(s => s.Value)
                    .Subscribe(value => calculateScratchpadIsDisabled()));

            calculateScratchpadIsDisabled();
        }

        private void AttachKeyboardSupportsSimulateKeyStrokesListener()
        {
            Action<IKeyboard> setSimulateKeyStrokes = kb =>
            {
                keyStateService.SimulateKeyStrokes = kb.SimulateKeyStrokes;
            };
            this.OnPropertyChanges(mvm => mvm.Keyboard).Subscribe(setSimulateKeyStrokes);
            setSimulateKeyStrokes(Keyboard);
        }

        private void AttachKeyboardSupportsCollapsedDockListener(IWindowManipulationService mainWindowManipulationService)
        {
            Action<bool> resizeDockIfCollapsedDockingNotSupported = collapsedDockingSupported =>
            {
                if (!collapsedDockingSupported
                    && Settings.Default.MainWindowState == WindowStates.Docked
                    && Settings.Default.MainWindowDockSize == DockSizes.Collapsed)
                {
                    mainWindowManipulationService.ResizeDockToFull();
                }
            };
            this.OnPropertyChanges(mvm => mvm.KeyboardSupportsCollapsedDock).Subscribe(resizeDockIfCollapsedDockingNotSupported);
            resizeDockIfCollapsedDockingNotSupported(KeyboardSupportsCollapsedDock);
        }

        private void ResetSelectionProgress()
        {
            PointSelectionProgress = null;

            if (keyStateService != null)
            {
                keyStateService.KeySelectionProgress.Clear();
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
