using System;
using System.Linq;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using Prism.Mvvm;

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
            keyStateService.KeyDownStates[KeyValues.LeftShiftKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseLeftDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseMiddleDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseRightDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.SleepKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                keyStateService.KeyDownStates[kv].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged()));

            KeyValues.CombiningKeys.ForEach(kv =>
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

                //Key is publish only, but we are not publishing (simulating key strokes)
                if (!keyStateService.SimulateKeyStrokes
                    && KeyValues.PublishOnlyKeys.Contains(keyValue))
                {
                    return false;
                }
                
                //Key is MultiKeySelection, but a key which prevents text capture is down or locked down
                if (keyValue == KeyValues.MultiKeySelectionIsOnKey
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
                if (keyValue == KeyValues.MultiKeySelectionIsOnKey
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

                //Greek specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.GreekGreece)
                {
                    //Acute accent: Άά Έέ Ήή Ίί Όό Ύύ Ώώ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown()
                        && !keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || (keyValue == KeyValues.CombiningDiaeresisOrUmlautKey 
                                && !keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value.IsDownOrLockedDown()) //The acute accent can be combined with a diaeresis on lower case letters only (shift must be is up)
                            || keyValue == new KeyValue("α")
                            || keyValue == new KeyValue("ε")
                            || keyValue == new KeyValue("η")
                            || keyValue == new KeyValue("ι")
                            || keyValue == new KeyValue("ο")
                            || keyValue == new KeyValue("υ")
                            || keyValue == new KeyValue("ω")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Acute accent + diaeresis: ΐ ΰ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown()
                        && keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown()
                        && !keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value.IsDownOrLockedDown()) //These two diacritics can only be combined with lowercase letters
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == KeyValues.CombiningDiaeresisOrUmlautKey//Allow the diaeresis to be manually released
                            || keyValue == new KeyValue("ι")
                            || keyValue == new KeyValue("υ");
                    }

                    //Diaeresis: Ϊϊ Ϋϋ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown()
                        && !keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningDiaeresisOrUmlautKey//Allow the diaeresis to be manually released
                            || (keyValue == KeyValues.CombiningAcuteAccentKey
                                && !keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value.IsDownOrLockedDown()) //The diaeresis can be combined with an acute accent on lower case letters only (shift must be is up)
                            || keyValue == new KeyValue("ι")
                            || keyValue == new KeyValue("υ")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //French specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.FrenchFrance)
                {
                    //Acute accent: Éé
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("e")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Grave accent: Àà Èè Ùù
                    if (keyStateService.KeyDownStates[KeyValues.CombiningGraveAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningGraveAccentKey //Allow the grave accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Circumflex: Ââ Êê Îî Ôô Ûû
                    if (keyStateService.KeyDownStates[KeyValues.CombiningCircumflexKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningCircumflexKey //Allow the circumflex to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Diaeresis: Ëë Ïï Üü Ÿÿ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningDiaeresisOrUmlautKey //Allow the diaeresis to be manually released
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("u")
                            || keyValue == new KeyValue("y")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Cedilla: Çç
                    if (keyStateService.KeyDownStates[KeyValues.CombiningCedillaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningCedillaKey //Allow the cedilla to be manually released
                            || keyValue == new KeyValue("c")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //Dutch specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.DutchBelgium
                    || Settings.Default.KeyboardAndDictionaryLanguage == Languages.DutchNetherlands)
                {
                    //Acute accent: Áá éÉ íÍ óÓ úÚ    
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Grave accent: Àà Èè
                    if (keyStateService.KeyDownStates[KeyValues.CombiningGraveAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningGraveAccentKey //Allow the grave accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Diaeresis: Ëë Ïï Öö Üü
                    if (keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningDiaeresisOrUmlautKey //Allow the diaeresis to be manually released
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //German specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.GermanGermany)
                {
                    //Diaeresis: Ää Öö Üü
                    if (keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningDiaeresisOrUmlautKey //Allow the diaeresis to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //Italian specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.ItalianItaly)
                {
                    //Acute accent: éÉ óÓ úÚ    
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Grave accent: Àà Èè Ìì Òò Ùù
                    if (keyStateService.KeyDownStates[KeyValues.CombiningGraveAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningGraveAccentKey //Allow the grave accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //Russian specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.RussianRussia)
                {
                    //Acute Accent: Аа Ээ Ыы Уу Оо Яя Ее Юю Ии
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("а")
                            || keyValue == new KeyValue("э")
                            || keyValue == new KeyValue("ы")
                            || keyValue == new KeyValue("у")
                            || keyValue == new KeyValue("о")
                            || keyValue == new KeyValue("я")
                            || keyValue == new KeyValue("е")
                            || keyValue == new KeyValue("ю")
                            || keyValue == new KeyValue("и")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //Spanish specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.SpanishSpain)
                {
                    //Acute accent: Áá éÉ íÍ óÓ úÚ    
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                    //Diaeresis: Üü
                    if (keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningDiaeresisOrUmlautKey //Allow the diaeresis to be manually released
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }
				
                //Turkish specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.TurkishTurkey)
                {
                    //Circumflex: Ââ Îî Ûû
                    if (keyStateService.KeyDownStates[KeyValues.CombiningCircumflexKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
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

        private void NotifyStateChanged()
        {
            OnPropertyChanged(Binding.IndexerName);
        }

        #endregion
    }
}
