using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class OutputService : BindableBase, IOutputService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IKeyboardService keyboardService;
        private readonly ISuggestionService suggestionService;
        private readonly IPublishService publishService;
        private readonly IDictionaryService dictionaryService;

        private string text;
        private string lastTextChange;
        private bool suppressNextAutoSpace = true;
        
        #endregion

        #region Ctor

        public OutputService(
            IKeyboardService keyboardService,
            ISuggestionService suggestionService,
            IPublishService publishService,
            IDictionaryService dictionaryService)
        {
            this.keyboardService = keyboardService;
            this.suggestionService = suggestionService;
            this.publishService = publishService;
            this.dictionaryService = dictionaryService;

            ReactToPublishingStateChanges();
            ReactToPublishableKeyDownStateChanges();
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return text; }
            private set { SetProperty(ref text, value); }
        }

        #endregion

        #region Methods - IOutputService

        public void ProcessFunctionKey(FunctionKeys functionKey)
        {
            Log.Debug(string.Format("Processing captured function key '{0}'", functionKey));

            switch (functionKey)
            {
                case FunctionKeys.BackMany:
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

                    if (textChangedByBackMany)
                    {
                        AutoPressShiftIfAppropriate();
                    }

                    StoreLastTextChange(null);
                    GenerateAutoCompleteSuggestions();

                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    break;

                case FunctionKeys.BackOne:
                    var backOneCount = string.IsNullOrEmpty(lastTextChange)
                        ? 1 //Default to removing one character if no lastTextChange
                        : lastTextChange.Length;

                    var textChangedByBackOne = false;

                    if (!string.IsNullOrEmpty(Text))
                    {
                        if (Text.Length < backOneCount)
                        {
                            backOneCount = Text.Length; //Coallesce backCount if somehow the Text length is less
                        }

                        var textAfterBackOne = Text.Substring(0, Text.Length - backOneCount);
                        textChangedByBackOne = Text != textAfterBackOne;

                        if (backOneCount > 1)
                        {
                            //Removing more than one character - only decrement removed string
                            dictionaryService.DecrementEntryUsageCount(Text.Substring(Text.Length - backOneCount, backOneCount).Trim());
                        }
                        else if(!string.IsNullOrEmpty(lastTextChange)
                            && lastTextChange.Length == 1
                            && !Char.IsWhiteSpace(lastTextChange[0]))
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

                    if (textChangedByBackOne)
                    {
                        AutoPressShiftIfAppropriate();
                    }

                    StoreLastTextChange(null);
                    GenerateAutoCompleteSuggestions();

                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    break;

                case FunctionKeys.ClearScratchpad:
                    Text = null;
                    StoreLastTextChange(null);
                    ClearSuggestions();
                    AutoPressShiftIfAppropriate();

                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    break;

                case FunctionKeys.Suggestion1:
                    SwapLastTextChangeForSuggestion(0);
                    break;

                case FunctionKeys.Suggestion2:
                    SwapLastTextChangeForSuggestion(1);
                    break;

                case FunctionKeys.Suggestion3:
                    SwapLastTextChangeForSuggestion(2);
                    break;

                case FunctionKeys.Suggestion4:
                    SwapLastTextChangeForSuggestion(3);
                    break;

                case FunctionKeys.Suggestion5:
                    SwapLastTextChangeForSuggestion(4);
                    break;

                case FunctionKeys.Suggestion6:
                    SwapLastTextChangeForSuggestion(5);
                    break;

                default:
                    if (functionKey.ToVirtualKeyCode() != null)
                    {
                        //Key corresponds to physical keyboard key
                        GenerateAutoCompleteSuggestions();

                        //If the key cannot be pressed or locked down (these are handled elsewhere) then publish it and release unlocked keys.
                        var keyValue = new KeyValue { FunctionKey = functionKey };
                        if (!KeyValues.KeysWhichCanBePressedDown.Contains(keyValue)
                            && !KeyValues.KeysWhichCanBeLockedDown.Contains(keyValue))
                        {
                            PublishKeyPress(functionKey);
                            ReleaseUnlockedKeys();
                        }
                    }
                    break;
            }
        }

        public void ProcessSingleKeyText(string capturedText)
        {
            Log.Debug(string.Format("Processing captured text '{0}'", capturedText.ConvertEscapedCharsToLiterals()));
            ProcessText(capturedText);
        }

        public void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions)
        {
            Log.Debug(string.Format("Processing {0} captured multi-key selection results", 
                captureAndSuggestions != null ? captureAndSuggestions.Count : 0));

            if (captureAndSuggestions == null || !captureAndSuggestions.Any()) return;

            StoreSuggestions(
                ModifySuggestions(captureAndSuggestions.Count > 1
                    ? captureAndSuggestions.Skip(1).ToList()
                    : null));

            ProcessText(captureAndSuggestions.First());
        }

        public void LeftMouseButtonDown(Point point)
        {
            Log.Debug(string.Format("Pressing down left mouse at point '{0}'", point));
            publishService.MouseMouseToPoint(point);
            publishService.LeftMouseButtonDown();
        }

        public void LeftMouseButtonUp(Point point)
        {
            Log.Debug(string.Format("Releasing left mouse at point '{0}'", point));
            publishService.MouseMouseToPoint(point);
            publishService.LeftMouseButtonUp();
        }

        public void LeftMouseButtonClick(Point point)
        {
            Log.Debug(string.Format("Generating a left mouse click at point '{0}'", point));
            publishService.MouseMouseToPoint(point);
            publishService.LeftMouseButtonClick();
        }

        public void LeftMouseButtonDoubleClick(Point point)
        {
            Log.Debug(string.Format("Generating a left mouse double click at point '{0}'", point));
            publishService.MouseMouseToPoint(point);
            publishService.LeftMouseButtonDoubleClick();
        }

        public void MoveMouseTo(Point point)
        {
            Log.Debug(string.Format("Moving mouse to point '{0}'", point));
            publishService.MouseMouseToPoint(point);
        }

        public void RightMouseButtonClick(Point point)
        {
            Log.Debug(string.Format("Generating a right mouse click at point '{0}'", point));
            publishService.MouseMouseToPoint(point);
            publishService.RightMouseButtonClick();
        }

        public void ScrollMouseWheelUp(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating a vertical mouse scroll of {0} clicks up at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelUp(clicks);
        }

        public void ScrollMouseWheelUpAndLeft(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating vertical and horizontal mouse scrolls of {0} clicks up and left at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelUpAndLeft(clicks);
        }

        public void ScrollMouseWheelUpAndRight(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating vertical and horizontal mouse scrolls of {0} clicks up and right at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelUpAndRight(clicks);
        }

        public void ScrollMouseWheelDown(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating a vertical mouse scroll of {0} clicks down at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelDown(clicks);
        }

        public void ScrollMouseWheelDownAndLeft(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating vertical and horizontal mouse scrolls of {0} clicks down and left at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelDownAndLeft(clicks);
        }

        public void ScrollMouseWheelDownAndRight(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating vertical and horizontal mouse scrolls of {0} clicks down and right at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelDownAndRight(clicks);
        }

        public void ScrollMouseWheelLeft(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating a horizontal mouse scroll of {0} clicks left at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelLeft(clicks);
        }

        public void ScrollMouseWheelRight(int clicks, Point point)
        {
            Log.Debug(string.Format("Generating a horizontal mouse scroll of {0} clicks right at point '{1}'", clicks, point));
            publishService.MouseMouseToPoint(point);
            publishService.ScrollMouseWheelRight(clicks);
        }

        #endregion

        #region Methods - private

        private void ProcessText(string captureText)
        {
            Log.Debug(string.Format("Processing captured text '{0}'", captureText.ConvertEscapedCharsToLiterals()));

            if (string.IsNullOrEmpty(captureText)) return;

            //Suppress auto space if... 
            if (string.IsNullOrEmpty(lastTextChange) //we have no text change history
                || (lastTextChange.Length == 1 && captureText.Length == 1) //we are capturing char by char (after 1st char)
                || (captureText.Length == 1 && !char.IsLetter(captureText.First())) //we have captured a single char which is not a letter
                || new[] { " ", "\n" }.Contains(lastTextChange)) //the current capture follows a space or newline
            {
                Log.Debug("Suppressing next auto space.");
                suppressNextAutoSpace = true;
            }

            var textBeforeCaptureText = Text;

            //Modify the capture and apply to Text
            var modifiedCaptureText = ModifyText(captureText);
            if (!string.IsNullOrEmpty(modifiedCaptureText))
            {
                var spaceAdded = AutoAddSpace();
                if (spaceAdded)
                {
                    //Auto space added - recalc whether shift should be auto-pressed
                    var shiftPressed = AutoPressShiftIfAppropriate();

                    if (shiftPressed)
                    {
                        //LeftShift has been auto-pressed - re-apply modifiers to captured text and suggestions
                        modifiedCaptureText = ModifyText(captureText);
                        StoreSuggestions(ModifySuggestions(suggestionService.Suggestions));

                        //Ensure suggestions do not contain the modifiedText
                        if (!string.IsNullOrEmpty(modifiedCaptureText)
                            && suggestionService.Suggestions != null
                            && suggestionService.Suggestions.Contains(modifiedCaptureText))
                        {
                            suggestionService.Suggestions = suggestionService.Suggestions.Where(s => s != modifiedCaptureText).ToList();
                        }
                    }
                }

                Text = string.Concat(Text, modifiedCaptureText);
            }

            //Increment/decrement usage counts
            if (captureText.Length > 1)
            {
                dictionaryService.IncrementEntryUsageCount(captureText);
            }
            else if (captureText.Length == 1
                && !Char.IsWhiteSpace(captureText[0]))
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
            for (int index = 0; index < captureText.Length; index++)
            {
                PublishKeyPress(captureText[index],
                    modifiedCaptureText != null && modifiedCaptureText.Length == captureText.Length
                        ? modifiedCaptureText[index]
                        : (char?)null,
                        captureText.Length > 1); //Publish each character as TEXT (not key presses) if the capture is a multi-key capture. This preserves casing from the dictionary entry.

                ReleaseUnlockedKeys();
            }

            if (!string.IsNullOrEmpty(modifiedCaptureText))
            {
                AutoPressShiftIfAppropriate();
                suppressNextAutoSpace = false;
            }

            StoreLastTextChange(modifiedCaptureText);
            GenerateAutoCompleteSuggestions();
        }

        private void ReactToPublishingStateChanges()
        {
            keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].OnPropertyChanges(s => s.Value)
                .Subscribe(value =>
                {
                    if (value.IsDownOrLockedDown()) //Publishing has been turned on
                    {
                        publishService.ReleaseAllDownKeys();

                        foreach (var key in KeyValues.KeysWhichCanBePressedOrLockedDown)
                        {
                            if (keyboardService.KeyDownStates[key].Value.IsDownOrLockedDown()
                                && key.FunctionKey != null)
                            {
                                var virtualKeyCode = key.FunctionKey.Value.ToVirtualKeyCode();

                                if (virtualKeyCode != null)
                                {
                                    publishService.KeyDown(virtualKeyCode.Value);
                                }
                            }
                        }
                    }
                    else //Publishing has been turned off
                    {
                        publishService.ReleaseAllDownKeys();
                    }
                });
        }

        private void ReactToPublishableKeyDownStateChanges()
        {
            foreach (var key in KeyValues.KeysWhichCanBePressedOrLockedDown
                                    .Where(k => k.FunctionKey != null 
                                        && k.FunctionKey.Value.ToVirtualKeyCode() != null))
            {
                var keyCopy = key; //Access to foreach variable in modified

                keyboardService.KeyDownStates[key].OnPropertyChanges(s => s.Value)
                    .Subscribe(value =>
                    {
                        if (keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown())
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

        private void StoreLastTextChange(string textChange)
        {
            Log.Debug(string.Format("Storing last text change '{0}'", textChange));
            lastTextChange = textChange;
        }

        private void GenerateAutoCompleteSuggestions()
        {
            if (lastTextChange == null || lastTextChange.Length == 1) //Don't generate auto complete words if the last input was a multi key capture
            {
                var inProgressWord = Text == null ? null : Text.InProgressWord(Text.Length);

                if (!string.IsNullOrEmpty(inProgressWord))
                {
                    Log.DebugFormat("Generating auto complete suggestions from '{0}'.", inProgressWord);

                    var suggestions = dictionaryService.GetAutoCompleteSuggestions(inProgressWord)
                        .Take(Settings.Default.MaxDictionaryMatchesOrSuggestions)
                        .Select(de => de.Entry)
                        .ToList();

                    //Correctly case auto complete suggestions
                    suggestions = suggestions.Select(s => s.ToLower()).ToList(); //Start lower

                    //Then case each suggestion letter to match what has already been typed
                    for (var index = 0; index < inProgressWord.Length; index++)
                    {
                        if (Char.IsUpper(inProgressWord[index]))
                        {
                            int indexCopy = index;
                            suggestions = suggestions.Select(s =>
                            {
                                var prefix = s.Substring(0, indexCopy);
                                var upperLetter = s.Substring(indexCopy, 1).ToUpper();
                                var suffix = s.Length > indexCopy + 1
                                    ? s.Substring(indexCopy + 1, s.Length - (indexCopy + 1))
                                    : null;

                                return string.Concat(prefix, upperLetter, suffix);
                            })
                            .ToList();
                        }
                    }

                    //Finally case the rest of each suggestion based on whether the shift key is down, or locked down
                    suggestions = suggestions.Select(s =>
                    {
                        var prefix = s.Substring(0, inProgressWord.Length);
                        string suffix = null;

                        if (s.Length > inProgressWord.Length)
                        {
                            suffix = s.Substring(inProgressWord.Length, s.Length - inProgressWord.Length);
                                suffix = keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down
                                ? suffix.FirstCharToUpper()
                                : keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown
                                    ? suffix.ToUpper()
                                    : suffix;
                        }

                        return string.Concat(prefix, suffix);
                    }).ToList();
                    

                    suggestionService.Suggestions = suggestions;
                    return;
                }

                ClearSuggestions();
            }
        }

        private void ClearSuggestions()
        {
            Log.Debug("Clearing suggestions.");
            suggestionService.Suggestions = null;
        }

        private List<string> ModifySuggestions(List<string> suggestions)
        {
            Log.Debug(string.Format("Modifying {0} suggestions.", suggestions != null ? suggestions.Count : 0));

            if(suggestions == null || !suggestions.Any()) return null;

            var modifiedSuggestions = suggestions
                .Select(ModifyText)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            Log.Debug(string.Format("After applying modifiers there are {0} modified suggestions.", modifiedSuggestions.Count));

            return modifiedSuggestions.Any() ? modifiedSuggestions : null;
        }

        private void StoreSuggestions(List<string> suggestions)
        {
            Log.Debug(string.Format("Storing {0} suggestions.", suggestions != null ? suggestions.Count : 0));

            suggestionService.Suggestions = suggestions != null && suggestions.Any()
                ? suggestions
                : null;
        }
        
        private void PublishKeyPress(FunctionKeys functionKey)
        {
            if (keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown())
            {
                Log.Debug(string.Format("KeyDownUp called with functionKey '{0}'.",  functionKey));

                var virtualKeyCode = functionKey.ToVirtualKeyCode();
                if (virtualKeyCode != null)
                {
                    publishService.KeyDownUp(virtualKeyCode.Value);
                }
            }
        }

        private void PublishKeyPress(char character, char? modifiedCharacter, bool publishModifiedCharacterAsText)
        {
            if (keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown())
            {
                Log.Debug(string.Format("KeyDownUp called with character '{0}' and modified character '{1}'",
                    character.ConvertEscapedCharToLiteral(), 
                    modifiedCharacter == null ? null : modifiedCharacter.Value.ConvertEscapedCharToLiteral()));

                var virtualKeyCode = character.ToVirtualKeyCode();
                if (virtualKeyCode != null
                    && !publishModifiedCharacterAsText)
                {
                    publishService.KeyDownUp(virtualKeyCode.Value);
                }
                else if (modifiedCharacter != null)
                {
                    publishService.TypeText(modifiedCharacter.ToString());
                }
            }
        }

        private void ReleaseUnlockedKeys()
        {
            Log.Debug("ReleaseUnlockedKeys called.");

            foreach (var key in keyboardService.KeyDownStates.Keys)
            {
                if (keyboardService.KeyDownStates[key].Value == KeyDownStates.Down)
                {
                    Log.Debug(string.Format("Releasing {0} key.", key));
                    keyboardService.KeyDownStates[key].Value = KeyDownStates.Up;
                }
            }
        }

        private void SwapLastTextChangeForSuggestion(int index)
        {
            Log.Debug(string.Format("SwapLastTextChangeForSuggestion called with index {0}", index));

            var suggestionIndex = (suggestionService.SuggestionsPage * suggestionService.SuggestionsPerPage) + index;
            if (suggestionService.Suggestions.Count > suggestionIndex)
            {
                if (!string.IsNullOrEmpty(lastTextChange)
                    && lastTextChange.Length > 1)
                {
                    var replacedText = lastTextChange;
                    SwapText(lastTextChange, suggestionService.Suggestions[suggestionIndex]);
                    var newSuggestions = suggestionService.Suggestions.ToList();
                    newSuggestions[suggestionIndex] = replacedText;
                    StoreSuggestions(newSuggestions);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Text))
                    {
                        var inProgressWord = Text.InProgressWord(Text.Length);
                        if (!string.IsNullOrEmpty(inProgressWord))
                        {
                            SwapText(inProgressWord, suggestionService.Suggestions[suggestionIndex]);
                            var newSuggestions = suggestionService.Suggestions.ToList();
                            newSuggestions.RemoveAt(suggestionIndex);
                            StoreSuggestions(newSuggestions);
                        }
                    }
                }
            }
        }

        private void SwapText(string textToSwapOut, string textToSwapIn)
        {
            Log.Debug(string.Format("SwapText called to swap '{0}' for '{1}'.", textToSwapOut, textToSwapIn));

            if (!string.IsNullOrEmpty(textToSwapOut)
                && !string.IsNullOrEmpty(textToSwapIn)
                && Text != null
                && Text.Length >= textToSwapOut.Length)
            {
                dictionaryService.DecrementEntryUsageCount(textToSwapOut);
                dictionaryService.IncrementEntryUsageCount(textToSwapIn);

                Text = string.Concat(Text.Substring(0, Text.Length - textToSwapOut.Length), textToSwapIn);

                for (int i = 0; i < textToSwapOut.Length; i++)
                {
                    PublishKeyPress(FunctionKeys.BackOne);
                }

                foreach (char c in textToSwapIn)
                {
                    PublishKeyPress(c, c, true); //Character has already been modified, so pass 'c' for both args
                }

                StoreLastTextChange(textToSwapIn);
            }
        }

        private bool AutoAddSpace()
        {
            if (Settings.Default.AutoAddSpace
                && Text != null
                && Text.Any()
                && !suppressNextAutoSpace)
            {
                Log.Debug("Publishing auto space and adding auto space to Text.");
                PublishKeyPress(' ', ' ', true); //It's a space
                Text = string.Concat(Text, " ");
                return true;
            }

            return false;
        }

        private bool AutoPressShiftIfAppropriate()
        {
            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
            {
                Log.Debug("Auto-pressing shift.");
                keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Down;
                return true;
            }

            return false;
        }

        private string ModifyText(string textToModify)
        {
            //TODO Handle LeftAlt modified captures - LeftAlt+Code = unicode characters

            if (KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                keyboardService.KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.Debug(string.Format("A key which prevents text capture is down - modifying '{0}' to null.", 
                    textToModify.ConvertEscapedCharsToLiterals()));
                return null;
            }

            if (!string.IsNullOrEmpty(textToModify))
            {
                if (keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
                {
                    var modifiedText = textToModify.FirstCharToUpper();
                    Log.Debug(string.Format("LeftShift is on so modifying '{0}' to '{1}.", textToModify, modifiedText));
                    return modifiedText;
                }

                if (keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown)
                {
                    var modifiedText = textToModify.ToUpper();
                    Log.Debug(string.Format("LeftShift is locked so modifying '{0}' to '{1}.", textToModify, modifiedText));
                    return modifiedText;
                }
            }

            return textToModify;
        }

        #endregion
    }
}