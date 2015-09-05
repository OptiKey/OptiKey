using System;
using System.Linq;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyEnabledStates : BindableBase
    {
        #region Fields

        private readonly IKeyStateService keyStateService;
        private readonly ISuggestionStateService suggestionService;
        private readonly ICapturingStateManager capturingStateManager;
        private readonly ILastMouseActionStateManager lastMouseActionStateManager;
        private readonly ICalibrationService calibrationService;

        #endregion

        #region Ctor

        public KeyEnabledStates(
            IKeyStateService keyStateService, 
            ISuggestionStateService suggestionService,
            ICapturingStateManager capturingStateManager,
            ILastMouseActionStateManager lastMouseActionStateManager,
            ICalibrationService calibrationService)
        {
            this.keyStateService = keyStateService;
            this.suggestionService = suggestionService;
            this.capturingStateManager = capturingStateManager;
            this.lastMouseActionStateManager = lastMouseActionStateManager;
            this.calibrationService = calibrationService;

            suggestionService.OnPropertyChanges(ss => ss.Suggestions).Subscribe(_ => NotifyStateChanged());
            suggestionService.OnPropertyChanges(ss => ss.SuggestionsPage).Subscribe(_ => NotifyStateChanged());
            suggestionService.OnPropertyChanges(ss => ss.SuggestionsPerPage).Subscribe(_ => NotifyStateChanged());

            keyStateService.OnPropertyChanges(kss => kss.SimulateKeyStrokes).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseLeftDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseMiddleDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseRightDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.SleepKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                keyStateService.KeyDownStates[kv].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged()));

            capturingStateManager.OnPropertyChanges(csm => csm.CapturingMultiKeySelection).Subscribe(_ => NotifyStateChanged());

            lastMouseActionStateManager.OnPropertyChanges(lmasm => lmasm.LastMouseActionExists).Subscribe(_ => NotifyStateChanged());

            Settings.Default.OnPropertyChanges(s => s.MultiKeySelectionEnabled).Subscribe(_ => NotifyStateChanged());
            Settings.Default.OnPropertyChanges(s => s.MainWindowState).Subscribe(_ => NotifyStateChanged());
            Settings.Default.OnPropertyChanges(s => s.MainWindowDockPosition).Subscribe(_ => NotifyStateChanged());
        }

        #endregion

        #region Properties

        public bool this[KeyValue keyValue]
        {
            get
            {
                //Key is not Sleep, but we are sleeping
                if (keyStateService.KeyDownStates[KeyValues.SleepKey].Value.IsDownOrLockedDown()
                    && keyValue != KeyValues.SleepKey)
                {
                    return false;
                }

                //Key is publish only, but we are not publishing
                if (!keyStateService.SimulateKeyStrokes
                    && KeyValues.PublishOnlyKeys.Contains(keyValue))
                {
                    return false;
                }
                
                //Key is MultiKeySelection, but a key which prevents text capture is down or locked down
                if (keyValue == KeyValues.MultiKeySelectionKey
                    && KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                        keyStateService.KeyDownStates[kv].Value.IsDownOrLockedDown()))
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

                //Expand/Collapse dock when not docked
                if ((keyValue == KeyValues.ExpandDockKey || keyValue == KeyValues.CollapseDockKey)
                    && Settings.Default.MainWindowState != WindowStates.Docked)
                {
                    return false;
                }

                //Move & Resize keys when docked
                if(Settings.Default.MainWindowState == WindowStates.Docked
                    && ((Settings.Default.MainWindowDockPosition == DockEdges.Top &&
                            (keyValue == KeyValues.MoveToTopBoundaryKey
                            || keyValue == KeyValues.MoveToTopKey
                            || keyValue == KeyValues.MoveToTopAndLeftKey
                            || keyValue == KeyValues.MoveToTopAndRightKey
                            || keyValue == KeyValues.MoveToLeftKey
                            || keyValue == KeyValues.MoveToRightKey
                            || keyValue == KeyValues.ExpandToTopKey
                            || keyValue == KeyValues.ExpandToTopAndLeftKey
                            || keyValue == KeyValues.ExpandToTopAndRightKey
                            || keyValue == KeyValues.ExpandToLeftKey
                            || keyValue == KeyValues.ExpandToRightKey
                            || keyValue == KeyValues.ExpandToBottomAndLeftKey
                            || keyValue == KeyValues.ExpandToBottomAndRightKey
                            || keyValue == KeyValues.ShrinkFromTopKey
                            || keyValue == KeyValues.ShrinkFromTopAndRightKey
                            || keyValue == KeyValues.ShrinkFromTopAndLeftKey
                            || keyValue == KeyValues.ShrinkFromLeftKey
                            || keyValue == KeyValues.ShrinkFromRightKey
                            || keyValue == KeyValues.ShrinkFromBottomAndLeftKey
                            || keyValue == KeyValues.ShrinkFromBottomAndRightKey))
                        || (Settings.Default.MainWindowDockPosition == DockEdges.Bottom &&
                            (keyValue == KeyValues.MoveToBottomBoundaryKey
                            || keyValue == KeyValues.MoveToBottomKey
                            || keyValue == KeyValues.MoveToBottomAndLeftKey
                            || keyValue == KeyValues.MoveToBottomAndRightKey
                            || keyValue == KeyValues.MoveToLeftKey
                            || keyValue == KeyValues.MoveToRightKey
                            || keyValue == KeyValues.ExpandToBottomKey
                            || keyValue == KeyValues.ExpandToBottomAndLeftKey
                            || keyValue == KeyValues.ExpandToBottomAndRightKey
                            || keyValue == KeyValues.ExpandToLeftKey
                            || keyValue == KeyValues.ExpandToRightKey
                            || keyValue == KeyValues.ExpandToTopAndLeftKey
                            || keyValue == KeyValues.ExpandToTopAndRightKey
                            || keyValue == KeyValues.ShrinkFromBottomKey
                            || keyValue == KeyValues.ShrinkFromBottomAndRightKey
                            || keyValue == KeyValues.ShrinkFromBottomAndLeftKey
                            || keyValue == KeyValues.ShrinkFromLeftKey
                            || keyValue == KeyValues.ShrinkFromRightKey
                            || keyValue == KeyValues.ShrinkFromTopAndLeftKey
                            || keyValue == KeyValues.ShrinkFromTopAndRightKey))
                        || (Settings.Default.MainWindowDockPosition == DockEdges.Left &&
                            (keyValue == KeyValues.MoveToLeftBoundaryKey
                            || keyValue == KeyValues.MoveToLeftKey
                            || keyValue == KeyValues.MoveToBottomAndLeftKey
                            || keyValue == KeyValues.MoveToTopAndLeftKey
                            || keyValue == KeyValues.MoveToTopKey
                            || keyValue == KeyValues.MoveToBottomKey
                            || keyValue == KeyValues.ExpandToLeftKey
                            || keyValue == KeyValues.ExpandToBottomAndLeftKey
                            || keyValue == KeyValues.ExpandToTopAndLeftKey
                            || keyValue == KeyValues.ExpandToTopKey
                            || keyValue == KeyValues.ExpandToBottomKey
                            || keyValue == KeyValues.ExpandToTopAndRightKey
                            || keyValue == KeyValues.ExpandToBottomAndRightKey
                            || keyValue == KeyValues.ShrinkFromLeftKey
                            || keyValue == KeyValues.ShrinkFromBottomAndLeftKey
                            || keyValue == KeyValues.ShrinkFromTopAndLeftKey
                            || keyValue == KeyValues.ShrinkFromTopKey
                            || keyValue == KeyValues.ShrinkFromBottomKey
                            || keyValue == KeyValues.ShrinkFromTopAndRightKey
                            || keyValue == KeyValues.ShrinkFromBottomAndRightKey))
                        || (Settings.Default.MainWindowDockPosition == DockEdges.Right &&
                            (keyValue == KeyValues.MoveToRightBoundaryKey
                            || keyValue == KeyValues.MoveToRightKey
                            || keyValue == KeyValues.MoveToBottomAndRightKey
                            || keyValue == KeyValues.MoveToTopAndRightKey
                            || keyValue == KeyValues.MoveToTopKey
                            || keyValue == KeyValues.MoveToBottomKey
                            || keyValue == KeyValues.ExpandToRightKey
                            || keyValue == KeyValues.ExpandToBottomAndRightKey
                            || keyValue == KeyValues.ExpandToTopAndRightKey
                            || keyValue == KeyValues.ExpandToTopKey
                            || keyValue == KeyValues.ExpandToBottomKey
                            || keyValue == KeyValues.ExpandToTopAndLeftKey
                            || keyValue == KeyValues.ExpandToBottomAndLeftKey
                            || keyValue == KeyValues.ShrinkFromRightKey
                            || keyValue == KeyValues.ShrinkFromBottomAndRightKey
                            || keyValue == KeyValues.ShrinkFromTopAndRightKey
                            || keyValue == KeyValues.ShrinkFromTopKey
                            || keyValue == KeyValues.ShrinkFromBottomKey
                            || keyValue == KeyValues.ShrinkFromTopAndLeftKey
                            || keyValue == KeyValues.ShrinkFromBottomAndLeftKey))
                        || keyValue == KeyValues.MoveToBottomAndLeftBoundariesKey
                        || keyValue == KeyValues.MoveToBottomAndRightBoundariesKey
                        || keyValue == KeyValues.MoveToTopAndLeftBoundariesKey
                        || keyValue == KeyValues.MoveToTopAndRightBoundariesKey))
                {
                    return false;
                }

                //Mouse actions involving left button if it is already down
                if ((keyValue == KeyValues.MouseDragKey 
                    || keyValue == KeyValues.MouseLeftClickKey 
                    || keyValue == KeyValues.MouseLeftDoubleClickKey
                    || keyValue == KeyValues.MouseMoveAndLeftClickKey 
                    || keyValue == KeyValues.MouseMoveAndLeftDoubleClickKey)
                        && keyStateService.KeyDownStates[KeyValues.MouseLeftDownUpKey].Value.IsDownOrLockedDown())
                {
                    return false;
                }

                //Mouse actions involving middle button if it is already down
                if ((keyValue == KeyValues.MouseMiddleClickKey
                    || keyValue == KeyValues.MouseMoveAndMiddleClickKey)
                        && keyStateService.KeyDownStates[KeyValues.MouseMiddleDownUpKey].Value.IsDownOrLockedDown())
                {
                    return false;
                }

                //Mouse actions involving right button if it is already down
                if ((keyValue == KeyValues.MouseRightClickKey
                    || keyValue == KeyValues.MouseMoveAndRightClickKey)
                        && keyStateService.KeyDownStates[KeyValues.MouseRightDownUpKey].Value.IsDownOrLockedDown())
                {
                    return false;
                }

                //Multi-key capture is disabled
                if (keyValue == KeyValues.MultiKeySelectionKey
                    && !Settings.Default.MultiKeySelectionEnabled)
                {
                    return false;
                }

                //Key is not a letter, but we're capturing a multi-keyValue selection (which must be ended by selecting a letter)
                if (capturingStateManager.CapturingMultiKeySelection
                    && !KeyValues.MultiKeySelectionKeys.Contains(keyValue))
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
