using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Models
{
    public static class KeyValues
    {
        public static readonly KeyValue AddToDictionaryKey = new KeyValue(FunctionKeys.AddToDictionary);
        public static readonly KeyValue AlphaKeyboardKey = new KeyValue(FunctionKeys.AlphaKeyboard);
        public static readonly KeyValue ArrowDownKey = new KeyValue(FunctionKeys.ArrowDown);
        public static readonly KeyValue ArrowLeftKey = new KeyValue(FunctionKeys.ArrowLeft);
        public static readonly KeyValue ArrowRightKey = new KeyValue(FunctionKeys.ArrowRight);
        public static readonly KeyValue ArrowUpKey = new KeyValue(FunctionKeys.ArrowUp);
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
        public static readonly KeyValue CombiningBreveKey = new KeyValue("\x0306");
        public static readonly KeyValue CombiningCaronOrHacekKey = new KeyValue("\x030C");
        public static readonly KeyValue CombiningCedillaKey = new KeyValue("\x0327");
        public static readonly KeyValue CombiningCircumflexKey = new KeyValue("\x0302");
        public static readonly KeyValue CombiningCommaAboveOrSmoothBreathingKey = new KeyValue("\x0313");
        public static readonly KeyValue CombiningCyrillicPsiliPneumataOrSmoothBreathingKey = new KeyValue("\x0486");
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
        public static readonly KeyValue CommuniKate_1_235 = new KeyValue(FunctionKeys.CommuniKate_1_235);
        public static readonly KeyValue CommuniKate_1_236 = new KeyValue(FunctionKeys.CommuniKate_1_236);
        public static readonly KeyValue CommuniKate_1_237 = new KeyValue(FunctionKeys.CommuniKate_1_237);
        public static readonly KeyValue CommuniKate_1_238 = new KeyValue(FunctionKeys.CommuniKate_1_238);
        public static readonly KeyValue CommuniKate_1_239 = new KeyValue(FunctionKeys.CommuniKate_1_239);
        public static readonly KeyValue CommuniKate_1_240 = new KeyValue(FunctionKeys.CommuniKate_1_240);
        public static readonly KeyValue CommuniKate_1_241 = new KeyValue(FunctionKeys.CommuniKate_1_241);
        public static readonly KeyValue CommuniKate_1_242 = new KeyValue(FunctionKeys.CommuniKate_1_242);
        public static readonly KeyValue CommuniKate_1_243 = new KeyValue(FunctionKeys.CommuniKate_1_243);
        public static readonly KeyValue CommuniKate_1_244 = new KeyValue(FunctionKeys.CommuniKate_1_244);
        public static readonly KeyValue CommuniKate_1_245 = new KeyValue(FunctionKeys.CommuniKate_1_245);
        public static readonly KeyValue CommuniKate_1_246 = new KeyValue(FunctionKeys.CommuniKate_1_246);
        public static readonly KeyValue CommuniKate_1_247 = new KeyValue(FunctionKeys.CommuniKate_1_247);
        public static readonly KeyValue CommuniKate_1_248 = new KeyValue(FunctionKeys.CommuniKate_1_248);
        public static readonly KeyValue CommuniKate_1_249 = new KeyValue(FunctionKeys.CommuniKate_1_249);
        public static readonly KeyValue CommuniKate_1_250 = new KeyValue(FunctionKeys.CommuniKate_1_250);
        public static readonly KeyValue CommuniKate_1_251 = new KeyValue(FunctionKeys.CommuniKate_1_251);
        public static readonly KeyValue CommuniKate_1_252 = new KeyValue(FunctionKeys.CommuniKate_1_252);
        public static readonly KeyValue CommuniKate_1_253 = new KeyValue(FunctionKeys.CommuniKate_1_253);
        public static readonly KeyValue CommuniKate_1_254 = new KeyValue(FunctionKeys.CommuniKate_1_254);
        public static readonly KeyValue CommuniKate_1_255 = new KeyValue(FunctionKeys.CommuniKate_1_255);
        public static readonly KeyValue CommuniKate_1_256 = new KeyValue(FunctionKeys.CommuniKate_1_256);
        public static readonly KeyValue CommuniKate_1_257 = new KeyValue(FunctionKeys.CommuniKate_1_257);
        public static readonly KeyValue CommuniKate_1_258 = new KeyValue(FunctionKeys.CommuniKate_1_258);
        public static readonly KeyValue CommuniKate_1_259 = new KeyValue(FunctionKeys.CommuniKate_1_259);
        public static readonly KeyValue CommuniKate_1_260 = new KeyValue(FunctionKeys.CommuniKate_1_260);
        public static readonly KeyValue CommuniKate_1_261 = new KeyValue(FunctionKeys.CommuniKate_1_261);
        public static readonly KeyValue CommuniKate_1_262 = new KeyValue(FunctionKeys.CommuniKate_1_262);
        public static readonly KeyValue CommuniKate_1_263 = new KeyValue(FunctionKeys.CommuniKate_1_263);
        public static readonly KeyValue CommuniKate_1_264 = new KeyValue(FunctionKeys.CommuniKate_1_264);
        public static readonly KeyValue CommuniKate_1_265 = new KeyValue(FunctionKeys.CommuniKate_1_265);
        public static readonly KeyValue CommuniKate_1_266 = new KeyValue(FunctionKeys.CommuniKate_1_266);
        public static readonly KeyValue CommuniKate_1_267 = new KeyValue(FunctionKeys.CommuniKate_1_267);
        public static readonly KeyValue CommuniKate_1_268 = new KeyValue(FunctionKeys.CommuniKate_1_268);
        public static readonly KeyValue CommuniKate_1_269 = new KeyValue(FunctionKeys.CommuniKate_1_269);
        public static readonly KeyValue CommuniKate_1_270 = new KeyValue(FunctionKeys.CommuniKate_1_270);
        public static readonly KeyValue CommuniKate_1_271 = new KeyValue(FunctionKeys.CommuniKate_1_271);
        public static readonly KeyValue CommuniKate_1_272 = new KeyValue(FunctionKeys.CommuniKate_1_272);
        public static readonly KeyValue CommuniKate_1_273 = new KeyValue(FunctionKeys.CommuniKate_1_273);
        public static readonly KeyValue CommuniKate_1_274 = new KeyValue(FunctionKeys.CommuniKate_1_274);
        public static readonly KeyValue CommuniKate_1_275 = new KeyValue(FunctionKeys.CommuniKate_1_275);
        public static readonly KeyValue CommuniKate_1_276 = new KeyValue(FunctionKeys.CommuniKate_1_276);
        public static readonly KeyValue CommuniKate_1_277 = new KeyValue(FunctionKeys.CommuniKate_1_277);
        public static readonly KeyValue CommuniKate_1_278 = new KeyValue(FunctionKeys.CommuniKate_1_278);
        public static readonly KeyValue CommuniKate_1_279 = new KeyValue(FunctionKeys.CommuniKate_1_279);
        public static readonly KeyValue CommuniKate_1_280 = new KeyValue(FunctionKeys.CommuniKate_1_280);
        public static readonly KeyValue CommuniKate_1_281 = new KeyValue(FunctionKeys.CommuniKate_1_281);
        public static readonly KeyValue CommuniKate_1_282 = new KeyValue(FunctionKeys.CommuniKate_1_282);
        public static readonly KeyValue CommuniKate_1_283 = new KeyValue(FunctionKeys.CommuniKate_1_283);
        public static readonly KeyValue CommuniKate_1_284 = new KeyValue(FunctionKeys.CommuniKate_1_284);
        public static readonly KeyValue CommuniKate_1_285 = new KeyValue(FunctionKeys.CommuniKate_1_285);
        public static readonly KeyValue CommuniKate_1_286 = new KeyValue(FunctionKeys.CommuniKate_1_286);
        public static readonly KeyValue CommuniKate_1_287 = new KeyValue(FunctionKeys.CommuniKate_1_287);
        public static readonly KeyValue CommuniKate_1_288 = new KeyValue(FunctionKeys.CommuniKate_1_288);
        public static readonly KeyValue CommuniKate_1_289 = new KeyValue(FunctionKeys.CommuniKate_1_289);
        public static readonly KeyValue CommuniKate_1_290 = new KeyValue(FunctionKeys.CommuniKate_1_290);
        public static readonly KeyValue CommuniKate_1_291 = new KeyValue(FunctionKeys.CommuniKate_1_291);
        public static readonly KeyValue CommuniKate_1_292 = new KeyValue(FunctionKeys.CommuniKate_1_292);
        public static readonly KeyValue CommuniKate_1_293 = new KeyValue(FunctionKeys.CommuniKate_1_293);
        public static readonly KeyValue CommuniKate_1_294 = new KeyValue(FunctionKeys.CommuniKate_1_294);
        public static readonly KeyValue CommuniKate_1_295 = new KeyValue(FunctionKeys.CommuniKate_1_295);
        public static readonly KeyValue CommuniKate_1_296 = new KeyValue(FunctionKeys.CommuniKate_1_296);
        public static readonly KeyValue CommuniKate_1_297 = new KeyValue(FunctionKeys.CommuniKate_1_297);
        public static readonly KeyValue CommuniKate_1_298 = new KeyValue(FunctionKeys.CommuniKate_1_298);
        public static readonly KeyValue CommuniKate_1_299 = new KeyValue(FunctionKeys.CommuniKate_1_299);
        public static readonly KeyValue CommuniKate_1_300 = new KeyValue(FunctionKeys.CommuniKate_1_300);
        public static readonly KeyValue CommuniKate_1_301 = new KeyValue(FunctionKeys.CommuniKate_1_301);
        public static readonly KeyValue CommuniKate_1_302 = new KeyValue(FunctionKeys.CommuniKate_1_302);
        public static readonly KeyValue CommuniKate_1_303 = new KeyValue(FunctionKeys.CommuniKate_1_303);
        public static readonly KeyValue CommuniKate_1_304 = new KeyValue(FunctionKeys.CommuniKate_1_304);
        public static readonly KeyValue CommuniKate_1_305 = new KeyValue(FunctionKeys.CommuniKate_1_305);
        public static readonly KeyValue CommuniKate_1_306 = new KeyValue(FunctionKeys.CommuniKate_1_306);
        public static readonly KeyValue CommuniKate_1_307 = new KeyValue(FunctionKeys.CommuniKate_1_307);
        public static readonly KeyValue CommuniKate_1_308 = new KeyValue(FunctionKeys.CommuniKate_1_308);
        public static readonly KeyValue CommuniKate_1_309 = new KeyValue(FunctionKeys.CommuniKate_1_309);
        public static readonly KeyValue CommuniKate_1_310 = new KeyValue(FunctionKeys.CommuniKate_1_310);
        public static readonly KeyValue CommuniKate_1_311 = new KeyValue(FunctionKeys.CommuniKate_1_311);
        public static readonly KeyValue CommuniKate_1_312 = new KeyValue(FunctionKeys.CommuniKate_1_312);
        public static readonly KeyValue CommuniKate_1_313 = new KeyValue(FunctionKeys.CommuniKate_1_313);
        public static readonly KeyValue CommuniKate_1_314 = new KeyValue(FunctionKeys.CommuniKate_1_314);
        public static readonly KeyValue CommuniKate_1_315 = new KeyValue(FunctionKeys.CommuniKate_1_315);
        public static readonly KeyValue CommuniKate_1_316 = new KeyValue(FunctionKeys.CommuniKate_1_316);
        public static readonly KeyValue CommuniKate_1_317 = new KeyValue(FunctionKeys.CommuniKate_1_317);
        public static readonly KeyValue CommuniKate_1_318 = new KeyValue(FunctionKeys.CommuniKate_1_318);
        public static readonly KeyValue CommuniKate_1_319 = new KeyValue(FunctionKeys.CommuniKate_1_319);
        public static readonly KeyValue CommuniKate_1_320 = new KeyValue(FunctionKeys.CommuniKate_1_320);
        public static readonly KeyValue CommuniKate_1_321 = new KeyValue(FunctionKeys.CommuniKate_1_321);
        public static readonly KeyValue CommuniKate_1_322 = new KeyValue(FunctionKeys.CommuniKate_1_322);
        public static readonly KeyValue CommuniKate_1_323 = new KeyValue(FunctionKeys.CommuniKate_1_323);
        public static readonly KeyValue CommuniKate_1_324 = new KeyValue(FunctionKeys.CommuniKate_1_324);
        public static readonly KeyValue CommuniKate_1_325 = new KeyValue(FunctionKeys.CommuniKate_1_325);
        public static readonly KeyValue CommuniKate_1_326 = new KeyValue(FunctionKeys.CommuniKate_1_326);
        public static readonly KeyValue CommuniKate_1_327 = new KeyValue(FunctionKeys.CommuniKate_1_327);
        public static readonly KeyValue CommuniKate_1_328 = new KeyValue(FunctionKeys.CommuniKate_1_328);
        public static readonly KeyValue CommuniKate_1_329 = new KeyValue(FunctionKeys.CommuniKate_1_329);
        public static readonly KeyValue CommuniKate_1_330 = new KeyValue(FunctionKeys.CommuniKate_1_330);
        public static readonly KeyValue CommuniKate_1_331 = new KeyValue(FunctionKeys.CommuniKate_1_331);
        public static readonly KeyValue CommuniKate_ = new KeyValue(FunctionKeys.CommuniKate_);
        public static readonly KeyValue CommuniKate_Home = new KeyValue(FunctionKeys.CommuniKate_Home);
        public static readonly KeyValue CroatianCroatiaKey = new KeyValue(FunctionKeys.CroatianCroatia);
        public static readonly KeyValue ConversationAlphaKeyboardKey = new KeyValue(FunctionKeys.ConversationAlphaKeyboard);
        public static readonly KeyValue ConversationConfirmKeyboardKey = new KeyValue(FunctionKeys.ConversationConfirmKeyboard);
        public static readonly KeyValue ConversationConfirmYesKey = new KeyValue(FunctionKeys.ConversationConfirmYes);
        public static readonly KeyValue ConversationConfirmNoKey = new KeyValue(FunctionKeys.ConversationConfirmNo);
        public static readonly KeyValue ConversationNumericAndSymbolsKeyboardKey = new KeyValue(FunctionKeys.ConversationNumericAndSymbolsKeyboard);
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
        public static readonly KeyValue FrenchFranceKey = new KeyValue(FunctionKeys.FrenchFrance);
        public static readonly KeyValue GermanGermanyKey = new KeyValue(FunctionKeys.GermanGermany);
        public static readonly KeyValue GreekGreeceKey = new KeyValue(FunctionKeys.GreekGreece);
        public static readonly KeyValue HomeKey = new KeyValue(FunctionKeys.Home);
        public static readonly KeyValue IncreaseOpacityKey = new KeyValue(FunctionKeys.IncreaseOpacity);
        public static readonly KeyValue InsertKey = new KeyValue(FunctionKeys.Insert);
        public static readonly KeyValue ItalianItalyKey = new KeyValue(FunctionKeys.ItalianItaly);
        public static readonly KeyValue LanguageKeyboardKey = new KeyValue(FunctionKeys.LanguageKeyboard);
        public static readonly KeyValue LeftAltKey = new KeyValue(FunctionKeys.LeftAlt);
        public static readonly KeyValue LeftCtrlKey = new KeyValue(FunctionKeys.LeftCtrl);
        public static readonly KeyValue LeftShiftKey = new KeyValue(FunctionKeys.LeftShift);
        public static readonly KeyValue LeftWinKey = new KeyValue(FunctionKeys.LeftWin);
        public static readonly KeyValue MenuKey = new KeyValue(FunctionKeys.Menu);
        public static readonly KeyValue MenuKeyboardKey = new KeyValue(FunctionKeys.MenuKeyboard);
        public static readonly KeyValue MinimiseKey = new KeyValue(FunctionKeys.Minimise);
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
        public static readonly KeyValue PgDnKey = new KeyValue(FunctionKeys.PgDn);
        public static readonly KeyValue PgUpKey = new KeyValue(FunctionKeys.PgUp);
        public static readonly KeyValue PhysicalKeysKeyboardKey = new KeyValue(FunctionKeys.PhysicalKeysKeyboard);
        public static readonly KeyValue PortuguesePortugalKey = new KeyValue(FunctionKeys.PortuguesePortugal);
        public static readonly KeyValue PreviousSuggestionsKey = new KeyValue(FunctionKeys.PreviousSuggestions);
        public static readonly KeyValue PrintScreenKey = new KeyValue(FunctionKeys.PrintScreen);
        public static readonly KeyValue QuitKey = new KeyValue(FunctionKeys.Quit);
        public static readonly KeyValue RepeatLastMouseActionKey = new KeyValue(FunctionKeys.RepeatLastMouseAction);
        public static readonly KeyValue RussianRussiaKey = new KeyValue(FunctionKeys.RussianRussia);
        public static readonly KeyValue ScrollLockKey = new KeyValue(FunctionKeys.ScrollLock);
        public static readonly KeyValue ShrinkFromBottomKey = new KeyValue(FunctionKeys.ShrinkFromBottom);
        public static readonly KeyValue ShrinkFromBottomAndLeftKey = new KeyValue(FunctionKeys.ShrinkFromBottomAndLeft);
        public static readonly KeyValue ShrinkFromBottomAndRightKey = new KeyValue(FunctionKeys.ShrinkFromBottomAndRight);
        public static readonly KeyValue ShrinkFromLeftKey = new KeyValue(FunctionKeys.ShrinkFromLeft);
        public static readonly KeyValue ShrinkFromRightKey = new KeyValue(FunctionKeys.ShrinkFromRight);
        public static readonly KeyValue ShrinkFromTopKey = new KeyValue(FunctionKeys.ShrinkFromTop);
        public static readonly KeyValue ShrinkFromTopAndLeftKey = new KeyValue(FunctionKeys.ShrinkFromTopAndLeft);
        public static readonly KeyValue ShrinkFromTopAndRightKey = new KeyValue(FunctionKeys.ShrinkFromTopAndRight);
        public static readonly KeyValue SimplifiedAlphaClear = new KeyValue(FunctionKeys.SimplifiedAlphaClear);
        public static readonly KeyValue SimplifiedAlphaABCDEFGHI = new KeyValue(FunctionKeys.SimplifiedAlphaABCDEFGHI);
        public static readonly KeyValue SimplifiedAlphaJKLMNOPQR = new KeyValue(FunctionKeys.SimplifiedAlphaJKLMNOPQR);
        public static readonly KeyValue SimplifiedAlphaSTUVWXYZ = new KeyValue(FunctionKeys.SimplifiedAlphaSTUVWXYZ);
        public static readonly KeyValue SimplifiedAlphaABC = new KeyValue(FunctionKeys.SimplifiedAlphaABC);
        public static readonly KeyValue SimplifiedAlphaDEF = new KeyValue(FunctionKeys.SimplifiedAlphaDEF);
        public static readonly KeyValue SimplifiedAlphaGHI = new KeyValue(FunctionKeys.SimplifiedAlphaGHI);
        public static readonly KeyValue SimplifiedAlphaJKL = new KeyValue(FunctionKeys.SimplifiedAlphaJKL);
        public static readonly KeyValue SimplifiedAlphaMNO = new KeyValue(FunctionKeys.SimplifiedAlphaMNO);
        public static readonly KeyValue SimplifiedAlphaPQR = new KeyValue(FunctionKeys.SimplifiedAlphaPQR);
        public static readonly KeyValue SimplifiedAlphaSTU = new KeyValue(FunctionKeys.SimplifiedAlphaSTU);
        public static readonly KeyValue SimplifiedAlphaVWX = new KeyValue(FunctionKeys.SimplifiedAlphaVWX);
        public static readonly KeyValue SimplifiedAlphaYZ = new KeyValue(FunctionKeys.SimplifiedAlphaYZ);
        public static readonly KeyValue SimplifiedAlphaQE = new KeyValue(FunctionKeys.SimplifiedAlphaQE);
        public static readonly KeyValue SimplifiedAlphaNum = new KeyValue(FunctionKeys.SimplifiedAlphaNum);
        public static readonly KeyValue SimplifiedAlpha123 = new KeyValue(FunctionKeys.SimplifiedAlpha123);
        public static readonly KeyValue SimplifiedAlpha456 = new KeyValue(FunctionKeys.SimplifiedAlpha456);
        public static readonly KeyValue SimplifiedAlpha789 = new KeyValue(FunctionKeys.SimplifiedAlpha789);
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
        public static readonly KeyValue TurkishTurkeyKey = new KeyValue(FunctionKeys.TurkishTurkey);
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
                { Languages.DanishDenmark, "abcdefghijklmnopqrstuvxyzæøå"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.DutchBelgium, defaultList },
                { Languages.DutchNetherlands, defaultList },
                { Languages.EnglishCanada, defaultList },
                { Languages.EnglishUK, defaultList },
                { Languages.EnglishUS, defaultList },
                { Languages.FrenchFrance, defaultList },
                { Languages.GermanGermany, "abcdefghijklmnopqrstuvwxyzß"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.GreekGreece, "ασδφγηξκλ;ςερτυθιοπζχψωβνμ"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList()
                },
                { Languages.ItalianItaly, defaultList },
                { Languages.PortuguesePortugal, defaultList },
                { Languages.RussianRussia, "абвгдеёжзийклмнопрстуфхцчшщъыьэюя"
                                                .ToCharArray()
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
                                                .ToList() },
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
                }
            };
        }

        public static List<KeyValue> KeysWhichCanBePressedDown
        {
            get
            {
                return CombiningKeys.Concat(
                    new List <KeyValue>
                    {
                        LeftAltKey,
                        LeftCtrlKey,
                        LeftShiftKey,
                        LeftWinKey,
                        MouseMagnifierKey,
                        MultiKeySelectionIsOnKey
                    })
                    .ToList();
            }
        }

        public static List<KeyValue> KeysWhichCanBeLockedDown
        {
            get
            {
                return new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftShiftKey,
                    LeftWinKey,
                    MouseLeftDownUpKey,
                    MouseMagneticCursorKey,
                    MouseMagnifierKey,
                    MouseMiddleDownUpKey,
                    MouseRightDownUpKey,
                    MultiKeySelectionIsOnKey,
                    SleepKey
                };
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
                    CombiningTildeKey
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
            get { return multiKeySelectionKeys[Settings.Default.KeyboardAndDictionaryLanguage]; }
        }
    }
}
