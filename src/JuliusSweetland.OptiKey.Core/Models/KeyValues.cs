// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JuliusSweetland.OptiKey.Models
{
    public static class KeyValues
    {
        public static readonly KeyValue AddToDictionaryKey = new KeyValue(FunctionKeys.AddToDictionary);
        public static readonly KeyValue Alpha1KeyboardKey = new KeyValue(FunctionKeys.Alpha1Keyboard);
        public static readonly KeyValue Alpha2KeyboardKey = new KeyValue(FunctionKeys.Alpha2Keyboard);
        public static readonly KeyValue ArrowDownKey = new KeyValue(FunctionKeys.ArrowDown);
        public static readonly KeyValue ArrowLeftKey = new KeyValue(FunctionKeys.ArrowLeft);
        public static readonly KeyValue ArrowRightKey = new KeyValue(FunctionKeys.ArrowRight);
        public static readonly KeyValue ArrowUpKey = new KeyValue(FunctionKeys.ArrowUp);
        public static readonly KeyValue AttentionKey = new KeyValue(FunctionKeys.Attention);
        public static readonly KeyValue BackFromKeyboardKey = new KeyValue(FunctionKeys.BackFromKeyboard);
        public static readonly KeyValue BackManyKey = new KeyValue(FunctionKeys.BackMany);
        public static readonly KeyValue BackOneKey = new KeyValue(FunctionKeys.BackOne);
        public static readonly KeyValue BreakKey = new KeyValue(FunctionKeys.Break);
        public static readonly KeyValue BrowserBackKey = new KeyValue(FunctionKeys.BrowserBack);
        public static readonly KeyValue BrowserForwardKey = new KeyValue(FunctionKeys.BrowserForward);
        public static readonly KeyValue BrowserHomeKey = new KeyValue(FunctionKeys.BrowserHome);
        public static readonly KeyValue BrowserSearchKey = new KeyValue(FunctionKeys.BrowserSearch);
        public static readonly KeyValue CalibrateKey = new KeyValue(FunctionKeys.Calibrate);
        public static readonly KeyValue CatalanSpainKey = new KeyValue(FunctionKeys.CatalanSpain);
        public static readonly KeyValue ClearScratchpadKey = new KeyValue(FunctionKeys.ClearScratchpad);
        public static readonly KeyValue CollapseDockKey = new KeyValue(FunctionKeys.CollapseDock);
        public static readonly KeyValue CombiningAcuteAccentKey = new KeyValue("\x0301");
        public static readonly KeyValue CombiningArabicKasraKey = new KeyValue("\x0650");
        public static readonly KeyValue CombiningArabicFathatanKey = new KeyValue("\x064B");
        public static readonly KeyValue CombiningArabicDammatanKey = new KeyValue("\x064C");
        public static readonly KeyValue CombiningArabicKasratanKey = new KeyValue("\x064D");
        public static readonly KeyValue CombiningArabicDammaKey = new KeyValue("\x064F");
        public static readonly KeyValue CombiningArabicLetterSuperscriptAlefKey = new KeyValue("\x0670");
        public static readonly KeyValue CombiningArabicFathaKey = new KeyValue("\x064E");
        public static readonly KeyValue CombiningArabicHamzaAboveKey = new KeyValue("\x0654");
        public static readonly KeyValue CombiningArabicShaddaKey = new KeyValue("\x0651");
        public static readonly KeyValue CombiningArabicMaddahAboveKey = new KeyValue("\x0653");
        public static readonly KeyValue CombiningArabicSmallHighLigatureSadWithLamWithAlefMaksuraKey = new KeyValue("\x06d6");
        public static readonly KeyValue CombiningBreveKey = new KeyValue("\x0306");
        public static readonly KeyValue CombiningCaronOrHacekKey = new KeyValue("\x030C");
        public static readonly KeyValue CombiningCedillaKey = new KeyValue("\x0327");
        public static readonly KeyValue CombiningCircumflexKey = new KeyValue("\x0302");
        public static readonly KeyValue CombiningCommaAboveOrSmoothBreathingKey = new KeyValue("\x0313");
        public static readonly KeyValue CombiningCyrillicPsiliPneumataOrSmoothBreathingKey = new KeyValue("\x0486");

        //Hindi characters - these are NOT actually combining characters, but spacing characters which appear to combine - left here in case I'm wrong and I need them back!
        //public static readonly KeyValue CombiningDevanagariSignAnusvara = new KeyValue("\x0902");
        //public static readonly KeyValue CombiningDevanagariSignCandrabindu = new KeyValue("\x0901"); //Combining version in 301 - why?
        //public static readonly KeyValue CombiningDevanagariSignNukta = new KeyValue("\x093C");
        //public static readonly KeyValue CombiningDevanagariSignVirama = new KeyValue("\x094D");
        //public static readonly KeyValue CombiningDevanagariSignVisarga = new KeyValue("\x0903");
        //public static readonly KeyValue CombiningDevanagariVowelSignAA = new KeyValue("\x093E");
        //public static readonly KeyValue CombiningDevanagariVowelSignAI = new KeyValue("\x0948");
        //public static readonly KeyValue CombiningDevanagariVowelSignAU = new KeyValue("\x094C");
        //public static readonly KeyValue CombiningDevanagariVowelSignCandraE = new KeyValue("\x0945");
        //public static readonly KeyValue CombiningDevanagariVowelSignCandraO = new KeyValue("\x0949");
        //public static readonly KeyValue CombiningDevanagariVowelSignE = new KeyValue("\x0947");
        //public static readonly KeyValue CombiningDevanagariVowelSignI = new KeyValue("\x093F");
        //public static readonly KeyValue CombiningDevanagariVowelSignII = new KeyValue("\x0940");
        //public static readonly KeyValue CombiningDevanagariVowelSignO = new KeyValue("\x094B");
        //public static readonly KeyValue CombiningDevanagariVowelSignU = new KeyValue("\x0941");
        //public static readonly KeyValue CombiningDevanagariVowelSignUU = new KeyValue("\x0942");
        //public static readonly KeyValue CombiningDevanagariVowelSignVocalicR = new KeyValue("\x0943");
        
        public static readonly KeyValue CombiningDiaeresisOrUmlautKey = new KeyValue("\x0308");
        public static readonly KeyValue CombiningDotAboveKey = new KeyValue("\x0307");
        public static readonly KeyValue CombiningDotAboveRightKey = new KeyValue("\x0358");
        public static readonly KeyValue CombiningDotBelowKey = new KeyValue("\x0323");
        public static readonly KeyValue CombiningDoubleAcuteAccentKey = new KeyValue("\x030B");
        public static readonly KeyValue CombiningDoubleGraveAccentKey = new KeyValue("\x030F");
        public static readonly KeyValue CombiningGraveAccentKey = new KeyValue("\x0300");
        public static readonly KeyValue CombiningHookAboveKey = new KeyValue("\x0309");
        public static readonly KeyValue CombiningHornKey = new KeyValue("\x031B");
        public static readonly KeyValue CombiningInvertedBreveKey = new KeyValue("\x0311");
        public static readonly KeyValue CombiningIotaSubscriptOrYpogegrammeniKey = new KeyValue("\x0345");
        public static readonly KeyValue CombiningMacronKey = new KeyValue("\x0304");
        public static readonly KeyValue CombiningOgonekOrNosineKey = new KeyValue("\x0328");
        public static readonly KeyValue CombiningPalatalizedHookBelowKey = new KeyValue("\x0321");
        public static readonly KeyValue CombiningPerispomeneKey = new KeyValue("\x0342");
        public static readonly KeyValue CombiningRetroflexHookBelowKey = new KeyValue("\x0322");
        public static readonly KeyValue CombiningReversedCommaAboveOrRoughBreathingKey = new KeyValue("\x0314");
        public static readonly KeyValue CombiningRingAboveKey = new KeyValue("\x030A");
        public static readonly KeyValue CombiningRingBelowKey = new KeyValue("\x0325");
        public static readonly KeyValue CombiningTildeKey = new KeyValue("\x0303");
        public static readonly KeyValue CommuniKateKey = new KeyValue(FunctionKeys.CommuniKate);
        public static readonly KeyValue CommuniKateKeyboardKey = new KeyValue(FunctionKeys.CommuniKateKeyboard);
        public static readonly KeyValue CroatianCroatiaKey = new KeyValue(FunctionKeys.CroatianCroatia);
        public static readonly KeyValue ConversationAlpha1KeyboardKey = new KeyValue(FunctionKeys.ConversationAlpha1Keyboard);
        public static readonly KeyValue ConversationAlpha2KeyboardKey = new KeyValue(FunctionKeys.ConversationAlpha2Keyboard);
        public static readonly KeyValue ConversationCommuniKateKeyboardKey = new KeyValue(FunctionKeys.ConversationCommuniKateKeyboard);
        public static readonly KeyValue ConversationConfirmKeyboardKey = new KeyValue(FunctionKeys.ConversationConfirmKeyboard);
        public static readonly KeyValue ConversationConfirmYesKey = new KeyValue(FunctionKeys.ConversationConfirmYes);
        public static readonly KeyValue ConversationConfirmNoKey = new KeyValue(FunctionKeys.ConversationConfirmNo);
        public static readonly KeyValue ConversationNumericAndSymbolsKeyboardKey = new KeyValue(FunctionKeys.ConversationNumericAndSymbolsKeyboard);
        public static readonly KeyValue CopyAllFromScratchpad = new KeyValue(FunctionKeys.CopyAllFromScratchpad);
        public static readonly KeyValue Currencies1KeyboardKey = new KeyValue(FunctionKeys.Currencies1Keyboard);
        public static readonly KeyValue Currencies2KeyboardKey = new KeyValue(FunctionKeys.Currencies2Keyboard);
        public static readonly KeyValue CzechCzechRepublicKey = new KeyValue(FunctionKeys.CzechCzechRepublic);
        public static readonly KeyValue DanishDenmarkKey = new KeyValue(FunctionKeys.DanishDenmark);
        public static readonly KeyValue DecreaseOpacityKey = new KeyValue(FunctionKeys.DecreaseOpacity);
        public static readonly KeyValue DeleteKey = new KeyValue(FunctionKeys.Delete);
        public static readonly KeyValue Diacritic1KeyboardKey = new KeyValue(FunctionKeys.Diacritic1Keyboard);
        public static readonly KeyValue Diacritic2KeyboardKey = new KeyValue(FunctionKeys.Diacritic2Keyboard);
        public static readonly KeyValue Diacritic3KeyboardKey = new KeyValue(FunctionKeys.Diacritic3Keyboard);
        public static readonly KeyValue DutchBelgiumKey = new KeyValue(FunctionKeys.DutchBelgium);
        public static readonly KeyValue DutchNetherlandsKey = new KeyValue(FunctionKeys.DutchNetherlands);
        public static readonly KeyValue DynamicKeyboardKey = new KeyValue(FunctionKeys.DynamicKeyboard);
        public static readonly KeyValue DynamicKeyboardNext = new KeyValue(FunctionKeys.DynamicKeyboardNext);
        public static readonly KeyValue DynamicKeyboardPrev = new KeyValue(FunctionKeys.DynamicKeyboardPrev);
        public static readonly KeyValue EndKey = new KeyValue(FunctionKeys.End);
        public static readonly KeyValue EnglishCanadaKey = new KeyValue(FunctionKeys.EnglishCanada);
        public static readonly KeyValue EnglishUKKey = new KeyValue(FunctionKeys.EnglishUK);
        public static readonly KeyValue EnglishUSKey = new KeyValue(FunctionKeys.EnglishUS);
        public static readonly KeyValue ExpandToBottomKey = new KeyValue(FunctionKeys.ExpandToBottom);
        public static readonly KeyValue ExpandToBottomAndLeftKey = new KeyValue(FunctionKeys.ExpandToBottomAndLeft);
        public static readonly KeyValue ExpandToBottomAndRightKey = new KeyValue(FunctionKeys.ExpandToBottomAndRight);
        public static readonly KeyValue ExpandToLeftKey = new KeyValue(FunctionKeys.ExpandToLeft);
        public static readonly KeyValue ExpandToRightKey = new KeyValue(FunctionKeys.ExpandToRight);
        public static readonly KeyValue ExpandToTopKey = new KeyValue(FunctionKeys.ExpandToTop);
        public static readonly KeyValue ExpandToTopAndLeftKey = new KeyValue(FunctionKeys.ExpandToTopAndLeft);
        public static readonly KeyValue ExpandToTopAndRightKey = new KeyValue(FunctionKeys.ExpandToTopAndRight);
        public static readonly KeyValue EscapeKey = new KeyValue(FunctionKeys.Escape);
        public static readonly KeyValue ExpandDockKey = new KeyValue(FunctionKeys.ExpandDock);
        public static readonly KeyValue F1Key = new KeyValue(FunctionKeys.F1);
        public static readonly KeyValue F2Key = new KeyValue(FunctionKeys.F2);
        public static readonly KeyValue F3Key = new KeyValue(FunctionKeys.F3);
        public static readonly KeyValue F4Key = new KeyValue(FunctionKeys.F4);
        public static readonly KeyValue F5Key = new KeyValue(FunctionKeys.F5);
        public static readonly KeyValue F6Key = new KeyValue(FunctionKeys.F6);
        public static readonly KeyValue F7Key = new KeyValue(FunctionKeys.F7);
        public static readonly KeyValue F8Key = new KeyValue(FunctionKeys.F8);
        public static readonly KeyValue F9Key = new KeyValue(FunctionKeys.F9);
        public static readonly KeyValue F10Key = new KeyValue(FunctionKeys.F10);
        public static readonly KeyValue F11Key = new KeyValue(FunctionKeys.F11);
        public static readonly KeyValue F12Key = new KeyValue(FunctionKeys.F12);
        public static readonly KeyValue F13Key = new KeyValue(FunctionKeys.F13);
        public static readonly KeyValue F14Key = new KeyValue(FunctionKeys.F14);
        public static readonly KeyValue F15Key = new KeyValue(FunctionKeys.F15);
        public static readonly KeyValue F16Key = new KeyValue(FunctionKeys.F16);
        public static readonly KeyValue F17Key = new KeyValue(FunctionKeys.F17);
        public static readonly KeyValue F18Key = new KeyValue(FunctionKeys.F18);
        public static readonly KeyValue F19Key = new KeyValue(FunctionKeys.F19);
        public static readonly KeyValue FinnishFinlandKey = new KeyValue(FunctionKeys.FinnishFinland);
        public static readonly KeyValue FrenchFranceKey = new KeyValue(FunctionKeys.FrenchFrance);
        public static readonly KeyValue FrenchCanadaKey = new KeyValue(FunctionKeys.FrenchCanada);
        public static readonly KeyValue GeorgianGeorgiaKey = new KeyValue(FunctionKeys.GeorgianGeorgia);
        public static readonly KeyValue GermanGermanyKey = new KeyValue(FunctionKeys.GermanGermany);
        public static readonly KeyValue GreekGreeceKey = new KeyValue(FunctionKeys.GreekGreece);
        public static readonly KeyValue HebrewIsraelKey = new KeyValue(FunctionKeys.HebrewIsrael);
        public static readonly KeyValue HindiIndiaKey = new KeyValue(FunctionKeys.HindiIndia);
        public static readonly KeyValue HomeKey = new KeyValue(FunctionKeys.Home);
        public static readonly KeyValue HungarianHungaryKey = new KeyValue(FunctionKeys.HungarianHungary);
        public static readonly KeyValue IncreaseOpacityKey = new KeyValue(FunctionKeys.IncreaseOpacity);
        public static readonly KeyValue InsertKey = new KeyValue(FunctionKeys.Insert);
        public static readonly KeyValue ItalianItalyKey = new KeyValue(FunctionKeys.ItalianItaly);
        public static readonly KeyValue JapaneseJapanKey = new KeyValue(FunctionKeys.JapaneseJapan);
        public static readonly KeyValue KoreanKoreaKey = new KeyValue(FunctionKeys.KoreanKorea);
        public static readonly KeyValue LanguageKeyboardKey = new KeyValue(FunctionKeys.LanguageKeyboard);
        public static readonly KeyValue LeftAltKey = new KeyValue(FunctionKeys.LeftAlt);
        public static readonly KeyValue LeftCtrlKey = new KeyValue(FunctionKeys.LeftCtrl);
        public static readonly KeyValue LeftShiftKey = new KeyValue(FunctionKeys.LeftShift);
        public static readonly KeyValue LeftWinKey = new KeyValue(FunctionKeys.LeftWin);
        public static readonly KeyValue LookToScrollActiveKey = new KeyValue(FunctionKeys.LookToScrollActive);
        public static readonly KeyValue LookToScrollBoundsKey = new KeyValue(FunctionKeys.LookToScrollBounds);
        public static readonly KeyValue LookToScrollIncrementKey = new KeyValue(FunctionKeys.LookToScrollIncrement);
        public static readonly KeyValue LookToScrollModeKey = new KeyValue(FunctionKeys.LookToScrollMode);
        public static readonly KeyValue LookToScrollSpeedKey = new KeyValue(FunctionKeys.LookToScrollSpeed);
        public static readonly KeyValue MenuKey = new KeyValue(FunctionKeys.Menu);
        public static readonly KeyValue MenuKeyboardKey = new KeyValue(FunctionKeys.MenuKeyboard);
        public static readonly KeyValue MinimiseKey = new KeyValue(FunctionKeys.Minimise);
        public static readonly KeyValue MoreKey = new KeyValue(FunctionKeys.More);
        public static readonly KeyValue MouseDragKey = new KeyValue(FunctionKeys.MouseDrag);
        public static readonly KeyValue MouseKeyboardKey = new KeyValue(FunctionKeys.MouseKeyboard);
        public static readonly KeyValue MouseLeftClickKey = new KeyValue(FunctionKeys.MouseLeftClick);
        public static readonly KeyValue MouseLeftDoubleClickKey = new KeyValue(FunctionKeys.MouseLeftDoubleClick);
        public static readonly KeyValue MouseLeftDownUpKey = new KeyValue(FunctionKeys.MouseLeftDownUp);
        public static readonly KeyValue MouseMagneticCursorKey = new KeyValue(FunctionKeys.MouseMagneticCursor);
        public static readonly KeyValue MouseMiddleClickKey = new KeyValue(FunctionKeys.MouseMiddleClick);
        public static readonly KeyValue MouseMiddleDownUpKey = new KeyValue(FunctionKeys.MouseMiddleDownUp);
        public static readonly KeyValue MouseMoveAmountInPixelsKey = new KeyValue(FunctionKeys.MouseMoveAmountInPixels);
        public static readonly KeyValue MouseMoveAndLeftClickKey = new KeyValue(FunctionKeys.MouseMoveAndLeftClick);
        public static readonly KeyValue MouseMoveAndLeftDoubleClickKey = new KeyValue(FunctionKeys.MouseMoveAndLeftDoubleClick);
        public static readonly KeyValue MouseMoveAndMiddleClickKey = new KeyValue(FunctionKeys.MouseMoveAndMiddleClick);
        public static readonly KeyValue MouseMoveAndRightClickKey = new KeyValue(FunctionKeys.MouseMoveAndRightClick);
        public static readonly KeyValue MouseMoveToKey = new KeyValue(FunctionKeys.MouseMoveTo);
        public static readonly KeyValue MouseMoveToBottomKey = new KeyValue(FunctionKeys.MouseMoveToBottom);
        public static readonly KeyValue MouseMoveToLeftKey = new KeyValue(FunctionKeys.MouseMoveToLeft);
        public static readonly KeyValue MouseMoveToRightKey = new KeyValue(FunctionKeys.MouseMoveToRight);
        public static readonly KeyValue MouseMoveToTopKey = new KeyValue(FunctionKeys.MouseMoveToTop);
        public static readonly KeyValue MouseRightClickKey = new KeyValue(FunctionKeys.MouseRightClick);
        public static readonly KeyValue MouseRightDownUpKey = new KeyValue(FunctionKeys.MouseRightDownUp);
        public static readonly KeyValue MouseScrollAmountInClicksKey = new KeyValue(FunctionKeys.MouseScrollAmountInClicks);
        public static readonly KeyValue MouseScrollToTopKey = new KeyValue(FunctionKeys.MouseScrollToTop);
        public static readonly KeyValue MouseScrollToBottomKey = new KeyValue(FunctionKeys.MouseScrollToBottom);
        public static readonly KeyValue MouseMoveAndScrollToBottomKey = new KeyValue(FunctionKeys.MouseMoveAndScrollToBottom);
        public static readonly KeyValue MouseMoveAndScrollToLeftKey = new KeyValue(FunctionKeys.MouseMoveAndScrollToLeft);
        public static readonly KeyValue MouseMoveAndScrollToRightKey = new KeyValue(FunctionKeys.MouseMoveAndScrollToRight);
        public static readonly KeyValue MouseMoveAndScrollToTopKey = new KeyValue(FunctionKeys.MouseMoveAndScrollToTop);
        public static readonly KeyValue MouseMagnifierKey = new KeyValue(FunctionKeys.MouseMagnifier);
        public static readonly KeyValue MoveAndResizeAdjustmentAmountKey = new KeyValue(FunctionKeys.MoveAndResizeAdjustmentAmount);
        public static readonly KeyValue MoveToBottomKey = new KeyValue(FunctionKeys.MoveToBottom);
        public static readonly KeyValue MoveToBottomAndLeftKey = new KeyValue(FunctionKeys.MoveToBottomAndLeft);
        public static readonly KeyValue MoveToBottomAndLeftBoundariesKey = new KeyValue(FunctionKeys.MoveToBottomAndLeftBoundaries);
        public static readonly KeyValue MoveToBottomAndRightKey = new KeyValue(FunctionKeys.MoveToBottomAndRight);
        public static readonly KeyValue MoveToBottomAndRightBoundariesKey = new KeyValue(FunctionKeys.MoveToBottomAndRightBoundaries);
        public static readonly KeyValue MoveToBottomBoundaryKey = new KeyValue(FunctionKeys.MoveToBottomBoundary);
        public static readonly KeyValue MoveToLeftKey = new KeyValue(FunctionKeys.MoveToLeft);
        public static readonly KeyValue MoveToLeftBoundaryKey = new KeyValue(FunctionKeys.MoveToLeftBoundary);
        public static readonly KeyValue MoveToRightKey = new KeyValue(FunctionKeys.MoveToRight);
        public static readonly KeyValue MoveToRightBoundaryKey = new KeyValue(FunctionKeys.MoveToRightBoundary);
        public static readonly KeyValue MoveToTopKey = new KeyValue(FunctionKeys.MoveToTop);
        public static readonly KeyValue MoveToTopAndLeftKey = new KeyValue(FunctionKeys.MoveToTopAndLeft);
        public static readonly KeyValue MoveToTopAndLeftBoundariesKey = new KeyValue(FunctionKeys.MoveToTopAndLeftBoundaries);
        public static readonly KeyValue MoveToTopAndRightKey = new KeyValue(FunctionKeys.MoveToTopAndRight);
        public static readonly KeyValue MoveToTopAndRightBoundariesKey = new KeyValue(FunctionKeys.MoveToTopAndRightBoundaries);
        public static readonly KeyValue MoveToTopBoundaryKey = new KeyValue(FunctionKeys.MoveToTopBoundary);
        public static readonly KeyValue MultiKeySelectionIsOnKey = new KeyValue(FunctionKeys.MultiKeySelectionIsOn);
        public static readonly KeyValue NextSuggestionsKey = new KeyValue(FunctionKeys.NextSuggestions);
        public static readonly KeyValue NoQuestionResultKey = new KeyValue(FunctionKeys.NoQuestionResult);
        public static readonly KeyValue NumberLockKey = new KeyValue(FunctionKeys.NumberLock);
        public static readonly KeyValue NumericAndSymbols1KeyboardKey = new KeyValue(FunctionKeys.NumericAndSymbols1Keyboard);
        public static readonly KeyValue NumericAndSymbols2KeyboardKey = new KeyValue(FunctionKeys.NumericAndSymbols2Keyboard);
        public static readonly KeyValue NumericAndSymbols3KeyboardKey = new KeyValue(FunctionKeys.NumericAndSymbols3Keyboard);
        public static readonly KeyValue PersianIranKey = new KeyValue(FunctionKeys.PersianIran);
        public static readonly KeyValue PgDnKey = new KeyValue(FunctionKeys.PgDn);
        public static readonly KeyValue PgUpKey = new KeyValue(FunctionKeys.PgUp);
        public static readonly KeyValue PhysicalKeysKeyboardKey = new KeyValue(FunctionKeys.PhysicalKeysKeyboard);
        public static readonly KeyValue PolishPolandKey = new KeyValue(FunctionKeys.PolishPoland);
        public static readonly KeyValue PortuguesePortugalKey = new KeyValue(FunctionKeys.PortuguesePortugal);
        public static readonly KeyValue PortugueseBrazilKey = new KeyValue(FunctionKeys.PortugueseBrazil);
        public static readonly KeyValue PreviousSuggestionsKey = new KeyValue(FunctionKeys.PreviousSuggestions);
        public static readonly KeyValue PrintScreenKey = new KeyValue(FunctionKeys.PrintScreen);
        public static readonly KeyValue QuitKey = new KeyValue(FunctionKeys.Quit);
        public static readonly KeyValue RepeatLastMouseActionKey = new KeyValue(FunctionKeys.RepeatLastMouseAction);
        public static readonly KeyValue RestartKey = new KeyValue(FunctionKeys.Restart);
        public static readonly KeyValue RussianRussiaKey = new KeyValue(FunctionKeys.RussianRussia);
        public static readonly KeyValue SerbianSerbiaKey = new KeyValue(FunctionKeys.SerbianSerbia);
        public static readonly KeyValue ScrollLockKey = new KeyValue(FunctionKeys.ScrollLock);
        public static readonly KeyValue SelectVoiceKey = new KeyValue(FunctionKeys.SelectVoice);
        public static readonly KeyValue ShrinkFromBottomKey = new KeyValue(FunctionKeys.ShrinkFromBottom);
        public static readonly KeyValue ShrinkFromBottomAndLeftKey = new KeyValue(FunctionKeys.ShrinkFromBottomAndLeft);
        public static readonly KeyValue ShrinkFromBottomAndRightKey = new KeyValue(FunctionKeys.ShrinkFromBottomAndRight);
        public static readonly KeyValue ShrinkFromLeftKey = new KeyValue(FunctionKeys.ShrinkFromLeft);
        public static readonly KeyValue ShrinkFromRightKey = new KeyValue(FunctionKeys.ShrinkFromRight);
        public static readonly KeyValue ShrinkFromTopKey = new KeyValue(FunctionKeys.ShrinkFromTop);
        public static readonly KeyValue ShrinkFromTopAndLeftKey = new KeyValue(FunctionKeys.ShrinkFromTopAndLeft);
        public static readonly KeyValue ShrinkFromTopAndRightKey = new KeyValue(FunctionKeys.ShrinkFromTopAndRight);
        public static readonly KeyValue SimplifiedAlphaClearKey = new KeyValue(FunctionKeys.SimplifiedAlphaClear);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup1AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup1All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup2AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup2All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup3AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup3All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup4AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup4All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup5AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup5All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup6AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup6All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup7AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup7All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup8AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup8All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup9AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup9All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup10AllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup10All);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup1SubGroup1Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup1SubGroup1);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup1SubGroup2Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup1SubGroup2);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup1SubGroup3Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup1SubGroup3);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup2SubGroup1Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup2SubGroup1);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup2SubGroup2Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup2SubGroup2);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup2SubGroup3Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup2SubGroup3);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup3SubGroup1Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup3SubGroup1);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup3SubGroup2Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup3SubGroup2);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup3SubGroup3Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup3SubGroup3);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup4SubGroup1Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup4SubGroup1);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup4SubGroup2Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup4SubGroup2);
        public static readonly KeyValue SimplifiedKeyboardAlphaGroup4SubGroup3Key = new KeyValue(FunctionKeys.SimplifiedKeyboardAlphaGroup4SubGroup3);
        public static readonly KeyValue SimplifiedKeyboardNumericAllKey = new KeyValue(FunctionKeys.SimplifiedKeyboardNumericAll);
        public static readonly KeyValue SimplifiedKeyboardNumericGroup1Key = new KeyValue(FunctionKeys.SimplifiedKeyboardNumericGroup1);
        public static readonly KeyValue SimplifiedKeyboardNumericGroup2Key = new KeyValue(FunctionKeys.SimplifiedKeyboardNumericGroup2);
        public static readonly KeyValue SimplifiedKeyboardNumericGroup3Key = new KeyValue(FunctionKeys.SimplifiedKeyboardNumericGroup3);
        public static readonly KeyValue SimplifiedKeyboardGermanUmlaut = new KeyValue(FunctionKeys.SimplifiedKeyboardGermanUmlaut);
        public static readonly KeyValue SimplifiedKeyboardHome = new KeyValue(FunctionKeys.SimplifiedKeyboardHome);
        public static readonly KeyValue SizeAndPositionKeyboardKey = new KeyValue(FunctionKeys.SizeAndPositionKeyboard);
        public static readonly KeyValue SleepKey = new KeyValue(FunctionKeys.Sleep);
        public static readonly KeyValue SlovakSlovakiaKey = new KeyValue(FunctionKeys.SlovakSlovakia);
        public static readonly KeyValue SlovenianSloveniaKey = new KeyValue(FunctionKeys.SlovenianSlovenia);
        public static readonly KeyValue SpeakKey = new KeyValue(FunctionKeys.Speak);
        public static readonly KeyValue SpanishSpainKey = new KeyValue(FunctionKeys.SpanishSpain);
        public static readonly KeyValue Suggestion1Key = new KeyValue(FunctionKeys.Suggestion1);
        public static readonly KeyValue Suggestion2Key = new KeyValue(FunctionKeys.Suggestion2);
        public static readonly KeyValue Suggestion3Key = new KeyValue(FunctionKeys.Suggestion3);
        public static readonly KeyValue Suggestion4Key = new KeyValue(FunctionKeys.Suggestion4);
        public static readonly KeyValue Suggestion5Key = new KeyValue(FunctionKeys.Suggestion5);
        public static readonly KeyValue Suggestion6Key = new KeyValue(FunctionKeys.Suggestion6);
        public static readonly KeyValue TabKey = new KeyValue("\x09");
        public static readonly KeyValue ToggleCaseOfPreviousCharacterKey = new KeyValue(FunctionKeys.ToggleCaseOfPreviousCharacter);
        public static readonly KeyValue TranslationKey = new KeyValue(FunctionKeys.Translation);
        public static readonly KeyValue TurkishTurkeyKey = new KeyValue(FunctionKeys.TurkishTurkey);
        public static readonly KeyValue UkrainianUkraineKey = new KeyValue(FunctionKeys.UkrainianUkraine);
        public static readonly KeyValue UrduPakistanKey = new KeyValue(FunctionKeys.UrduPakistan);
        public static readonly KeyValue WebBrowsingKeyboardKey = new KeyValue(FunctionKeys.WebBrowsingKeyboard);
        public static readonly KeyValue YesQuestionResultKey = new KeyValue(FunctionKeys.YesQuestionResult);

        private static readonly Dictionary<Languages, List<KeyValue>> multiKeySelectionKeys;

        static KeyValues()
        {
            var defaultList = "abcdefghijklmnopqrstuvwxyz"
                .ToCharArray()
                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                .ToList();

            multiKeySelectionKeys = new Dictionary<Languages, List<KeyValue>>
            {
                { Languages.CatalanSpain, "abcdefghijklmnopqrstuvwxyzñç"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.CroatianCroatia, "abcčćdđefghijklmnopqrsštuvwxyzž"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                 { Languages.CzechCzechRepublic, "aábcčdďeéěfghiíjklmnňoópqrřsštťuúůvwxyýzž"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.DanishDenmark, "abcdefghijklmnopqrstuvwxyzæøå"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.DutchBelgium, defaultList },
                { Languages.DutchNetherlands, defaultList },
                { Languages.EnglishCanada, defaultList },
                { Languages.EnglishUK, defaultList },
                { Languages.EnglishUS, defaultList },
                { Languages.FinnishFinland, "abcdefghijklmnopqrstuvwxyzåäö"
                                                .ToCharArray()
                                                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                                                .ToList()
                },
                { Languages.FrenchFrance, "abcdefghijklmnopqrstuvwxyzçé"
                                                .ToCharArray()
                                                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                                                .ToList()
                },
                { Languages.FrenchCanada, "abcdefghijklmnopqrstuvwxyzçé"
                    .ToCharArray()
                    .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                    .ToList()
                },
                { Languages.GermanGermany, "abcdefghijklmnopqrstuvwxyzß"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.GeorgianGeorgia, "აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.GreekGreece, "ασδφγηξκλ;ςερτυθιοπζχψωβνμ"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.HebrewIsrael, "אבגדהוזחטיכלמנסעפצקרשת"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.HungarianHungary,"aábccsddzdzseéfggyhiíjkllymnnyoóöőpqrssztyuúüűvwxyzzs"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.ItalianItaly, "abcdefghijklmnopqrstuvwxyzî"
                                                .ToCharArray()
                                                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                                                .ToList()
                },
                { Languages.PersianIran, "ضصثقفغعهخحجچشسیبلاتنمکگظطژزرذدوپآ"
                    .ToCharArray()
                    .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                    .ToList()
                },
                { Languages.PolishPoland, "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzźż"
                                                .ToCharArray()
                                                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                                                .ToList()
                },
                { Languages.PortuguesePortugal, "abcdefghijklmnopqrstuvwxyzçà"
                                                .ToCharArray()
                                                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                                                .ToList()
                },
                { Languages.PortugueseBrazil, "abcdefghijklmnopqrstuvwxyzçà"
                                                .ToCharArray()
                                                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
                                                .ToList()
                },
                { Languages.RussianRussia, "абвгдеёжзийклмнопрстуфхцчшщъыьэюя"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList() },
                { Languages.SerbianSerbia, "абвгдђежзијклљмнњопрстћуфхцчџш"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.SlovakSlovakia, "aáäbcčdďeéfghchiíjklĺľmnoóôpqrŕsštťuúvwxyýzž"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.SlovenianSlovenia, "abcčćdđefghijklmnopqrsštuvwxyzž"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.SpanishSpain, "abcdefghijklmnopqrstuvwxyzñ"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.TurkishTurkey, "abcçdefgğhiıjklmnoöprsştuüvyz"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.UkrainianUkraine, "абвгґдеєжзиіїйклмнопрстуфхцчшщьюя"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.UrduPakistan, "طصھدٹپتبجحمورنلہاکیقفےسشغعظضذڈثۃھچخژزڑںءآگاً"
                    .ToCharArray()
                    .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                    .ToList()
                }
            };
        }

        public static List<KeyValue> MutuallyExclusiveMouseActionKeys
        {
            get
            {
                var keys = new List<KeyValue>
                {
                    MouseMoveAndLeftClickKey,
                    MouseMoveAndRightClickKey, 
                    MouseMoveAndLeftDoubleClickKey, 
                    MouseMoveAndMiddleClickKey,  
                    MouseMoveAndScrollToTopKey,
                    MouseMoveAndScrollToBottomKey,
                    MouseMoveAndScrollToLeftKey,
                    MouseMoveAndScrollToRightKey,
                };
                return keys;
            }
        }

        public static List<KeyValue> KeysWhichCanBePressedDown
        {
            get
            {
                var keys = new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftShiftKey,
                    LeftWinKey,
                    MouseMagnifierKey,
                    MultiKeySelectionIsOnKey
                };

                keys = keys.Concat(MutuallyExclusiveMouseActionKeys).ToList();

                if (!Settings.Default.TypeDiacriticsAfterLetters)
                {
                    //By default TypeDiacriticsAfterLetters is false and the default behaviour
                    //is for the user to press down the diacritic key and then the letter.
                    //Diacritics, therefore, have to be in the set which can be pressed down.
                    keys = keys.Concat(CombiningKeys).ToList();
                }

                if (Settings.Default.KeySelectionTriggerSource == TriggerSources.KeyboardKeyDownsUps)
                {
                    //LWIN cannot be pressed or locked down when using a KeyboardKeyDownUpSource as the trigger - the windows key
                    //does not combine as a modifier with other keys, which means we cannot detect any future trigger key presses
                    //once the LWIN key is down. The only work around is to prevent the key from being pressed down and used as a modifier.
                    //Issue for this bug https://globalmousekeyhook.codeplex.com/workitem/1188
                    //Source for this bug https://globalmousekeyhook.codeplex.com/SourceControl/latest#Main/MouseKeyboardActivityMonitor/KeyEventArgsExt.cs
                    keys.Remove(LeftWinKey);
                }

                return keys;
            }
        }

        public static List<KeyValue> KeysWhichCanBeLockedDown
        {
            get
            {
                var keys = new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftShiftKey,
                    LeftWinKey,
                    LookToScrollActiveKey,
                    LookToScrollBoundsKey,
                    MouseLeftDownUpKey,
                    MouseMagneticCursorKey,
                    MouseMagnifierKey,
                    MouseMiddleDownUpKey,
                    MouseRightDownUpKey,
                    MultiKeySelectionIsOnKey,                            
                    SleepKey
                };

                keys = keys.Concat(MutuallyExclusiveMouseActionKeys).ToList();

                if (Settings.Default.KeySelectionTriggerSource == TriggerSources.KeyboardKeyDownsUps)
                {
                    //LWIN cannot be pressed or locked down when using a KeyboardKeyDownUpSource as the trigger - the windows key
                    //does not combine as a modifier with other keys, which means we cannot detect any future trigger key presses
                    //once the LWIN key is down. The only work around is to prevent the key from being pressed down and used as a modifier.
                    //Issue for this bug https://globalmousekeyhook.codeplex.com/workitem/1188
                    //Source for this bug https://globalmousekeyhook.codeplex.com/SourceControl/latest#Main/MouseKeyboardActivityMonitor/KeyEventArgsExt.cs
                    keys.Remove(LeftWinKey);
                }

                return keys;
            }
        }

        public static List<KeyValue> KeysWhichCanBePressedOrLockedDown
        {
            get
            {
                return KeysWhichCanBePressedDown.Concat(KeysWhichCanBeLockedDown).Distinct().ToList();
            }
        }

        public static List<KeyValue> KeysWhichPreventTextCaptureIfDownOrLocked
        {
            get
            {
                return new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftWinKey
                };
            }
        }

        public static List<KeyValue> KeysDisabledWithMultiKeysSelectionIsOn
        {
            get
            {
                return CombiningKeys;
            }
        }

        public static List<KeyValue> CombiningKeys
        {
            get
            {
                return new List<KeyValue>
                {
                    //N.B. The order of these key values is important. This is the order in which combining keys which are down will be composed into a primary composite.
                    CombiningDiaeresisOrUmlautKey, //Diaeresis must be before the AcuteAccent if they are to combine into a COMBINING GREEK DIALYTIKA TONOS U+0344
                    CombiningAcuteAccentKey, //AuteAccent must be after the Diaeresis if they are to combine into a COMBINING GREEK DIALYTIKA TONOS U+0344
                    CombiningBreveKey,
                    CombiningCaronOrHacekKey,
                    CombiningCedillaKey,
                    CombiningCircumflexKey,
                    CombiningCommaAboveOrSmoothBreathingKey,
                    CombiningCyrillicPsiliPneumataOrSmoothBreathingKey,
                    CombiningDotAboveKey,
                    CombiningDotAboveRightKey,
                    CombiningDotBelowKey,
                    CombiningDoubleAcuteAccentKey,
                    CombiningDoubleGraveAccentKey,
                    CombiningGraveAccentKey,
                    CombiningHookAboveKey,
                    CombiningHornKey,
                    CombiningInvertedBreveKey,
                    CombiningIotaSubscriptOrYpogegrammeniKey,
                    CombiningMacronKey,
                    CombiningOgonekOrNosineKey,
                    CombiningPalatalizedHookBelowKey,
                    CombiningPerispomeneKey,
                    CombiningRetroflexHookBelowKey,
                    CombiningReversedCommaAboveOrRoughBreathingKey,
                    CombiningRingAboveKey,
                    CombiningRingBelowKey,
                    CombiningTildeKey,
                    CombiningArabicKasraKey,
                    CombiningArabicFathatanKey,
                    CombiningArabicDammaKey,
                    CombiningArabicDammatanKey,
                    CombiningArabicKasratanKey,
                    CombiningArabicLetterSuperscriptAlefKey,
                    CombiningArabicFathaKey,
                    CombiningArabicHamzaAboveKey,
                    CombiningArabicShaddaKey,
                    CombiningArabicMaddahAboveKey,
                    CombiningArabicSmallHighLigatureSadWithLamWithAlefMaksuraKey
                };
            }
        }

        /// <summary>
        /// Keys which are published when OptiKey is publishing (simulating key strokes). Otherwise these keys have no impact 
        /// on text within OptiKey (which is why LeftShift is not included as this modifies the case of entered text).
        /// </summary>
        public static List<KeyValue> PublishOnlyKeys
        {
            get
            {
                return new List<KeyValue>
                {
                    new KeyValue(FunctionKeys.LeftCtrl),
                    new KeyValue(FunctionKeys.LeftWin),
                    new KeyValue(FunctionKeys.LeftAlt),
                    new KeyValue(FunctionKeys.F1),
                    new KeyValue(FunctionKeys.F2),
                    new KeyValue(FunctionKeys.F3),
                    new KeyValue(FunctionKeys.F4),
                    new KeyValue(FunctionKeys.F5),
                    new KeyValue(FunctionKeys.F6),
                    new KeyValue(FunctionKeys.F7),
                    new KeyValue(FunctionKeys.F8),
                    new KeyValue(FunctionKeys.F9),
                    new KeyValue(FunctionKeys.F10),
                    new KeyValue(FunctionKeys.F11),
                    new KeyValue(FunctionKeys.F12),
                    new KeyValue(FunctionKeys.F13),
                    new KeyValue(FunctionKeys.F14),
                    new KeyValue(FunctionKeys.F15),
                    new KeyValue(FunctionKeys.F16),
                    new KeyValue(FunctionKeys.F17),
                    new KeyValue(FunctionKeys.F18),
                    new KeyValue(FunctionKeys.F19),
                    new KeyValue(FunctionKeys.PrintScreen),
                    new KeyValue(FunctionKeys.ScrollLock),
                    new KeyValue(FunctionKeys.NumberLock),
                    new KeyValue(FunctionKeys.Menu),
                    new KeyValue(FunctionKeys.ArrowUp),
                    new KeyValue(FunctionKeys.ArrowLeft),
                    new KeyValue(FunctionKeys.ArrowRight),
                    new KeyValue(FunctionKeys.ArrowDown),
                    new KeyValue(FunctionKeys.Break),
                    new KeyValue(FunctionKeys.Insert),
                    new KeyValue(FunctionKeys.Home),
                    new KeyValue(FunctionKeys.PgUp),
                    new KeyValue(FunctionKeys.PgDn),
                    new KeyValue(FunctionKeys.Delete),
                    new KeyValue(FunctionKeys.End),
                    new KeyValue(FunctionKeys.Escape)
                };
            }
        }

        public static List<KeyValue> MultiKeySelectionKeys
        {
            get
            {
                return multiKeySelectionKeys.ContainsKey(Settings.Default.KeyboardAndDictionaryLanguage)
                    ? multiKeySelectionKeys[Settings.Default.KeyboardAndDictionaryLanguage]
                    : new List<KeyValue>();
            }
        }
    }
}