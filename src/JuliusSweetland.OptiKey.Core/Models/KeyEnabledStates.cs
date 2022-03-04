// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

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
        private readonly List<Tuple<KeyValue, KeyValue>> keyFamily;

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
            this.keyFamily = keyStateService.KeyFamily;

            suggestionService.OnPropertyChanges(ss => ss.Suggestions).Subscribe(_ => NotifyStateChanged());
            suggestionService.OnPropertyChanges(ss => ss.SuggestionsPage).Subscribe(_ => NotifyStateChanged());
            suggestionService.OnPropertyChanges(ss => ss.SuggestionsPerPage).Subscribe(_ => NotifyStateChanged());

            keyStateService.OnPropertyChanges(kss => kss.SimulateKeyStrokes).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.LeftShiftKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseLeftDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseMiddleDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MouseRightDownUpKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyStateService.KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
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
                // Key has no payload
                if (keyValue == null || !keyValue.HasContent())
                {
                    return false;
                }

                //Key is not Sleep, but we are sleeping
                //KeyFamily is the collection of parent KeyValues (Item1) and the KeyValues of child commands (Item2) 
                //that could be locked down when the parent key is triggered. 
                //Do not disable any parent key that has a child Sleep command
                if (keyStateService.KeyDownStates[KeyValues.SleepKey].Value.IsDownOrLockedDown()
                    && keyValue != KeyValues.SleepKey
                    && !keyFamily.Exists(x => x.Item1 == keyValue && x.Item2 == KeyValues.SleepKey))
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

                //Key is Shift and ForceCapsLock setting is true
                if (keyValue == KeyValues.LeftShiftKey
                    && Settings.Default.ForceCapsLock)
                {
                    return false;
                }

                //Key is Calibrate, but calibrate service is not available
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
                if (Settings.Default.MainWindowState == WindowStates.Docked
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

                //Multi-key capture is disabled explicitly in the settings
                if (keyValue == KeyValues.MultiKeySelectionIsOnKey
                    && !Settings.Default.MultiKeySelectionEnabled)
                {
                    return false;
                }

                //Multi-key capture is disabled because the current language does not support the concept
                if (keyValue == KeyValues.MultiKeySelectionIsOnKey
                    && new[] { Languages.KoreanKorea }.Contains(Settings.Default.KeyboardAndDictionaryLanguage))
                {
                    return false;
                }

                //Key is not a letter, but we're capturing a multi-keyValue selection (which must be ended by selecting a letter)
                if (capturingStateManager.CapturingMultiKeySelection
                    && !KeyValues.MultiKeySelectionKeys.Contains(keyValue))
                {
                    return false;
                }

                // Multi-key is down/locked down - we shouldn't allow combining keys anymore
                if (KeyValues.KeysDisabledWithMultiKeysSelectionIsOn.Contains(keyValue)
                    && keyStateService.KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value.IsDownOrLockedDown())
                {
                    return false;
                }

                //Catalan specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.CatalanSpain)
                {
                    //Acute accent: Éé, Íí, Óó, Úú    
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                    //Grave accent: Àà, Èè, Òò
                    if (keyStateService.KeyDownStates[KeyValues.CombiningGraveAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningGraveAccentKey //Allow the grave accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("o")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                    //Diaeresis: Ïï, Üü
                    if (keyStateService.KeyDownStates[KeyValues.CombiningDiaeresisOrUmlautKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningDiaeresisOrUmlautKey //Allow the diaeresis to be manually released
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("u")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                    //Cedilla: çÇ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningCedillaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningCedillaKey //Allow the cedilla to be manually released
                            || keyValue == new KeyValue("c")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //Czech specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.CzechCzechRepublic)
                {
                    //Acute accent: áÁ éÉ íÍ óÓ úÚ ýÝ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                               || keyValue == new KeyValue("a")
                               || keyValue == new KeyValue("e")
                               || keyValue == new KeyValue("i")
                               || keyValue == new KeyValue("o")
                               || keyValue == new KeyValue("u")
                               || keyValue == new KeyValue("y")
                               || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Caron: čČ ďĎ ěĚ ňŇ řŘ šŠ ťŤ žŽ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningCaronOrHacekKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningCaronOrHacekKey //Allow the caron to be manually released
                        || keyValue == new KeyValue("c")
                        || keyValue == new KeyValue("d")
                        || keyValue == new KeyValue("e")
                        || keyValue == new KeyValue("n")
                        || keyValue == new KeyValue("r")
                        || keyValue == new KeyValue("s")
                        || keyValue == new KeyValue("t")
                        || keyValue == new KeyValue("z")
                        || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
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
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.FrenchFrance
                    || Settings.Default.KeyboardAndDictionaryLanguage == Languages.FrenchCanada)
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

                //Persian specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.PersianIran)
                {
                    //Fathatan
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicFathatanKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicFathatanKey //Allow the Fathatan to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ا");
                    }

                    //Dammatan
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicDammatanKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicDammatanKey //Allow the Dammatan to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ا");
                    }

                    //Kasratan
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicKasratanKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicKasratanKey //Allow the Kasratan to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ا");
                    }
                    
                    //Fatha
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicFathaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicFathaKey //Allow the Fatha to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ه")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("پ");
                    }

                    //Damma
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicDammaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicDammaKey //Allow the Damma to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ه")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("پ");
                    }

                    //Kasra
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicKasraKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicKasraKey //Allow the Kasra to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ه")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("پ");
                    }

                    //Shadda
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicShaddaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicShaddaKey //Allow the Shadda to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ه")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("پ");
                    }
                }

                //Polish specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.PolishPoland)
                {
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        //Acute accent: ćĆ ńŃ óÓ śŚ źŹ
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("c")
                            || keyValue == new KeyValue("n")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("s")
                            || keyValue == new KeyValue("z")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    if (keyStateService.KeyDownStates[KeyValues.CombiningOgonekOrNosineKey].Value.IsDownOrLockedDown())
                    {
                        //Ogonek accent: ąĄ ęĘ
                        return keyValue == KeyValues.CombiningOgonekOrNosineKey //Allow the ogonek accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    if (keyStateService.KeyDownStates[KeyValues.CombiningDotAboveKey].Value.IsDownOrLockedDown())
                    {
                        //Dot above accent: żŻ
                        return keyValue == KeyValues.CombiningDotAboveKey //Allow the dot above accent to be manually released
                            || keyValue == new KeyValue("z")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //Portuguese specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.PortuguesePortugal)
                {
                    //Acute accent: áÁ éÉ íÍ óÓ úÚ
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

                    //Grave accent: Àà
                    if (keyStateService.KeyDownStates[KeyValues.CombiningGraveAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningGraveAccentKey //Allow the grave accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Circumflex: Ââ Êê Ôô
                    if (keyStateService.KeyDownStates[KeyValues.CombiningCircumflexKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningCircumflexKey //Allow the circumflex to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("o")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Tilde: ãÃõÕ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningTildeKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningTildeKey //Allow the circumflex to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("o")
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

                //Slovak specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.SlovakSlovakia)
                {
                    //Acute accent: Áá éÉ íÍ ĺĹ óÓ ŕŔ úÚ ýÝ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                            || keyValue == new KeyValue("a")
                            || keyValue == new KeyValue("e")
                            || keyValue == new KeyValue("i")
                            || keyValue == new KeyValue("l")
                            || keyValue == new KeyValue("o")
                            || keyValue == new KeyValue("r")
                            || keyValue == new KeyValue("u")
                            || keyValue == new KeyValue("y")
                            || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }

                    //Caron: čČ ďĎ ľĽ ňŇ šŠ ťŤ žŽ
                    if (keyStateService.KeyDownStates[KeyValues.CombiningCaronOrHacekKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningCaronOrHacekKey //Allow the caron to be manually released
                        || keyValue == new KeyValue("c")
                        || keyValue == new KeyValue("d")
                        || keyValue == new KeyValue("l")
                        || keyValue == new KeyValue("n")
                        || keyValue == new KeyValue("s")
                        || keyValue == new KeyValue("t")
                        || keyValue == new KeyValue("z")
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

                //Ukrainian specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.UkrainianUkraine)
                {
                    //Acute accent: Аа Ее Єє Ии Іі Її Оо Уу Юю Яя
                    if (keyStateService.KeyDownStates[KeyValues.CombiningAcuteAccentKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningAcuteAccentKey //Allow the acute accent to be manually released
                               || keyValue == new KeyValue("а")
                               || keyValue == new KeyValue("е")
                               || keyValue == new KeyValue("є")
                               || keyValue == new KeyValue("и")
                               || keyValue == new KeyValue("і")
                               || keyValue == new KeyValue("ї")
                               || keyValue == new KeyValue("о")
                               || keyValue == new KeyValue("у")
                               || keyValue == new KeyValue("ю")
                               || keyValue == new KeyValue("я")
                               || keyValue == KeyValues.LeftShiftKey; //Allow shift to be toggled on/off
                    }
                }

                //Urdu specific rules
                if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.UrduPakistan)
                {
                    //Madd (Maddah in Arabic)
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicMaddahAboveKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicMaddahAboveKey //Allow the Madd to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ا"); //Alef
                    }

                    //Superscript Alef
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicLetterSuperscriptAlefKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicLetterSuperscriptAlefKey //Allow the Superscript Alef to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.CombiningArabicShaddaKey
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("ی");
                    }

                    //Hamza Above
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicHamzaAboveKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicHamzaAboveKey //Allow the Hamza Above to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ہ")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("و");
                    }

                    //Shadd (Shadda in Arabic)
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicShaddaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicShaddaKey //Allow the Shadd to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.CombiningArabicLetterSuperscriptAlefKey
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("ٹ")
                               || keyValue == new KeyValue("پ")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ہ")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("ڈ")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ۃ")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ڑ")
                               || keyValue == new KeyValue("ں")
                               || keyValue == new KeyValue("ء")
                               || keyValue == new KeyValue("آ")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("اً");
                    }

                    //Fathatan
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicFathatanKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicFathatanKey //Allow the Fathatan to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("ٹ")
                               || keyValue == new KeyValue("پ")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ہ")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("ڈ")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ۃ")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ڑ")
                               || keyValue == new KeyValue("ں")
                               || keyValue == new KeyValue("ء")
                               || keyValue == new KeyValue("آ")
                               || keyValue == new KeyValue("گ");
                    }

                    //Kasra
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicKasraKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicKasraKey //Allow the Kasra to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("ٹ")
                               || keyValue == new KeyValue("پ")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ہ")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("ڈ")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ۃ")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ڑ")
                               || keyValue == new KeyValue("ں")
                               || keyValue == new KeyValue("ء")
                               || keyValue == new KeyValue("آ")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("اً");
                    }

                    //Damma
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicDammaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicDammaKey //Allow the Damma to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("ٹ")
                               || keyValue == new KeyValue("پ")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ہ")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("ڈ")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ۃ")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ڑ")
                               || keyValue == new KeyValue("ں")
                               || keyValue == new KeyValue("ء")
                               || keyValue == new KeyValue("آ")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("اً");
                    }

                    //Fatha
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicFathaKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicFathaKey //Allow the Fatha to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("ٹ")
                               || keyValue == new KeyValue("پ")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ہ")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("ڈ")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ۃ")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ڑ")
                               || keyValue == new KeyValue("ں")
                               || keyValue == new KeyValue("ء")
                               || keyValue == new KeyValue("آ")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("اً");
                    }

                    //Small High Ligature Sad With Lam With Alef Maksura
                    if (keyStateService.KeyDownStates[KeyValues.CombiningArabicSmallHighLigatureSadWithLamWithAlefMaksuraKey].Value.IsDownOrLockedDown())
                    {
                        return keyValue == KeyValues.CombiningArabicSmallHighLigatureSadWithLamWithAlefMaksuraKey //Allow the Small High Ligature Sad With Lam With Alef Maksura to be manually released
                               || keyValue == KeyValues.Alpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.Alpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha1KeyboardKey //Allow movement between the keyboards
                               || keyValue == KeyValues.ConversationAlpha2KeyboardKey //Allow movement between the keyboards
                               || keyValue == new KeyValue("ط")
                               || keyValue == new KeyValue("ص")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("د")
                               || keyValue == new KeyValue("ٹ")
                               || keyValue == new KeyValue("پ")
                               || keyValue == new KeyValue("ت")
                               || keyValue == new KeyValue("ب")
                               || keyValue == new KeyValue("ج")
                               || keyValue == new KeyValue("ح")
                               || keyValue == new KeyValue("م")
                               || keyValue == new KeyValue("و")
                               || keyValue == new KeyValue("ر")
                               || keyValue == new KeyValue("ن")
                               || keyValue == new KeyValue("ل")
                               || keyValue == new KeyValue("ہ")
                               || keyValue == new KeyValue("ا")
                               || keyValue == new KeyValue("ک")
                               || keyValue == new KeyValue("ی")
                               || keyValue == new KeyValue("ق")
                               || keyValue == new KeyValue("ف")
                               || keyValue == new KeyValue("ے")
                               || keyValue == new KeyValue("س")
                               || keyValue == new KeyValue("ش")
                               || keyValue == new KeyValue("غ")
                               || keyValue == new KeyValue("ع")
                               || keyValue == new KeyValue("ظ")
                               || keyValue == new KeyValue("ض")
                               || keyValue == new KeyValue("ذ")
                               || keyValue == new KeyValue("ڈ")
                               || keyValue == new KeyValue("ث")
                               || keyValue == new KeyValue("ۃ")
                               || keyValue == new KeyValue("ھ")
                               || keyValue == new KeyValue("چ")
                               || keyValue == new KeyValue("خ")
                               || keyValue == new KeyValue("ژ")
                               || keyValue == new KeyValue("ز")
                               || keyValue == new KeyValue("ڑ")
                               || keyValue == new KeyValue("ں")
                               || keyValue == new KeyValue("ء")
                               || keyValue == new KeyValue("آ")
                               || keyValue == new KeyValue("گ")
                               || keyValue == new KeyValue("اً");
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
