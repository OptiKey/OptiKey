using System;
using System.Collections.Generic;
using System.Linq;
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

                    StoreLastTextChange(null);
                    ClearSuggestions();

                    if (textChangedByBackMany)
                    {
                        AutoPressShiftIfAppropriate();
                    }

                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    break;

                case FunctionKeys.BackOne:
                    var backOneCount = string.IsNullOrEmpty(lastTextChange)
                        ? 1
                        : lastTextChange.Length;

                    var textChangedByBackOne = false;

                    if (!string.IsNullOrEmpty(Text))
                    {
                        if (Text.Length < backOneCount)
                        {
                            backOneCount = Text.Length; //Coallesce backCount if somehow the Text length is less
                        }

                        dictionaryService.DecrementEntryUsageCount(Text.Substring(Text.Length - backOneCount, backOneCount).Trim());

                        var textAfterBackOne = Text.Substring(0, Text.Length - backOneCount);
                        textChangedByBackOne = Text != textAfterBackOne;
                        Text = textAfterBackOne;
                    }

                    for (int i = 0; i < backOneCount; i++)
                    {
                        PublishKeyPress(FunctionKeys.BackOne);
                        ReleaseUnlockedKeys();
                    }

                    StoreLastTextChange(null);
                    ClearSuggestions();

                    if (textChangedByBackOne)
                    {
                        AutoPressShiftIfAppropriate();
                    }

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
                        ClearSuggestions();

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

            ClearSuggestions();
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

        #endregion

        #region Methods - private

        private void ProcessText(string capturedText)
        {
            Log.Debug(string.Format("Processing captured text '{0}'", capturedText.ConvertEscapedCharsToLiterals()));

            if (string.IsNullOrEmpty(capturedText)) return;

            //Suppress auto space if... 
            if (string.IsNullOrEmpty(lastTextChange) //we have no text change history
                || (lastTextChange.Length == 1 && capturedText.Length == 1) //we are capturing char by char (after 1st char)
                || (capturedText.Length == 1 && !char.IsLetter(capturedText.First())) //we have captured a single char which is not a letter
                || new[] { " ", "\n" }.Contains(lastTextChange)) //the current capture follows a space or newline
            {
                Log.Debug("Suppressing next auto space.");
                suppressNextAutoSpace = true;
            }

            //Modify the capture and apply to Text
            var modifiedCaptureText = ModifyCapturedText(capturedText);
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
                        modifiedCaptureText = ModifyCapturedText(capturedText);
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

                dictionaryService.IncrementEntryUsageCount(modifiedCaptureText);
                Text = string.Concat(Text, modifiedCaptureText);
            }

            //Publish each character (if SimulatingKeyStrokes), releasing on (but not locked) modifier keys as appropriate
            for (int index = 0; index < capturedText.Length; index++)
            {
                PublishKeyPress(capturedText[index],
                    modifiedCaptureText != null && modifiedCaptureText.Length == capturedText.Length
                        ? modifiedCaptureText[index]
                        : (char?)null);

                ReleaseUnlockedKeys();
            }

            if (!string.IsNullOrEmpty(modifiedCaptureText))
            {
                AutoPressShiftIfAppropriate();
                suppressNextAutoSpace = false;
            }

            StoreLastTextChange(modifiedCaptureText);
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
                                    publishService.PublishKeyDown(virtualKeyCode.Value);
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
                                publishService.PublishKeyDown(virtualKeyCode);
                            }
                            else
                            {
                                publishService.PublishKeyUp(virtualKeyCode);
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
                .Select(ModifyCapturedText)
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
                Log.Debug(string.Format("PublishKeyPress called with functionKey '{0}'.",  functionKey));

                var virtualKeyCode = functionKey.ToVirtualKeyCode();
                if (virtualKeyCode != null)
                {
                    publishService.PublishKeyPress(virtualKeyCode.Value);
                }
            }
        }

        private void PublishKeyPress(char character, char? modifiedCharacter)
        {
            if (keyboardService.KeyDownStates[KeyValues.SimulateKeyStrokesKey].Value.IsDownOrLockedDown())
            {
                Log.Debug(string.Format("PublishKeyPress called with character '{0}' and modified character '{1}'",
                    character.ConvertEscapedCharToLiteral(), 
                    modifiedCharacter == null ? null : modifiedCharacter.Value.ConvertEscapedCharToLiteral()));

                var virtualKeyCode = character.ToVirtualKeyCode();
                if (virtualKeyCode != null)
                {
                    publishService.PublishKeyPress(virtualKeyCode.Value);
                }
                else if (modifiedCharacter != null)
                {
                    publishService.PublishText(modifiedCharacter.ToString());
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

            if (!string.IsNullOrEmpty(lastTextChange))
            {
                var suggestionIndex = (suggestionService.SuggestionsPage * suggestionService.SuggestionsPerPage) + index;
                if (suggestionService.Suggestions.Count > suggestionIndex)
                {
                    var replacedText = lastTextChange;
                    SwapLastTextChangeForSuggestion(suggestionService.Suggestions[suggestionIndex]);
                    var newSuggestions = suggestionService.Suggestions.ToList();
                    newSuggestions[suggestionIndex] = replacedText;
                    StoreSuggestions(newSuggestions);
                }
            }
        }

        private void SwapLastTextChangeForSuggestion(string suggestion)
        {
            Log.Debug(string.Format("SwapLastTextChangeForSuggestion called with suggestion '{0}'", suggestion));

            if (!string.IsNullOrEmpty(lastTextChange)
                && !string.IsNullOrEmpty(suggestion))
            {
                dictionaryService.DecrementEntryUsageCount(lastTextChange);
                Text = Text.Substring(0, Text.Length - lastTextChange.Length);

                dictionaryService.IncrementEntryUsageCount(suggestion);
                Text = string.Concat(Text, suggestion);

                for (int i = 0; i < lastTextChange.Length; i++)
                {
                    PublishKeyPress(FunctionKeys.BackOne);
                }

                foreach (char c in suggestion)
                {
                    PublishKeyPress(c, null);
                }

                StoreLastTextChange(suggestion);
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
                PublishKeyPress(' ', null);
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

        private string ModifyCapturedText(string capturedText)
        {
            //TODO Handle LeftAlt modified captures - LeftAlt+Code = unicode characters

            if (KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                keyboardService.KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.Debug(string.Format("A key which prevents text capture is down - modifying '{0}' to null.", 
                    capturedText.ConvertEscapedCharsToLiterals()));
                return null;
            }

            if (!string.IsNullOrEmpty(capturedText))
            {
                if (keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
                {
                    var modifiedText = capturedText.FirstCharToUpper();
                    Log.Debug(string.Format("LeftShift is on so modifying '{0}' to '{1}.", capturedText, modifiedText));
                    return modifiedText;
                }

                if (keyboardService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown)
                {
                    var modifiedText = capturedText.ToUpper();
                    Log.Debug(string.Format("LeftShift is locked so modifying '{0}' to '{1}.", capturedText, modifiedText));
                    return modifiedText;
                }
            }

            return capturedText;
        }

        #endregion
    }
}