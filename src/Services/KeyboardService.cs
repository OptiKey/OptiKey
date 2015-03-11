using System;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class KeyboardService : BindableBase, IKeyboardService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly NotifyingConcurrentDictionary<KeyValue, double> keySelectionProgress;
        private readonly NotifyingConcurrentDictionary<KeyValue, KeyDownStates> keyDownStates;
        private readonly KeyEnabledStates keyEnabledStates;

        private bool turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased;

        #endregion

        #region Ctor

        public KeyboardService(
            ISuggestionService suggestionService, 
            ICapturingStateManager capturingStateManager, 
            ICalibrationService calibrationService,
            IWindowStateService mainWindowStateService)
        {
            keySelectionProgress = new NotifyingConcurrentDictionary<KeyValue, double>();
            keyDownStates = new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>();
            keyEnabledStates = new KeyEnabledStates(this, suggestionService, capturingStateManager, calibrationService, mainWindowStateService);

            InitialiseKeyDownStates();
            AddKeyboardSetChangeHandlers();
            AddKeyDownStatesChangeHandlers();
        }

        #endregion

        #region Properties

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
                Log.Debug(string.Format("Changing key down state of '{0}' key from UP to DOWN.", keyValue));
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.Down;
            }
            else if (KeyValues.KeysWhichCanBeLockedDown.Contains(keyValue)
                     && !KeyValues.KeysWhichCanBePressedDown.Contains(keyValue)
                     && KeyDownStates[keyValue].Value == Enums.KeyDownStates.Up)
            {
                Log.Debug(string.Format("Changing key down state of '{0}' key from UP to LOCKED DOWN.", keyValue));
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.LockedDown;
            }
            else if (KeyValues.KeysWhichCanBeLockedDown.Contains(keyValue)
                     && KeyDownStates[keyValue].Value == Enums.KeyDownStates.Down)
            {
                Log.Debug(string.Format("Changing key down state of '{0}' key from DOWN to LOCKED DOWN.", keyValue));
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.LockedDown;
            }
            else if (KeyDownStates[keyValue].Value != Enums.KeyDownStates.Up)
            {
                Log.Debug(string.Format("Changing key down state of '{0}' key from {1} to UP.", keyValue,
                    KeyDownStates[keyValue].Value == Enums.KeyDownStates.Down ? "DOWN" : "LOCKED DOWN"));
                KeyDownStates[keyValue].Value = Enums.KeyDownStates.Up;
            }
        }

        #endregion

        #region Private Methods

        private void InitialiseKeyDownStates()
        {
            Log.Debug("Initialising KeyDownStates.");

            KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value =
                Settings.Default.SimulateKeyStrokes ? Enums.KeyDownStates.LockedDown : Enums.KeyDownStates.Up;

            KeyDownStates[KeyValues.MouseMagnifierKey].Value =
                Settings.Default.MouseMagnifier ? Enums.KeyDownStates.LockedDown : Enums.KeyDownStates.Up;

            KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value =
                Settings.Default.MultiKeySelectionEnabled ? Enums.KeyDownStates.LockedDown : Enums.KeyDownStates.Up;
        }

        private void AddKeyboardSetChangeHandlers()
        {
            Log.Debug("Adding KeyboardSet setting change handlers.");

            Settings.Default.OnPropertyChanges(s => s.KeyboardSet).Subscribe(visualMode =>
            {
                if (visualMode == KeyboardsSets.SpeechOnly)
                {
                    KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value = Enums.KeyDownStates.Up;
                    KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value = Enums.KeyDownStates.Up;
                }
            });
        }

        private void AddKeyDownStatesChangeHandlers()
        {
            Log.Debug("Adding KeyDownStates change handlers.");

            KeyDownStates[KeyValues.SimulateKeyStrokesKey].OnPropertyChanges(s => s.Value).Subscribe(value =>
            {
                Settings.Default.SimulateKeyStrokes = KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown();
                ReleasePublishOnlyKeysIfNotPublishing();
            });

            KeyDownStates[KeyValues.MouseMagnifierKey].OnPropertyChanges(s => s.Value).Subscribe(value =>
                Settings.Default.MouseMagnifier = KeyDownStates[KeyValues.MouseMagnifierKey].Value.IsDownOrLockedDown());

            KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].OnPropertyChanges(s => s.Value).Subscribe(value =>
                Settings.Default.MultiKeySelectionEnabled = KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value.IsDownOrLockedDown());

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                KeyDownStates[kv].OnPropertyChanges(s => s.Value).Subscribe(value => CalculateMultiKeySelectionSupported()));

            ReleasePublishOnlyKeysIfNotPublishing();
            CalculateMultiKeySelectionSupported();
        }

        private void ReleasePublishOnlyKeysIfNotPublishing()
        {
            Log.Debug("ReleasePublishOnlyKeysIfNotPublishing called.");

            if (!KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown())
            {
                foreach (var keyValue in KeyDownStates.Keys)
                {
                    if (KeyValues.PublishOnlyKeys.Contains(keyValue)
                        && KeyDownStates[keyValue].Value.IsDownOrLockedDown())
                    {
                        Log.Debug(string.Format("Releasing '{0}' as we are not publishing.", keyValue));
                        KeyDownStates[keyValue].Value = Enums.KeyDownStates.Up;
                    }
                }
            }
        }

        private void CalculateMultiKeySelectionSupported()
        {
            Log.Debug("CalculateMultiKeySelectionSupported called.");

            if (KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value.IsDownOrLockedDown()
                && KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.Debug("A key which prevents text capture is down - toggling MultiKeySelectionEnabled to false.");

                KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value = Enums.KeyDownStates.Up;
                turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased = true;
            }
            else if (turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased
                && !KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv => KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.Debug("No keys which prevents text capture is down - returing setting MultiKeySelectionEnabled to true.");

                KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value = Enums.KeyDownStates.LockedDown;
                turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased = false;
            }
        }

        #endregion
    }
}
