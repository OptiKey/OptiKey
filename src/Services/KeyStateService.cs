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
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class KeyStateService : BindableBase, IKeyStateService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            set {SetProperty(ref simulateKeyStrokes, value); }
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
            Log.Debug("Initialising KeyDownStates.");
            
            KeyDownStates[KeyValues.MouseMagnifierKey].Value =
                Settings.Default.MouseMagnifierLockedDown ? Enums.KeyDownStates.LockedDown : Enums.KeyDownStates.Up;

            KeyDownStates[KeyValues.MultiKeySelectionKey].Value =
                Settings.Default.MultiKeySelectionEnabled && Settings.Default.MultiKeySelectionLockedDown
                    ? Enums.KeyDownStates.LockedDown
                    : Enums.KeyDownStates.Up;
        }

        private void AddSettingChangeHandlers()
        {
            Log.Debug("Adding setting change handlers.");
            
            Settings.Default.OnPropertyChanges(s => s.MultiKeySelectionEnabled).Where(mkse => !mkse).Subscribe(_ =>
            {
                //Release multi-key selection key if multi-key selection is disabled from the settings
                KeyDownStates[KeyValues.MultiKeySelectionKey].Value = Enums.KeyDownStates.Up;
                if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.MultiKeySelectionKey);
            });
        }

        private void AddSimulateKeyStrokesChangeHandler()
        {
            Log.Debug("Adding KeyDownStates change handlers.");
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
                    Log.Debug("No stored KeyStateService state to restore for SimulateKeyStrokes=true.");
                }
                else
                {
                    Log.Debug("No stored KeyStateService state to restore for SimulateKeyStrokes=false - releasing all publish only keys.");
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
            Log.Debug("Adding KeyDownStates change handlers.");

            KeyDownStates[KeyValues.MouseMagnifierKey].OnPropertyChanges(s => s.Value).Subscribe(value =>
            {
                Settings.Default.MouseMagnifierLockedDown = KeyDownStates[KeyValues.MouseMagnifierKey].Value == Enums.KeyDownStates.LockedDown;
                Settings.Default.Save();
            });

            KeyDownStates[KeyValues.MultiKeySelectionKey].OnPropertyChanges(s => s.Value).Subscribe(value =>
            {
                Settings.Default.MultiKeySelectionLockedDown = KeyDownStates[KeyValues.MultiKeySelectionKey].Value == Enums.KeyDownStates.LockedDown;
                Settings.Default.Save();
            });

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                KeyDownStates[kv].OnPropertyChanges(s => s.Value).Subscribe(value => CalculateMultiKeySelectionSupported()));
            CalculateMultiKeySelectionSupported();
        }

        private void CalculateMultiKeySelectionSupported()
        {
            Log.Debug("CalculateMultiKeySelectionSupported called.");

            if (KeyDownStates[KeyValues.MultiKeySelectionKey].Value.IsDownOrLockedDown()
                && KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.Debug("A key which prevents text capture is down - toggling MultiKeySelectionIsOn to false.");

                //Automatically turn multi-key capture back on again when appropriate if it is currently locked down (if it is just down then let it go)
                turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased =
                    KeyDownStates[KeyValues.MultiKeySelectionKey].Value == Enums.KeyDownStates.LockedDown;

                KeyDownStates[KeyValues.MultiKeySelectionKey].Value = Enums.KeyDownStates.Up;
                if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.MultiKeySelectionKey);
            }
            else if (turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased
                && !KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => KeyDownStates[kv].Value.IsDownOrLockedDown())
                && Settings.Default.MultiKeySelectionEnabled)
            {
                Log.Debug("No keys which prevents text capture is down - returing setting MultiKeySelectionIsOn to true.");

                KeyDownStates[KeyValues.MultiKeySelectionKey].Value = Enums.KeyDownStates.LockedDown;
                if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.MultiKeySelectionKey);
                turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased = false;
            }
        }

        #endregion
    }
}
