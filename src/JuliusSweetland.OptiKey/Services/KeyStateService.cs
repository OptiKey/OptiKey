using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class KeyStateService : BindableBase, IKeyStateService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly NotifyingConcurrentDictionary<KeyValue, double> keySelectionProgress;
        private readonly NotifyingConcurrentDictionary<KeyValue, KeyDownStates> keyDownStates;
        private readonly KeyEnabledStates keyEnabledStates;
        private readonly Action<KeyValue> fireKeySelectionEvent;
        private readonly Dictionary<bool, KeyStateServiceState> state = new Dictionary<bool, KeyStateServiceState>();
        
        private bool simulateKeyStrokes;
        private bool turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased;
        
        #endregion

        #region Ctor

        public KeyStateService(
            ISuggestionStateService suggestionService, 
            ICapturingStateManager capturingStateManager,
            ILastMouseActionStateManager lastMouseActionStateManager,
            ICalibrationService calibrationService,
            Action<KeyValue> fireKeySelectionEvent)
        {
            this.fireKeySelectionEvent = fireKeySelectionEvent;
            keySelectionProgress = new NotifyingConcurrentDictionary<KeyValue, double>();
            keyDownStates = new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>();
            keyEnabledStates = new KeyEnabledStates(this, suggestionService, capturingStateManager, lastMouseActionStateManager, calibrationService);

            InitialiseKeyDownStates();
            AddSettingChangeHandlers();
            AddSimulateKeyStrokesChangeHandler();
            AddKeyDownStatesChangeHandlers();
        }

        #endregion

        #region Properties

        public bool SimulateKeyStrokes
        {
            get { return simulateKeyStrokes; }
            set { SetProperty(ref simulateKeyStrokes, value); }
        }
        public NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get { return keySelectionProgress; } }
        public NotifyingConcurrentDictionary<KeyValue, KeyDownStates> KeyDownStates { get { return keyDownStates; } }
        public KeyEnabledStates KeyEnabledStates { get { return keyEnabledStates; } }

        #endregion

        #region Public Methods

        public void ProgressKeyDownState(KeyValue keyValue)
        {
            if (KeyValues.KeysWhichCanBePressedDown.Contains(keyValue)
                && KeyDownStates[keyValue].Value == Enums.KeyDownStates.Up)
            {
                Log.DebugFormat("Changing key down state of '{0}' key from UP to DOWN.", keyValue);
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.Down;
            }
            else if (KeyValues.KeysWhichCanBeLockedDown.Contains(keyValue)
                     && !KeyValues.KeysWhichCanBePressedDown.Contains(keyValue)
                     && KeyDownStates[keyValue].Value == Enums.KeyDownStates.Up)
            {
                Log.DebugFormat("Changing key down state of '{0}' key from UP to LOCKED DOWN.", keyValue);
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.LockedDown;
            }
            else if (KeyValues.KeysWhichCanBeLockedDown.Contains(keyValue)
                     && KeyDownStates[keyValue].Value == Enums.KeyDownStates.Down)
            {
                Log.DebugFormat("Changing key down state of '{0}' key from DOWN to LOCKED DOWN.", keyValue);
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.LockedDown;
            }
            else if (KeyDownStates[keyValue].Value != Enums.KeyDownStates.Up)
            {
                Log.DebugFormat("Changing key down state of '{0}' key from {1} to UP.", keyValue,
                    KeyDownStates[keyValue].Value == Enums.KeyDownStates.Down ? "DOWN" : "LOCKED DOWN");
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.Up;
            }
        }

        #endregion

        #region Private Methods

        private void InitialiseKeyDownStates()
        {
            Log.Info("Initialising KeyDownStates.");
            
            KeyDownStates[KeyValues.MouseMagnifierKey].Value =
                Settings.Default.MouseMagnifierLockedDown ? Enums.KeyDownStates.LockedDown : Enums.KeyDownStates.Up;

            SetMultiKeySelectionKeyStateFromSetting();
        }

        private void SetMultiKeySelectionKeyStateFromSetting()
        {
            KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value =
                Settings.Default.MultiKeySelectionEnabled &&
                ((SimulateKeyStrokes && Settings.Default.MultiKeySelectionLockedDownWhenSimulatingKeyStrokes)
                || (!SimulateKeyStrokes && Settings.Default.MultiKeySelectionLockedDownWhenNotSimulatingKeyStrokes))
                    ? Enums.KeyDownStates.LockedDown
                    : Enums.KeyDownStates.Up;
        }

        private void AddSettingChangeHandlers()
        {
            Log.Info("Adding setting change handlers.");
            
            Settings.Default.OnPropertyChanges(s => s.MultiKeySelectionEnabled).Where(mkse => !mkse).Subscribe(_ =>
            {
                //Release multi-key selection key if multi-key selection is disabled from the settings
                KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value = Enums.KeyDownStates.Up;
            });
        }

        private void AddSimulateKeyStrokesChangeHandler()
        {
            Log.Info("Adding KeyDownStates change handlers.");
            this.OnPropertyChanges(t => t.SimulateKeyStrokes).Subscribe(_ => ReactToSimulateKeyStrokesChanges(true));
            ReactToSimulateKeyStrokesChanges(false);
        }

        //N.B. This method will be called before any other classes can react to the SimulateKeyStrokes change
        private void ReactToSimulateKeyStrokesChanges(bool saveCurrentState)
        {
            var newStateKey = SimulateKeyStrokes;
            var currentStateKey = !newStateKey;

            //Save old state values
            if (saveCurrentState)
            {
                var currentState = new KeyStateServiceState(currentStateKey, this);
                if (state.ContainsKey(currentStateKey))
                {
                    state[currentStateKey] = currentState;
                }
                else
                {
                    state.Add(currentStateKey, currentState);
                }
            }

            //Restore state or default state
            if (state.ContainsKey(newStateKey))
            {
                state[newStateKey].RestoreState();
            }
            else
            {
                if (SimulateKeyStrokes)
                {
                    Log.Info("No stored KeyStateService state to restore for SimulateKeyStrokes=true. Defaulting Multi-Key Selection key state.");
                    SetMultiKeySelectionKeyStateFromSetting();
                }
                else
                {
                    Log.Info("No stored KeyStateService state to restore for SimulateKeyStrokes=false.  Defaulting Multi-Key Selection key state & releasing all publish only keys.");
                    SetMultiKeySelectionKeyStateFromSetting();
                    foreach (var keyValue in KeyDownStates.Keys)
                    {
                        if (KeyValues.PublishOnlyKeys.Contains(keyValue)
                            && KeyDownStates[keyValue].Value.IsDownOrLockedDown())
                        {
                            Log.InfoFormat("Releasing '{0}' key.", keyValue);
                            KeyDownStates[keyValue].Value = Enums.KeyDownStates.Up;
                            if (fireKeySelectionEvent != null) fireKeySelectionEvent(keyValue);
                        }
                    }
                }
            }
        }

        private void AddKeyDownStatesChangeHandlers()
        {
            Log.Info("Adding KeyDownStates change handlers.");

            KeyDownStates[KeyValues.MouseMagnifierKey].OnPropertyChanges(s => s.Value).Subscribe(value => 
                Settings.Default.MouseMagnifierLockedDown = KeyDownStates[KeyValues.MouseMagnifierKey].Value == Enums.KeyDownStates.LockedDown);

            KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].OnPropertyChanges(s => s.Value).Subscribe(value =>
            {
                if (SimulateKeyStrokes)
                {
                    Settings.Default.MultiKeySelectionLockedDownWhenSimulatingKeyStrokes = 
                        KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value == Enums.KeyDownStates.LockedDown;
                }
                else
                {
                    Settings.Default.MultiKeySelectionLockedDownWhenNotSimulatingKeyStrokes = 
                        KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value == Enums.KeyDownStates.LockedDown;
                }
            });

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                KeyDownStates[kv].OnPropertyChanges(s => s.Value).Subscribe(value => CalculateMultiKeySelectionSupported()));
            CalculateMultiKeySelectionSupported();
        }

        private void CalculateMultiKeySelectionSupported()
        {
            Log.Info("CalculateMultiKeySelectionSupported called.");

            if (KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value.IsDownOrLockedDown()
                && KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.Info("A key which prevents text capture is down - toggling MultiKeySelectionIsOn to false.");

                //Automatically turn multi-key capture back on again when appropriate if it is currently locked down (if it is just down then let it go)
                turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased =
                    KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value == Enums.KeyDownStates.LockedDown;

                KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value = Enums.KeyDownStates.Up;
                if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.MultiKeySelectionIsOnKey);
            }
            else if (turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased
                && !KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => KeyDownStates[kv].Value.IsDownOrLockedDown())
                && Settings.Default.MultiKeySelectionEnabled)
            {
                Log.Info("No keys which prevents text capture is down - returing setting MultiKeySelectionIsOn to true.");

                KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value = Enums.KeyDownStates.LockedDown;
                if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.MultiKeySelectionIsOnKey);
                turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased = false;
            }
        }

        #endregion
    }
}
