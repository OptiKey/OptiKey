using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyEnabledStates : BindableBase
    {
        #region Fields

        private readonly IKeyboardService keyboardService;
        private readonly ISuggestionStateService suggestionService;
        private readonly ICapturingStateManager capturingStateManager;
        private readonly ILastMouseActionStateManager lastMouseActionStateManager;
        private readonly ICalibrationService calibrationService;
        private readonly IWindowStateService mainWindowStateService;

        #endregion

        #region Ctor

        public KeyEnabledStates(
            IKeyboardService keyboardService, 
            ISuggestionStateService suggestionService,
            ICapturingStateManager capturingStateManager,
            ILastMouseActionStateManager lastMouseActionStateManager,
            ICalibrationService calibrationService,
            IWindowStateService mainWindowStateService)
        {
            this.keyboardService = keyboardService;
            this.suggestionService = suggestionService;
            this.capturingStateManager = capturingStateManager;
            this.lastMouseActionStateManager = lastMouseActionStateManager;
            this.calibrationService = calibrationService;
            this.mainWindowStateService = mainWindowStateService;

            suggestionService.OnPropertyChanges(ss => ss.Suggestions).Subscribe(_ => NotifyStateChanged());
            suggestionService.OnPropertyChanges(ss => ss.SuggestionsPage).Subscribe(_ => NotifyStateChanged());
            suggestionService.OnPropertyChanges(ss => ss.SuggestionsPerPage).Subscribe(_ => NotifyStateChanged());

            keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyboardService.KeyDownStates[KeyValues.SleepKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                keyboardService.KeyDownStates[kv].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged()));

            capturingStateManager.OnPropertyChanges(csm => csm.CapturingMultiKeySelection).Subscribe(_ => NotifyStateChanged());

            lastMouseActionStateManager.OnPropertyChanges(lmasm => lmasm.LastMouseActionExists).Subscribe(_ => NotifyStateChanged());

            mainWindowStateService.OnPropertyChanges(mwss => mwss.WindowState).Subscribe(_ => NotifyStateChanged());
        }

        #endregion

        #region Properties

        public bool this[KeyValue keyValue]
        {
            get
            {
                //Key is not Sleep, but we are sleeping
                if (keyboardService.KeyDownStates[KeyValues.SleepKey].Value.IsDownOrLockedDown()
                    && keyValue != KeyValues.SleepKey)
                {
                    return false;
                }

                //Key is publish only, but we are not publishing
                if (!keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown()
                    && KeyValues.PublishOnlyKeys.Contains(keyValue))
                {
                    return false;
                }
                
                //Key is MultiKeySelection, but a key which prevents text capture is down or locked down
                if (keyValue == KeyValues.MultiKeySelectionEnabledKey
                    && KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                        keyboardService.KeyDownStates[kv].Value.IsDownOrLockedDown()))
                {
                    return false;
                }

                //Key is Calibrate, but not calibrate service available
                if (keyValue == KeyValues.CalibrateKey
                    && calibrationService == null)
                {
                    return false;
                }

                //Key is Repeat Last Mouse Action, but KeyEnabledStates.RepeatLastMouseActionIsValid is not true
                if (keyValue == KeyValues.RepeatLastMouseActionKey
                    && !lastMouseActionStateManager.LastMouseActionExists)
                {
                    return false;
                }

                //Key is Previous suggestions, but no suggestions, or on page 1
                if (keyValue == KeyValues.PreviousSuggestionsKey
                    && (suggestionService.Suggestions == null
                        || !suggestionService.Suggestions.Any()
                        || suggestionService.SuggestionsPage == 0))
                {
                    return false;
                }

                //Key is Next suggestions but no suggestions, or on last page
                if (keyValue == KeyValues.NextSuggestionsKey
                    && (suggestionService.Suggestions == null
                        || !suggestionService.Suggestions.Any()
                        || suggestionService.Suggestions.Count <= ((suggestionService.SuggestionsPage * suggestionService.SuggestionsPerPage) + suggestionService.SuggestionsPerPage)))
                {
                    return false;
                }

                //Key is Suggestion 1 but no suggestion exist for that index
                if (keyValue == KeyValues.Suggestion1Key
                    && !SuggestionKeyIsValid(0))
                {
                    return false;
                }

                //Key is Suggestion 2 but no suggestion exist for that index
                if (keyValue == KeyValues.Suggestion2Key
                    && !SuggestionKeyIsValid(1))
                {
                    return false;
                }

                //Key is Suggestion 3 but no suggestion exist for that index
                if (keyValue == KeyValues.Suggestion3Key
                    && !SuggestionKeyIsValid(2))
                {
                    return false;
                }

                //Key is Suggestion 4 but no suggestion exist for that index
                if (keyValue == KeyValues.Suggestion4Key
                    && !SuggestionKeyIsValid(3))
                {
                    return false;
                }

                //Key is Suggestion 5 but no suggestion exist for that index
                if (keyValue == KeyValues.Suggestion5Key
                    && !SuggestionKeyIsValid(4))
                {
                    return false;
                }

                //Key is Suggestion 6 but no suggestion exist for that index
                if (keyValue == KeyValues.Suggestion6Key
                    && !SuggestionKeyIsValid(5))
                {
                    return false;
                }
                
                //Key is not a letter, but we're capturing a multi-keyValue selection (which must be ended by selecting a letter)
                if (capturingStateManager.CapturingMultiKeySelection
                    && !KeyValues.MultiKeySelectionKeys.Contains(keyValue))
                {
                    return false;
                }

                //Key is Maximise, but the window is already maximised
                if (keyValue == KeyValues.MaximiseSizeKey
                    && mainWindowStateService.WindowState == WindowState.Maximized)
                {
                    return false;
                }

                //Key is Restore, but the window is already normal
                if (keyValue == KeyValues.RestoreSizeKey
                    && mainWindowStateService.WindowState == WindowState.Normal)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region Private Methods

        private bool SuggestionKeyIsValid(int index)
        {
            return suggestionService.Suggestions != null
                && suggestionService.Suggestions.Any()
                && suggestionService.Suggestions.Count > (suggestionService.SuggestionsPage * suggestionService.SuggestionsPerPage + index);
        }

        #endregion

        #region Notify State Changed

        public void NotifyStateChanged()
        {
            OnPropertyChanged(Binding.IndexerName);
        }

        #endregion
    }
}
