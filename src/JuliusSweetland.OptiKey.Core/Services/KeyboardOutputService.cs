// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class KeyboardOutputService : BindableBase, IKeyboardOutputService
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IKeyStateService keyStateService;
        private readonly ISuggestionStateService suggestionService;
        private readonly IPublishService publishService;
        private readonly IDictionaryService dictionaryService;
        private readonly Action<KeyValue> fireKeySelectionEvent;
        private readonly Dictionary<bool, KeyboardOutputServiceState> state = new Dictionary<bool, KeyboardOutputServiceState>();

        private string text;
        private string lastProcessedText;
        private bool lastProcessedTextWasSuggestion;
        private bool lastProcessedTextWasMultiKey;
        private bool suppressNextAutoSpace = true;
        private bool keyboardIsShiftAware;
        private bool shiftStateSetAutomatically;

        #endregion

        #region Ctor

        public KeyboardOutputService(
            IKeyStateService keyStateService,
            ISuggestionStateService suggestionService,
            IPublishService publishService,
            IDictionaryService dictionaryService,
            Action<KeyValue> fireKeySelectionEvent)
        {
            this.keyStateService = keyStateService;
            this.suggestionService = suggestionService;
            this.publishService = publishService;
            this.dictionaryService = dictionaryService;
            this.fireKeySelectionEvent = fireKeySelectionEvent;

            ReactToSimulateKeyStrokesChanges();
            ReactToShiftStateChanges();
            ReactToPublishableKeyDownStateChanges();
            ReactToKeyboardIsShiftAwareChanges();
            ReactToSuppressAutoCapitaliseIntelligentlyChanges();
            AutoPressShiftIfAppropriate();
            GenerateSuggestions(true);
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        public bool KeyboardIsShiftAware //Not on interface as only accessed via databinding
        {
            get { return keyboardIsShiftAware; }
            set { SetProperty(ref keyboardIsShiftAware, value); }
        }

        #endregion

        #region Methods - IKeyboardOutputService

        public void ProcessFunctionKey(FunctionKeys functionKey)
        {
            Log.DebugFormat("Processing captured function key '{0}'", functionKey);

            switch (functionKey)
            {
                case FunctionKeys.BackMany:
                    if (!string.IsNullOrEmpty(Text))
                    {
                        var backManyCount = Text.CountBackToLastCharCategoryBoundary();

                        dictionaryService.DecrementEntryUsageCount(Text.Substring(Text.Length - backManyCount, backManyCount).Trim());

                        var textAfterBackMany = Text.Substring(0, Text.Length - backManyCount);
                        var textChangedByBackMany = Text != textAfterBackMany;
                        Text = textAfterBackMany;

                        if (backManyCount == 0) backManyCount = 1; //Always publish at least one backspace

                        for (int i = 0; i < backManyCount; i++)
                        {
                            PublishKeyPress(FunctionKeys.BackOne);
                            ReleaseUnlockedKeys();
                        }

                        if (textChangedByBackMany
                            || string.IsNullOrEmpty(Text))
                        {
                            AutoPressShiftIfAppropriate();
                        }

                        StoreLastProcessedText(null);
                        GenerateSuggestions(true);

                        Log.Debug("Suppressing next auto space.");
                        suppressNextAutoSpace = true;
                    }
                    else
                    {
                        //Scratchpad is empty, but publish 1 backspace anyway, as per the behaviour for 'BackOne'
                        PublishKeyPress(FunctionKeys.BackOne);
                    }

                    lastProcessedTextWasSuggestion = false;
                    break;

                case FunctionKeys.BackOne:
                    var backOneCount = string.IsNullOrEmpty(lastProcessedText)
                        ? 1 //Default to removing one character if no lastProcessedText
                        : lastProcessedText.Length;

                    var textChangedByBackOne = false;

                    if (!string.IsNullOrEmpty(Text))
                    {
                        if (Text.Length < backOneCount)
                        {
                            backOneCount = Text.Length; //Coallesce backCount if somehow the Text length is less
                        }

                        if (backOneCount == 1)
                        {
                            var inProgressWord = Text.InProgressWord(Text.Length);
                            if (inProgressWord != null && !Settings.Default.LimitBackOne)
                            {
                                //Attempt to break-apart/decompose in-progress word using normalisation
                                var decomposedInProgressWord = inProgressWord.Decompose();
                                if (decomposedInProgressWord != inProgressWord)
                                {
                                    Log.DebugFormat("In-progress word can be broken apart/decomposed using normalisation. It will be normalised from '{0}' to '{1}'.", inProgressWord, decomposedInProgressWord);

                                    //Remove in-progress word from Text
                                    Text = Text.Substring(0, Text.Length - inProgressWord.Length);

                                    //Add back the decomposed in-progress word, minus the last character, composed again (to recombine if possible), plus the last decomposed character (so the last character is decomposed and the prefix of the word is recombined if possible)
                                    var characterToRemove = decomposedInProgressWord.Last();
                                    var newInProgressWord = string.Concat(decomposedInProgressWord.Substring(0, decomposedInProgressWord.Length - 1).Compose(), characterToRemove);
                                    Text = string.Concat(Text, newInProgressWord);

                                    //Remove composed string from external applications by outputting backspaces, then replace with decomposed word
                                    for (var backCount = 0; backCount < inProgressWord.Length; backCount++)
                                    {
                                        PublishKeyPress(FunctionKeys.BackOne);
                                    }
                                    foreach (var c in newInProgressWord)
                                    {
                                        PublishKeyPress(c);
                                    }
                                }
                            }
                        }

                        var textAfterBackOne = Text.Substring(0, Text.Length - backOneCount);
                        textChangedByBackOne = Text != textAfterBackOne;

                        if (backOneCount > 1)
                        {
                            //Removing more than one character - only decrement removed string
                            dictionaryService.DecrementEntryUsageCount(Text.Substring(Text.Length - backOneCount, backOneCount).Trim());
                        }
                        else if (!string.IsNullOrEmpty(lastProcessedText)
                            && lastProcessedText.Length == 1
                            && !char.IsWhiteSpace(lastProcessedText[0]))
                        {
                            dictionaryService.DecrementEntryUsageCount(Text.InProgressWord(Text.Length)); //We are removing a non-whitespace character - decrement the in progress word
                            dictionaryService.IncrementEntryUsageCount(textAfterBackOne.InProgressWord(Text.Length)); //And increment the in progress word that is left after the removal
                        }

                        Text = textAfterBackOne;
                    }

                    for (int i = 0; i < backOneCount; i++)
                    {
                        PublishKeyPress(FunctionKeys.BackOne);
                        ReleaseUnlockedKeys();
                    }

                    if (textChangedByBackOne
                        || string.IsNullOrEmpty(Text))
                    {
                        AutoPressShiftIfAppropriate();
                    }

                    StoreLastProcessedText(null);
                    GenerateSuggestions(false);

                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    lastProcessedTextWasSuggestion = false;
                    break;

                case FunctionKeys.ClearScratchpad:
                    Text = null;
                    StoreLastProcessedText(null);
                    ClearSuggestions();
                    AutoPressShiftIfAppropriate();
                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    lastProcessedTextWasSuggestion = false;
                    GenerateSuggestions(false);
                    break;

                case FunctionKeys.ConversationConfirmYes:
                    Text = null;
                    StoreLastProcessedText(null);
                    ClearSuggestions();
                    AutoPressShiftIfAppropriate();
                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    Text = Resources.YES;
                    break;

                case FunctionKeys.ConversationConfirmNo:
                    Text = null;
                    StoreLastProcessedText(null);
                    ClearSuggestions();
                    AutoPressShiftIfAppropriate();
                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    Text = Resources.NO;
                    break;

                case FunctionKeys.LeftShift:
                    shiftStateSetAutomatically = false;
                    GenerateSuggestions(lastProcessedTextWasSuggestion);
                    break;

                case FunctionKeys.SimplifiedAlphaClear:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.Home;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup1All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup1All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup2All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup2All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup3All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup3All;
                    break;
                case FunctionKeys.SimplifiedKeyboardAlphaGroup4All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup4All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup5All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup5All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup6All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup6All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup7All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup7All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup8All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup8All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup9All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup9All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup10All:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup10All;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup1SubGroup1:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup1SubGroup1;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup1SubGroup2:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup1SubGroup2;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup1SubGroup3:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup1SubGroup3;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup2SubGroup1:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup2SubGroup1;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup2SubGroup2:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup2SubGroup2;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup2SubGroup3:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup2SubGroup3;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup3SubGroup1:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup3SubGroup1;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup3SubGroup2:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup3SubGroup2;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup3SubGroup3:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup3SubGroup3;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup4SubGroup1:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup4SubGroup1;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup4SubGroup2:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup4SubGroup2;
                    break;

                case FunctionKeys.SimplifiedKeyboardAlphaGroup4SubGroup3:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.AlphaGroup4SubGroup3;
                    break;

                case FunctionKeys.SimplifiedKeyboardNumericAll:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.NumericAll;
                    break;

                case FunctionKeys.SimplifiedKeyboardNumericGroup1:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.NumericGroup1;
                    break;

                case FunctionKeys.SimplifiedKeyboardNumericGroup2:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.NumericGroup2;
                    break;

                case FunctionKeys.SimplifiedKeyboardNumericGroup3:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.NumericGroup3;
                    break;

                case FunctionKeys.SimplifiedKeyboardGermanUmlaut:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.GermanUmlaut;
                    break;

                case FunctionKeys.SimplifiedKeyboardHome:
                    Settings.Default.SimplifiedKeyboardContext = SimplifiedKeyboardContexts.Home;
                    break;

                case FunctionKeys.Suggestion1:
                    SwapLastTextChangeForSuggestion(0);
                    lastProcessedTextWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion2:
                    SwapLastTextChangeForSuggestion(1);
                    lastProcessedTextWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion3:
                    SwapLastTextChangeForSuggestion(2);
                    lastProcessedTextWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion4:
                    SwapLastTextChangeForSuggestion(3);
                    lastProcessedTextWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion5:
                    SwapLastTextChangeForSuggestion(4);
                    lastProcessedTextWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion6:
                    SwapLastTextChangeForSuggestion(5);
                    lastProcessedTextWasSuggestion = true;
                    break;

                case FunctionKeys.ToggleCaseOfPreviousCharacter:
                    {
                        var inProgressWord = Text == null ? null : Text.InProgressWord(Text.Length);
                        if (inProgressWord != null)
                        {
                            //Attempt to break-apart/decompose in-progress word using normalisation
                            var decomposedInProgressWord = inProgressWord.Decompose();
                            if (decomposedInProgressWord != inProgressWord)
                            {
                                Log.DebugFormat("In-progress word can be broken apart/decomposed using normalisation. It will be normalised from '{0}' to '{1}'.", inProgressWord, decomposedInProgressWord);
                            }

                            //Remove in-progress word from Text
                            Text = Text.Substring(0, Text.Length - inProgressWord.Length);

                            //Add back the decomposed in-progress word, with the last character (potentially) cased differently (lower->upper or vice versa), composed again (to recombine if possible)
                            var finalCharacter = decomposedInProgressWord.Last();
                            var reCasedFinalCharacter = finalCharacter.ToggleCase();
                            if (finalCharacter != reCasedFinalCharacter)
                            {
                                Log.DebugFormat("Last character of in-progress word has had its case toggled. It will be converted from '{0}' to '{1}'.", finalCharacter, reCasedFinalCharacter);
                            }
                            var newInProgressWord = string.Concat(decomposedInProgressWord.Substring(0, decomposedInProgressWord.Length - 1), reCasedFinalCharacter).Compose();
                            Text = string.Concat(Text, newInProgressWord);

                            //Remove composed string from external applications by outputting backspaces, then replace with decomposed word
                            for (var backCount = 0; backCount < inProgressWord.Length; backCount++)
                            {
                                PublishKeyPress(FunctionKeys.BackOne);
                            }

                            foreach (var c in newInProgressWord)
                            {
                                PublishKeyPress(c);
                            }

                            dictionaryService.DecrementEntryUsageCount(inProgressWord); //Decrement the original in-progress word
                            dictionaryService.IncrementEntryUsageCount(newInProgressWord); //And increment the new in-progress word

                            GenerateSuggestions(false);
                            lastProcessedTextWasSuggestion = false;
                        }
                    }
                    break;

                default:
                    if (functionKey.ToVirtualKeyCode() != null)
                    {
                        //Key corresponds to physical keyboard key
                        GenerateSuggestions(false);

                        //If the key cannot be pressed or locked down (these are handled in
                        //ReactToPublishableKeyDownStateChanges) then publish it and release unlocked keys
                        var keyValue = new KeyValue(functionKey);
                        if (!KeyValues.KeysWhichCanBePressedOrLockedDown.Contains(keyValue))
                        {
                            PublishKeyPress(functionKey);
                            ReleaseUnlockedKeys();
                        }
                    }

                    lastProcessedTextWasSuggestion = false;
                    break;
            }
        }

        public void ProcessSingleKeyText(string capturedText)
        {
            //If the setting to type diacritics after letters is set to true then we want to apply the diacritic as soon as it is typed.
            //Otherwise suppress it for now (it will be pressed down) and apply it when the next non-combining key is pressed.
            if (!Settings.Default.TypeDiacriticsAfterLetters
                && KeyValues.CombiningKeys.Any(k => k.String == capturedText))
            {
                //These are dead keys - do nothing until we have a letter to combine the pressed dead keys with
                Log.InfoFormat("Suppressing processing on {0} as it is a dead key", capturedText.ToPrintableString());
                return;
            }

            Log.DebugFormat("Processing single key captured text '{0}'", capturedText.ToPrintableString());

            var capturedTextAfterComposition = CombineStringWithActiveDeadKeys(capturedText);
            ProcessText(capturedTextAfterComposition, true);

            lastProcessedTextWasSuggestion = false;

            //Special handling for simplified keyboards
            if (Settings.Default.UseSimplifiedKeyboardLayout)
            {
                char last = capturedText.LastOrDefault();
                Settings.Default.SimplifiedKeyboardContext = last != default(char) && char.IsNumber(last)
                    ? SimplifiedKeyboardContexts.NumericAll
                    : SimplifiedKeyboardContexts.Home;
            }
        }

        public async Task ProcessSingleKeyPress(string inKey, KeyPressKeyValue.KeyPressType type)
        {
            Log.InfoFormat("ProcessSingleKeyPress called for key [{0}] press type [{1}]", inKey, type);

            // Special case for function keys
            FunctionKeys funKey;
            var virtualKeyCode = (Enum.TryParse(inKey, out funKey)) ? funKey.ToVirtualKeyCode() : null;
            if (virtualKeyCode != null)
            {
                if (type == KeyPressKeyValue.KeyPressType.Press)
                    publishService.KeyDown(virtualKeyCode.Value);
                else if (type == KeyPressKeyValue.KeyPressType.Release)
                    publishService.KeyUp(virtualKeyCode.Value);
                else
                    publishService.KeyDownUp(virtualKeyCode.Value);
                return;
            }

            // If not an Optikey FunctionKey, check if it matches a virtual key name
            VirtualKeyCode vkCode;
            if (Enum.TryParse(inKey, true, out vkCode)) // ignore case
            {
                if (type == KeyPressKeyValue.KeyPressType.Press)
                    publishService.KeyDown(vkCode);
                else if (type == KeyPressKeyValue.KeyPressType.Release)
                    publishService.KeyUp(vkCode);
                else
                    publishService.KeyDownUp(vkCode);
                return;
            }
            
            // Otherwise a vanilla key press
            foreach (var chaKey in inKey)
            {
                this.PressKey(chaKey, type);
            }
        
        }

        private string CombineStringWithActiveDeadKeys(string input)
        {
            Log.InfoFormat("Combing dead keys (diacritics) on '{0}'", input);
            var sb = new StringBuilder(input);
            KeyValues.CombiningKeys.ForEach(combiningKey =>
            {
                if (keyStateService.KeyDownStates[combiningKey].Value.IsDownOrLockedDown())
                {
                    Log.DebugFormat("Appending '{0}' onto '{1}'", combiningKey.String.ToPrintableString(), sb.ToString());
                    sb.Append(combiningKey.String);
                }
            });

            var output = sb.ToString().Compose();
            return output;
        }

        public void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions)
        {
            Log.DebugFormat("Processing {0} captured multi-key selection results",
                captureAndSuggestions != null ? captureAndSuggestions.Count : 0);

            if (captureAndSuggestions == null || !captureAndSuggestions.Any()) return;

            StoreSuggestions(
                ModifySuggestions(captureAndSuggestions.Count > 1
                    ? captureAndSuggestions.Skip(1).ToList()
                    : null));

            ProcessText(captureAndSuggestions.First(), false);

            lastProcessedTextWasSuggestion = false;
            lastProcessedTextWasMultiKey = true;
        }

        #endregion

        #region Methods - private

        private void ProcessText(string newText, bool generateSuggestions)
        {
            Log.DebugFormat("Processing captured text '{0}'", newText.ToPrintableString());

            if (string.IsNullOrEmpty(newText)) return;

            //Suppress auto space if...
            if (string.IsNullOrWhiteSpace(lastProcessedText))
            {
                //We have no text change history, or the last capture was whitespace.
                Log.Debug("Suppressing auto space before this capture as the last text change was null or white space.");
                suppressNextAutoSpace = true;
            }
            else if(!Settings.Default.KeyboardAndDictionaryLanguage.SupportsAutoSpace()) //Language does not support auto space
            {
                Log.DebugFormat("Suppressing auto space before this capture as the KeyboardAndDictionaryLanguage {0} does not support auto space.", Settings.Default.KeyboardAndDictionaryLanguage);
                suppressNextAutoSpace = true;
            }
            else if (new StringInfo(lastProcessedText).LengthInTextElements == 1
                     && new StringInfo(newText).LengthInTextElements == 1
                     && !lastProcessedTextWasSuggestion
                     && !(keyStateService.KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value.IsDownOrLockedDown() && char.IsLetter(newText.First())))
            {
                //We are capturing single chars and are on the 2nd+ character,
                //the last capture wasn't a suggestion (as these can also be 1 character and we want to inject the space if it is),
                //and the current capture is not a multi-key selection of a single letter (as we also want to inject the space for this scenario).
                Log.Debug("Suppressing auto space before this capture as the user appears to be typing one char at a time. Also the last text change was not a suggestion, and the current capture is not a single letter captured with multi-key selection enabled.");
                suppressNextAutoSpace = true;
            }
            else if (newText.Length == 1
                && !char.IsLetter(newText.First()))
            {
                //We have captured a single char which is not a letter
                Log.Debug("Suppressing auto space before this capture as this capture is a single character which is not a letter.");
                suppressNextAutoSpace = true;
            }
            else if (lastProcessedText.Length == 1
                && !new[] {'.', '!', '?', ',', ':', ';', ')', ']', '}', '>'}.Contains(lastProcessedText.First())
                && !char.IsLetter(lastProcessedText.First()))
            {
                //The current capture (which we know is a letter or multi-key capture) follows a single character
                //which is not a letter, or a closing or mid-sentence punctuation; e.g. whitespace or a symbol
                Log.Debug("Suppressing auto space before this capture as it follows a single character which is not a letter, or a closing or mid-sentence punctuation mark.");
                suppressNextAutoSpace = true;
            }

            var textBeforeCaptureText = Text;

            //Modify and adjust the capture and apply to Text
            var newTextModified = ApplyModifierKeys(newText);
            var lastProcessedTextToStore = newTextModified;
            if (!string.IsNullOrEmpty(newTextModified))
            {
                var spaceAdded = AutoAddSpace();
                if (spaceAdded)
                {
                    //Auto space added - recalc whether shift should be auto-pressed
                    var shiftPressed = AutoPressShiftIfAppropriate();

                    if (shiftPressed)
                    {
                        //LeftShift has been auto-pressed - re-apply modifiers to captured text and suggestions
                        newTextModified = ApplyModifierKeys(newText);
                        lastProcessedTextToStore = newTextModified; //Store modified new text as last processed text
                        StoreSuggestions(ModifySuggestions(suggestionService.Suggestions));

                        //Ensure suggestions do not contain the modifiedText
                        if (!string.IsNullOrEmpty(newTextModified)
                            && suggestionService.Suggestions != null
                            && suggestionService.Suggestions.Contains(newTextModified))
                        {
                            suggestionService.Suggestions = suggestionService.Suggestions.Where(s => s != newTextModified).ToList();
                        }
                    }
                }

                var alreadyOutputInProgressWord = Text != null ? Text.InProgressWord(Text.Length) : null;
                if (newTextModified != null && alreadyOutputInProgressWord != null)
                {
                    var inProgressWordWithNewProcessedText = string.Concat(alreadyOutputInProgressWord, newTextModified);

                    //Attempt to adjust and combine (using normalisation) the in-progress word (with new processed text appended)
                    var adjustedInProgressWordWithNewProcessedText = AdjustInProgressWord(inProgressWordWithNewProcessedText);
                    var adjustedAndComposedInProgressWordWithNewProcessedText = adjustedInProgressWordWithNewProcessedText.Compose();
                    if (adjustedAndComposedInProgressWordWithNewProcessedText != inProgressWordWithNewProcessedText)
                    {
                        Log.DebugFormat("In-progress word (including new text) can be combined/composed using normalisation. It will be normalised from '{0}' to '{1}'.", inProgressWordWithNewProcessedText, adjustedAndComposedInProgressWordWithNewProcessedText);

                        int commonRootLength = 0;
                        for (var index = 0; index < adjustedAndComposedInProgressWordWithNewProcessedText.Length; index++)
                        {
                            if (adjustedAndComposedInProgressWordWithNewProcessedText[index] != inProgressWordWithNewProcessedText[index])
                            {
                                commonRootLength = index;
                                break;
                            }
                        }

                        var countOfCharactersToRemove = alreadyOutputInProgressWord.Length - commonRootLength;
                        if (countOfCharactersToRemove > Text.Length)
                        {
                            countOfCharactersToRemove = Text.Length; //Coerce to length of text as we can't remove more than we've already output
                        }

                        //Remove (from the end of the string) the part of the in-progress word which will be changed by composition
                        Text = Text.Substring(0, Text.Length - countOfCharactersToRemove);

                        //Remove changed in-progress word suffix string from external applications by outputting backspaces - the new suffix will be published seperately
                        for (var backCount = 0; backCount < countOfCharactersToRemove; backCount++) //Don't include newTextProcessed as it does not exist on Text yet
                        {
                            PublishKeyPress(FunctionKeys.BackOne);
                        }

                        newTextModified = adjustedAndComposedInProgressWordWithNewProcessedText.Substring(commonRootLength, adjustedAndComposedInProgressWordWithNewProcessedText.Length - commonRootLength);
                    }
                }

                Text = string.Concat(Text, newTextModified);
            }

            StoreLastProcessedText(lastProcessedTextToStore);

            //Increment/decrement usage counts
            if (newText.Length > 1)
            {
                dictionaryService.IncrementEntryUsageCount(newText);
            }
            else if (newText.Length == 1
                && !char.IsWhiteSpace(newText[0]))
            {
                if (!string.IsNullOrEmpty(textBeforeCaptureText))
                {
                    var previousInProgressWord = textBeforeCaptureText.InProgressWord(textBeforeCaptureText.Length);
                    dictionaryService.DecrementEntryUsageCount(previousInProgressWord);
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    var currentInProgressWord = Text.InProgressWord(Text.Length);
                    dictionaryService.IncrementEntryUsageCount(currentInProgressWord);
                }
            }

            //Publish each character (if SimulatingKeyStrokes), releasing 'on' (but not 'locked') modifier keys as appropriate
            var textToPublish = newTextModified ?? newText;
            foreach (var c in textToPublish)
            {
                PublishKeyPress(c);
                ReleaseUnlockedKeys();
            }

            if (!string.IsNullOrEmpty(newTextModified))
            {
                AutoPressShiftIfAppropriate();
                suppressNextAutoSpace = false;
            }

            if (generateSuggestions)
            {
                GenerateSuggestions(false);
            }
        }

        private string AdjustInProgressWord(string inProgressWordWithNewProcessedText)
        {
            //Korean (Hangul) specific adjustments
            if (Settings.Default.KeyboardAndDictionaryLanguage == Languages.KoreanKorea)
            {
                var decomposedInProgressWord = inProgressWordWithNewProcessedText.Decompose();
                StringBuilder result = new StringBuilder(decomposedInProgressWord);

                for (int index = 1; index < decomposedInProgressWord.Length; index++)
                {
                    //If the character is a consonant it can be an initial or final consonant (which have different unicode values)
                    //Decide which this is likely to be and convert it before normalisation occurs as initial/final consonants combine
                    //into syllables differently (or not at all).
                    if (result[index].CanBeInitialOrFinalHangulConsonant())
                    {
                        if (result[index - 1].UnicodeCodePointRange() == UnicodeCodePointRanges.HangulVowel //Previous char is a Hangul vowel
                            && (result.Length <= index + 1 //There isn't a char after this one
                            || (result.Length > index + 1 && result[index + 1].UnicodeCodePointRange() != UnicodeCodePointRanges.HangulVowel))) //Or next char exists and it is NOT a Hangul vowel
                        {
                            result[index] = result[index].ConvertToFinalHangulConsonant();
                        }
                        else
                        {
                            result[index] = result[index].ConvertToInitialHangulConsonant();
                        }
                    }
                }
                return result.ToString();
            }

            return inProgressWordWithNewProcessedText;
        }

        private bool AutoPressShiftIfAppropriate()
        {
            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
            {
                Log.Debug("Auto-pressing shift.");
                keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Down;
                if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.LeftShiftKey);
                shiftStateSetAutomatically = true;
                SuppressOrReinstateAutoCapitalisation();
                return true;
            }
            return false;
        }

        private void ReactToKeyboardIsShiftAwareChanges()
        {
            this.OnPropertyChanges(tos => tos.KeyboardIsShiftAware)
                .Subscribe(_ => SuppressOrReinstateAutoCapitalisation());
        }

        private void ReactToSuppressAutoCapitaliseIntelligentlyChanges()
        {
            Settings.Default.OnPropertyChanges(s => s.SuppressAutoCapitaliseIntelligently)
                .Subscribe(_ => ReactToSuppressAutoCapitaliseIntelligentlyChanges());
        }

        private void SuppressOrReinstateAutoCapitalisation()
        {
            if (Settings.Default.AutoCapitalise
                && Settings.Default.SuppressAutoCapitaliseIntelligently
                && shiftStateSetAutomatically)
            {
                if (KeyboardIsShiftAware
                    && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
                {
                    keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Down;
                    if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.LeftShiftKey);
                    return;
                }

                if (!KeyboardIsShiftAware
                    && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
                {
                    keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Up;
                    if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.LeftShiftKey);
                    return;
                }
            }
        }

        private void ReactToShiftStateChanges()
        {
            keyStateService.KeyDownStates[KeyValues.LeftShiftKey].OnPropertyChanges(np => np.Value)
                .Subscribe(_ =>
                {
                    if (!shiftStateSetAutomatically)
                    {
                        GenerateSuggestions(false);
                    }
                });
        }

        private void ReactToSimulateKeyStrokesChanges()
        {
            Log.Info("Adding SimulateKeyStrokes change handlers.");
            keyStateService.OnPropertyChanges(kss => kss.SimulateKeyStrokes).Subscribe(_ => SimulateKeyStrokesHasChanged(true));
            SimulateKeyStrokesHasChanged(false);
        }

        private void SimulateKeyStrokesHasChanged(bool saveCurrentState)
        {
            var newStateKey = keyStateService.SimulateKeyStrokes;
            var currentStateKey = !newStateKey;

            if (saveCurrentState)
            {
                //Save old state values
                var lastState = new KeyboardOutputServiceState(
                    currentStateKey,
                    () => text, s => Text = s, //Set property (not field) to trigger bindings
                    () => lastProcessedText, s => lastProcessedText = s,
                    () => lastProcessedTextWasSuggestion, b => lastProcessedTextWasSuggestion = b,
                    () => suppressNextAutoSpace, b => suppressNextAutoSpace = b,
                    () => shiftStateSetAutomatically, b => shiftStateSetAutomatically = b,
                    () => suggestionService.Suggestions, s => suggestionService.Suggestions = s);
                if (state.ContainsKey(currentStateKey))
                {
                    state[currentStateKey] = lastState;
                }
                else
                {
                    state.Add(currentStateKey, lastState);
                }
            }

            //Restore state or default state
            if (state.ContainsKey(newStateKey))
            {
                state[newStateKey].RestoreState();
            }
            else
            {
                Log.InfoFormat("No stored KeyboardOutputService state to restore for SimulateKeyStrokes={0} - defaulting state.", newStateKey);
                Text = null;
                StoreLastProcessedText(null);
                ClearSuggestions();
                ReleaseUnlockedKeys();
                AutoPressShiftIfAppropriate();
                Log.Debug("Suppressing next auto space.");
                suppressNextAutoSpace = true;
            }

            //Release all down keys
            publishService.ReleaseAllDownKeys();

            if (keyStateService.SimulateKeyStrokes)
            {
                //SimulatingKeyStrokes is on so publish key down events for all down/locked down keys
                KeyValues.KeysWhichCanBePressedOrLockedDown
                    .Where(key => keyStateService.KeyDownStates[key].Value.IsDownOrLockedDown() && key.FunctionKey != null)
                    .Select(key => key.FunctionKey.Value.ToVirtualKeyCode())
                    .Where(virtualKeyCode => virtualKeyCode != null)
                    .ToList()
                    .ForEach(virtualKeyCode => publishService.KeyDown(virtualKeyCode.Value));
            }
        }

        private void ReactToPublishableKeyDownStateChanges()
        {
            foreach (var key in KeyValues.KeysWhichCanBePressedOrLockedDown
                .Where(k => k.FunctionKey != null && k.FunctionKey.Value.ToVirtualKeyCode() != null))
            {
                var keyCopy = key; //Access to foreach variable in modified

                keyStateService.KeyDownStates[key].OnPropertyChanges(s => s.Value)
                    .Subscribe(value =>
                    {
                        if (keyStateService.SimulateKeyStrokes)
                        {
                            // ReSharper disable PossibleInvalidOperationException
                            var virtualKeyCode = keyCopy.FunctionKey.Value.ToVirtualKeyCode().Value;
                            // ReSharper restore PossibleInvalidOperationException

                            if (value.IsDownOrLockedDown())
                            {
                                publishService.KeyDown(virtualKeyCode);
                            }
                            else
                            {
                                publishService.KeyUp(virtualKeyCode);
                            }
                        }
                    });
            }
        }

        private void StoreLastProcessedText(string textChange)
        {
            Log.DebugFormat("Storing last text change '{0}'", textChange);
            lastProcessedText = textChange;
        }

        private void GenerateSuggestions(bool nextWord)
        {
            if (Settings.Default.SuggestWords)
            {
                Log.DebugFormat("Generating auto complete suggestions from '{0}'.", Text);

                var inProgressWord = Text == null ? null : Text.InProgressWord(Text.Length);
                var root = Text;

                if (!Settings.Default.SuggestNextWords)
                {
                    nextWord = false;
                    root = inProgressWord;
                }
                else if (lastProcessedTextWasMultiKey)
                {
                    lastProcessedTextWasMultiKey = false;
                    lastProcessedTextWasSuggestion = true;
                }

                var suggestions = dictionaryService.GetSuggestions(root, nextWord)
                    .Take(Settings.Default.MaxDictionaryMatchesOrSuggestions)
                    .ToList();

                Log.DebugFormat("{0} suggestions generated (possibly capped to {1} by MaxDictionaryMatchesOrSuggestions setting)",
                    suggestions.Count(), Settings.Default.MaxDictionaryMatchesOrSuggestions);

                if (!nextWord & inProgressWord != null)
                {
                    // Ensure that the entered word is in the list of suggestions...
                    if (!suggestions.Contains(inProgressWord, StringComparer.CurrentCultureIgnoreCase))
                    {
                        suggestions.Insert(0, inProgressWord);
                        suggestions = suggestions.Take(Settings.Default.MaxDictionaryMatchesOrSuggestions).ToList();
                    }
                    else
                    {
                        // ...and that it is at the front of the list.
                        var index =
                            suggestions.FindIndex(
                                s => string.Equals(s, inProgressWord, StringComparison.CurrentCultureIgnoreCase));
                        if (index > 0)
                        {
                            suggestions.Swap(0, index);
                        }
                    }
                }

                //Correctly case suggestions
                var leftShiftIsDown = keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down;
                var leftShiftIsLockedDown = keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown;
                suggestionService.Suggestions = suggestions.Select(suggestion =>
                {
                    var suggestionChars = suggestion.ToCharArray();
                    for (var index = 0; index < suggestionChars.Length; index++)
                    {
                        bool makeUppercase = false;
                        if(nextWord || inProgressWord == null)
                        {
                            if (index == 0 && (leftShiftIsDown || leftShiftIsLockedDown)) //First character should be uppercase
                            {
                                makeUppercase = true;
                            }
                            else if (index > 0 && leftShiftIsLockedDown)
                            {
                                makeUppercase = true; //Shift is locked down, so all characters after the in progress word should be uppercase
                            }
                            else if (index > 0 && leftShiftIsLockedDown)
                            {
                                makeUppercase = true; //Shift is locked down, so all characters after the in progress word should be uppercase
                            }
                        }
                        else
                        {
                            if (index < inProgressWord.Length
                                && char.IsUpper(inProgressWord[index]))
                            {
                                makeUppercase = true; //In progress word is uppercase as this index
                            }
                            else if (index == inProgressWord.Length
                                && (leftShiftIsDown || leftShiftIsLockedDown))
                            {
                                makeUppercase = true; //Shift is down, so the next character after the end of the in progress word should be uppercase
                            }
                            else if (index > inProgressWord.Length
                                        && leftShiftIsLockedDown)
                            {
                                makeUppercase = true; //Shift is locked down, so all characters after the in progress word should be uppercase
                            }
                        }

                        if (makeUppercase)
                        {
                            suggestionChars[index] = char.ToUpper(suggestion[index], Settings.Default.KeyboardAndDictionaryLanguage.ToCultureInfo());
                        }
                    }

                    return new string(suggestionChars);
                })
                .Distinct() //Changing the casing can result in multiple identical entries (e.g. "am" and "AM" both could become "am")
                .ToList();

                return;
            }
        }

        private void ClearSuggestions()
        {
            Log.Info("Clearing suggestions.");
            suggestionService.Suggestions = null;
        }

        private List<string> ModifySuggestions(List<string> suggestions)
        {
            Log.DebugFormat("Modifying {0} suggestions.", suggestions != null ? suggestions.Count : 0);

            if (suggestions == null || !suggestions.Any()) return null;

            var modifiedSuggestions = suggestions
                .Select(ApplyModifierKeys)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            Log.DebugFormat("After applying modifiers there are {0} modified suggestions.", modifiedSuggestions.Count);

            return modifiedSuggestions.Any() ? modifiedSuggestions : null;
        }

        private void StoreSuggestions(List<string> suggestions)
        {
            Log.DebugFormat("Storing {0} suggestions.", suggestions != null ? suggestions.Count : 0);

            suggestionService.Suggestions = suggestions != null && suggestions.Any()
                ? suggestions
                : null;
        }

        private void PublishKeyPress(FunctionKeys functionKey)
        {
            if (keyStateService.SimulateKeyStrokes)
            {
                var virtualKeyCode = functionKey.ToVirtualKeyCode();
                if (virtualKeyCode != null)
                {
                    Log.InfoFormat("Publishing function key '{0}' => as virtual key code {1}", functionKey, virtualKeyCode);
                    publishService.KeyDownUp(virtualKeyCode.Value);
                }
            }
        }

        private void PressKey(char character, KeyPressKeyValue.KeyPressType type)
        {

            //Get keyboard layout of currently focused window
            IntPtr hWnd = PInvoke.GetForegroundWindow();
            int lpdwProcessId;
            int winThreadProcId = PInvoke.GetWindowThreadProcessId(hWnd, out lpdwProcessId);
            IntPtr keyboardLayout = PInvoke.GetKeyboardLayout(winThreadProcId);

            //Convert this into a culture string for logging
            string keyboardCulture = "Unknown";
            var installedInputLanguages = InputLanguage.InstalledInputLanguages;
            for (int i = 0; i < installedInputLanguages.Count; i++)
            {
                if (keyboardLayout == installedInputLanguages[i].Handle)
                {
                    keyboardCulture = installedInputLanguages[i].Culture.DisplayName;
                    break;
                }
            }

            //Attempt to lookup virtual key code (and modifier states)
            var vkKeyScan = PInvoke.VkKeyScanEx(character, keyboardLayout);
            if (vkKeyScan != -1)
            {

                // vkKeyScan is valid result from VkKeyScanEx
                // Low-order byte contains virtual key code, higher-order byte contains modifier flag(s)
                var vkCode = vkKeyScan & 0xff;
                var shift = (vkKeyScan & 0x100) > 0;
                var ctrl = (vkKeyScan & 0x200) > 0;
                var alt = (vkKeyScan & 0x400) > 0;

                bool releaseShift = false;
                bool releaseCtrl = false;
                bool releaseAlt = false;

                if (shift && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
                {
                    publishService.KeyDown(FunctionKeys.LeftShift.ToVirtualKeyCode().Value);
                    releaseShift = true;
                }
                if (ctrl && keyStateService.KeyDownStates[KeyValues.LeftCtrlKey].Value == KeyDownStates.Up)
                {
                    publishService.KeyDown(FunctionKeys.LeftCtrl.ToVirtualKeyCode().Value);
                    releaseCtrl = true;
                }
                if (alt && keyStateService.KeyDownStates[KeyValues.LeftAltKey].Value == KeyDownStates.Up)
                {
                    publishService.KeyDown(FunctionKeys.LeftAlt.ToVirtualKeyCode().Value);
                    releaseAlt = true;
                }

                switch (type)
                {
                    case KeyPressKeyValue.KeyPressType.Press:
                        publishService.KeyDown((VirtualKeyCode)vkCode);
                        break;
                    case KeyPressKeyValue.KeyPressType.Release:
                        publishService.KeyUp((VirtualKeyCode)vkCode);
                        break;
                    case KeyPressKeyValue.KeyPressType.PressAndRelease:
                        publishService.KeyDownUp((VirtualKeyCode)vkCode);
                        break;
                }

                if (releaseShift)
                {
                    publishService.KeyUp(FunctionKeys.LeftShift.ToVirtualKeyCode().Value);
                }
                if (releaseCtrl)
                {
                    publishService.KeyUp(FunctionKeys.LeftCtrl.ToVirtualKeyCode().Value);
                }
                if (releaseAlt)
                {
                    publishService.KeyUp(FunctionKeys.LeftAlt.ToVirtualKeyCode().Value);
                }

                Log.InfoFormat("Publishing '{0}' => as virtual key code {1}(0x{1:X}){2}{3}{4} (using VkKeyScanEx with keyboard layout:{5})",
                    character.ToPrintableString(), vkCode, shift ? "+SHIFT" : null,
                    ctrl ? "+CTRL" : null, alt ? "+ALT" : null, keyboardCulture);
            }
            else
            {
                Log.InfoFormat("No virtual key code found for '{0}' so publishing as text (OS keyboard layout:{1})",
                    character.ToPrintableString(), keyboardCulture);
                publishService.TypeText(character.ToString());
            }
        }

        private void PublishKeyPress(char character)
        {
            if (keyStateService.SimulateKeyStrokes)
            {
                // Special characters
                var virtualKeyCode = character.ToVirtualKeyCode();
                if (virtualKeyCode != null)
                {
                    Log.InfoFormat("Publishing '{0}' => as virtual key code {1} (using hard coded mapping)",
                        character.ToPrintableString(), virtualKeyCode);
                    publishService.KeyDownUp(virtualKeyCode.Value);
                    return;
                }

                // Type text without publishing key presses
                if (!Settings.Default.PublishVirtualKeyCodesForCharacters)
                {
                    Log.InfoFormat("Publishing '{0}' as text", character.ToPrintableString());
                    publishService.TypeText(character.ToString());
                    return;
                }

                // Actually press (and release) key
                this.PressKey(character, KeyPressKeyValue.KeyPressType.PressAndRelease);

            }
        }

        private void ReleaseUnlockedKeys()
        {
            Log.Info("ReleaseUnlockedKeys called.");

            foreach (var key in keyStateService.KeyDownStates.Keys)
            {
                if (keyStateService.KeyDownStates[key].Value == KeyDownStates.Down)
                {
                    Log.DebugFormat("Releasing {0} key.", key);
                    keyStateService.KeyDownStates[key].Value = KeyDownStates.Up;
                    if (fireKeySelectionEvent != null) fireKeySelectionEvent(key);
                }
            }

            //This method is called by manual user actions so the shift key would be released if it was automatically set
            shiftStateSetAutomatically = false;
        }

        private void SwapLastTextChangeForSuggestion(int index)
        {
            Log.DebugFormat("SwapLastTextChangeForSuggestion called with index {0}", index);

            var suggestionIndex = (suggestionService.SuggestionsPage * suggestionService.SuggestionsPerPage) + index;
            if (suggestionService.Suggestions.Count > suggestionIndex)
            {
                if (!string.IsNullOrEmpty(lastProcessedText)
                    && lastProcessedText.Length > 1
                    && lastProcessedTextWasMultiKey)
                {
                    //We are swapping out a multi-key capture
                    var replacedText = lastProcessedText;
                    SwapText(lastProcessedText, suggestionService.Suggestions[suggestionIndex]);
                    var newSuggestions = suggestionService.Suggestions.ToList();
                    newSuggestions[suggestionIndex] = replacedText;
                    StoreSuggestions(newSuggestions);
                }
                else
                {
                    var inProgressWord = Text == null ? null : Text.InProgressWord(Text.Length);
                    if (!Settings.Default.SuggestNextWords || !lastProcessedTextWasSuggestion && !string.IsNullOrEmpty(inProgressWord) && Char.IsLetterOrDigit(inProgressWord.Last()))
                    {
                        //We are auto-completing a word with a suggestion
                        SwapText(inProgressWord, suggestionService.Suggestions[suggestionIndex]);
                        GenerateSuggestions(true);
                    }
                    else
                    {
                        //We are writing the first word or adding a whole word
                        ProcessText(suggestionService.Suggestions[suggestionIndex], false);
                        GenerateSuggestions(true);
                    }
                }
                suppressNextAutoSpace = false;
            }
        }

        private void SwapText(string textToSwapOut, string textToSwapIn)
        {
            Log.DebugFormat("SwapText called to swap '{0}' for '{1}'.", textToSwapOut, textToSwapIn);

            if (!string.IsNullOrEmpty(textToSwapOut)
                && !string.IsNullOrEmpty(textToSwapIn)
                && Text != null
                && Text.Length >= textToSwapOut.Length)
            {
                dictionaryService.DecrementEntryUsageCount(textToSwapOut);
                dictionaryService.IncrementEntryUsageCount(textToSwapIn);

                Text = string.Concat(Text.Substring(0, Text.Length - textToSwapOut.Length), textToSwapIn);

                var textHasSameRoot = textToSwapIn.StartsWith(textToSwapOut, StringComparison.Ordinal);
                if (!textHasSameRoot) //Only backspace the old word if it doesn't share the same root as the new word
                {
                    for (int i = 0; i < textToSwapOut.Length; i++)
                    {
                        PublishKeyPress(FunctionKeys.BackOne);
                    }
                }

                var publishText = textHasSameRoot ? textToSwapIn.Substring(textToSwapOut.Length) : textToSwapIn;
                foreach (char c in publishText)
                {
                    PublishKeyPress(c);
                }

                StoreLastProcessedText(textToSwapIn);
            }
        }

        private bool AutoAddSpace()
        {
            if (Settings.Default.AutoAddSpace
                && Text != null
                && Text.Any()
                && !suppressNextAutoSpace)
            {
                Log.Info("Publishing auto space and adding auto space to Text.");
                PublishKeyPress(' ');
                Text = string.Concat(Text, " ");
                return true;
            }

            return false;
        }

        private string ApplyModifierKeys(string textToModify)
        {
            //TODO Handle LeftAlt modified captures - LeftAlt+Code = unicode characters

            if (KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                keyStateService.KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.DebugFormat("A key which prevents text capture is down - modifying '{0}' to null.", textToModify.ToPrintableString());
                return null;
            }

            if (!string.IsNullOrEmpty(textToModify))
            {
                if (keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
                {
                    var modifiedText = textToModify.FirstCharToUpper();
                    Log.DebugFormat("LeftShift is on so modifying '{0}' to '{1}.", textToModify, modifiedText);
                    return modifiedText;
                }

                if (keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown)
                {
                    var modifiedText = textToModify.ToUpper(Settings.Default.KeyboardAndDictionaryLanguage.ToCultureInfo());
                    Log.DebugFormat("LeftShift is locked so modifying '{0}' to '{1}.", textToModify, modifiedText);
                    return modifiedText;
                }
            }

            return textToModify;
        }

        #endregion
    }
}