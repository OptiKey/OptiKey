using System;
using System.Linq;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Services
{
    public class KeyboardService : BindableBase, IKeyboardService
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly NotifyingConcurrentDictionary<KeyValue, double> keySelectionProgress;
        private readonly NotifyingConcurrentDictionary<KeyValue, KeyDownStates> keyDownStates;
        private readonly KeyEnabledStates keyEnabledStates;

        private bool turnOnMultiKeySelectionWhenKeysWhichPreventTextCaptureAreReleased;

        public NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get { return keySelectionProgress; } }
        public NotifyingConcurrentDictionary<KeyValue, KeyDownStates> KeyDownStates { get { return keyDownStates; } }
        public KeyEnabledStates KeyEnabledStates { get { return keyEnabledStates; } }

        public KeyboardService(ISuggestionService suggestionService, ICapturingStateManager capturingStateManager)
        {
            keySelectionProgress = new NotifyingConcurrentDictionary<KeyValue, double>();
            keyDownStates = new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>();
            keyEnabledStates = new KeyEnabledStates(this, suggestionService, capturingStateManager);

            InitialiseKeyDownStates();
            AddKeyDownStatesChangeHandlers();
        }

        private void InitialiseKeyDownStates()
        {
            Log.Debug("Initialising KeyDownStates.");

            KeyDownStates[KeyValues.PublishKey].Value =
                Settings.Default.PublishingKeys ? Enums.KeyDownStates.LockedDown : Enums.KeyDownStates.Up;

            KeyDownStates[KeyValues.MultiKeySelectionEnabledKey].Value =
                Settings.Default.MultiKeySelectionEnabled ? Enums.KeyDownStates.LockedDown : Enums.KeyDownStates.Up;
        }

        private void AddKeyDownStatesChangeHandlers()
        {
            Log.Debug("Adding KeyDownStates change handlers.");

            KeyDownStates[KeyValues.PublishKey].OnPropertyChanges(s => s.Value).Subscribe(value =>
            {
                Settings.Default.PublishingKeys = KeyDownStates[KeyValues.PublishKey].Value.IsDownOrLockedDown();
                ReleasePublishOnlyKeysIfNotPublishing();
            });

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

            if (!KeyDownStates[KeyValues.PublishKey].Value.IsDownOrLockedDown())
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
    }
}
