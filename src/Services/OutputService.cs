using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;
using VirtualKeyCode = JuliusSweetland.ETTA.Native.Enums.VirtualKeyCode;

namespace JuliusSweetland.ETTA.Services
{
    public class OutputService : BindableBase, IOutputService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IKeyboardStateManager keyboardStateManager;
        private readonly IPublishService publishService;
        private readonly IDictionaryService dictionaryService;

        private string text;
        private string lastTextChange;
        private bool suppressNextAutoSpace = true;
        
        #endregion

        #region Ctor

        public OutputService(
            IKeyboardStateManager keyboardStateManager,
            IPublishService publishService,
            IDictionaryService dictionaryService)
        {
            this.keyboardStateManager = keyboardStateManager;
            this.publishService = publishService;
            this.dictionaryService = dictionaryService;
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

        public void ProcessCapture(FunctionKeys functionKey)
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
                        PublishKeyStroke(FunctionKeys.BackOne);
                        ReleaseUnlockedModifiers();
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
                        PublishKeyStroke(FunctionKeys.BackOne);
                        ReleaseUnlockedModifiers();
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
                    PublishKeyStroke(functionKey); //No Text modification from any other function key - just publish key stroke if possible
                    break;
            }
        }

        public void ProcessCapture(string capturedText)
        {
            Log.Debug(string.Format("Processing captured text '{0}'", capturedText));

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
                        StoreSuggestions(ModifySuggestions(keyboardStateManager.Suggestions));

                        //Ensure suggestions do not contain the modifiedText
                        if (!string.IsNullOrEmpty(modifiedCaptureText)
                            && keyboardStateManager.Suggestions != null
                            && keyboardStateManager.Suggestions.Contains(modifiedCaptureText))
                        {
                            keyboardStateManager.Suggestions = keyboardStateManager.Suggestions.Where(s => s != modifiedCaptureText).ToList();
                        }
                    }
                }

                dictionaryService.IncrementEntryUsageCount(modifiedCaptureText);
                Text = string.Concat(Text, modifiedCaptureText);
            }

            //Publish each character (if publishing), releasing on (but not locked) modifier keys as appropriate
            for (int index = 0; index < capturedText.Length; index++)
            {
                PublishKeyStroke(capturedText[index], 
                    modifiedCaptureText != null && modifiedCaptureText.Length == capturedText.Length
                        ? modifiedCaptureText[index]
                        : (char?)null);

                ReleaseUnlockedModifiers();
            }

            StoreLastTextChange(modifiedCaptureText);
            AutoPressShiftIfAppropriate();
            suppressNextAutoSpace = false;
        }

        public void ProcessCapture(List<string> captureAndSuggestions)
        {
            Log.Debug(string.Format("Processing {0} captured multi-key selection results", 
                captureAndSuggestions != null ? captureAndSuggestions.Count : 0));

            if (captureAndSuggestions == null || !captureAndSuggestions.Any()) return;

            StoreSuggestions(
                ModifySuggestions(captureAndSuggestions.Count > 1
                    ? captureAndSuggestions.Skip(1).ToList()
                    : null));

            ProcessCapture(captureAndSuggestions.First());
        }

        #endregion

        #region Methods - private

        private void StoreLastTextChange(string textChange)
        {
            Log.Debug(string.Format("Storing last text change '{0}'", textChange));
            lastTextChange = textChange;
        }

        private void ClearSuggestions()
        {
            Log.Debug("Clearing suggestions.");
            keyboardStateManager.Suggestions = null;
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

            keyboardStateManager.Suggestions = suggestions != null && suggestions.Any()
                ? suggestions
                : null;
        }
        
        private void PublishKeyStroke(FunctionKeys functionKey)
        {
            if (keyboardStateManager.KeyDownStates[KeyValues.PublishKey].Value.IsDownOrLockedDown())
            {
                Log.Debug(string.Format("PublishKeyStroke called with functionKey '{0}'.",  functionKey));

                var virtualKeyCodeSet = functionKey.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    PublishModifiedVirtualKeyCodeSet(virtualKeyCodeSet.Value);
                }
            }
        }

        private void PublishKeyStroke(char character, char? modifiedCharacter)
        {
            if (keyboardStateManager.KeyDownStates[KeyValues.PublishKey].Value.IsDownOrLockedDown())
            {
                Log.Debug(string.Format("PublishKeyStroke called with character '{0}' and modified character '{1}'", character, modifiedCharacter));

                var virtualKeyCodeSet = character.ToVirtualKeyCodeSet();
                if (virtualKeyCodeSet != null)
                {
                    PublishModifiedVirtualKeyCodeSet(virtualKeyCodeSet.Value);
                }
                else if (modifiedCharacter != null)
                {
                    publishService.PublishText(modifiedCharacter.ToString());
                }
            }
        }

        private void PublishModifiedVirtualKeyCodeSet(VirtualKeyCodeSet virtualKeyCodeSet)
        {
            Log.Debug(string.Format("PublishModifiedVirtualKeyCodeSet called with virtualKeyCodeSet '{0}'", virtualKeyCodeSet));

            if (virtualKeyCodeSet.ModifierKeyCodes == null)
            {
                virtualKeyCodeSet.ModifierKeyCodes = new List<VirtualKeyCode>();
            }

            var altVirtualKeyCode = FunctionKeys.LeftAlt.ToVirtualKeyCodeSet().Value.KeyCodes.First();
            if (keyboardStateManager.KeyDownStates[KeyValues.LeftAltKey].Value.IsDownOrLockedDown()
                && !virtualKeyCodeSet.ModifierKeyCodes.Contains(altVirtualKeyCode))
            {
                virtualKeyCodeSet.ModifierKeyCodes.Add(altVirtualKeyCode);
            }

            var ctrlVirtualKeyCode = FunctionKeys.LeftCtrl.ToVirtualKeyCodeSet().Value.KeyCodes.First();
            if (keyboardStateManager.KeyDownStates[KeyValues.LeftCtrlKey].Value.IsDownOrLockedDown()
                && !virtualKeyCodeSet.ModifierKeyCodes.Contains(ctrlVirtualKeyCode))
            {
                virtualKeyCodeSet.ModifierKeyCodes.Add(ctrlVirtualKeyCode);
            }

            var shiftVirtualKeyCode = FunctionKeys.LeftShift.ToVirtualKeyCodeSet().Value.KeyCodes.First();
            if (keyboardStateManager.KeyDownStates[KeyValues.LeftShiftKey].Value.IsDownOrLockedDown()
                && !virtualKeyCodeSet.ModifierKeyCodes.Contains(shiftVirtualKeyCode))
            {
                virtualKeyCodeSet.ModifierKeyCodes.Add(shiftVirtualKeyCode);
            }

            publishService.PublishModifiedKeyStroke(virtualKeyCodeSet);
        }

        private void ReleaseUnlockedModifiers()
        {
            Log.Debug("ReleaseUnlockedModifiers called.");

            if (keyboardStateManager.KeyDownStates[KeyValues.LeftAltKey].Value == KeyDownStates.Down)
            {
                Log.Debug("Releasing LeftAlt key.");

                keyboardStateManager.KeyDownStates[KeyValues.LeftAltKey].Value = KeyDownStates.Up;
            }

            if (keyboardStateManager.KeyDownStates[KeyValues.LeftCtrlKey].Value == KeyDownStates.Down)
            {
                Log.Debug("Releasing LeftCtrl key.");

                keyboardStateManager.KeyDownStates[KeyValues.LeftCtrlKey].Value = KeyDownStates.Up;
            }

            if (keyboardStateManager.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
            {
                Log.Debug("Releasing LeftShift key.");

                keyboardStateManager.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Up;
            }
        }

        private void SwapLastTextChangeForSuggestion(int index)
        {
            Log.Debug(string.Format("SwapLastTextChangeForSuggestion called with index {0}", index));

            if (!string.IsNullOrEmpty(lastTextChange))
            {
                var suggestionIndex = (keyboardStateManager.SuggestionsPage * keyboardStateManager.SuggestionsPerPage) + index;
                if (keyboardStateManager.Suggestions.Count > suggestionIndex)
                {
                    var replacedText = lastTextChange;
                    SwapLastTextChangeForSuggestion(keyboardStateManager.Suggestions[suggestionIndex]);
                    var newSuggestions = keyboardStateManager.Suggestions.ToList();
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
                    PublishKeyStroke(FunctionKeys.BackOne);
                }

                foreach (char c in suggestion)
                {
                    PublishKeyStroke(c, null);
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
                PublishKeyStroke(' ', null);
                Text = string.Concat(Text, " ");
                return true;
            }

            return false;
        }

        private bool AutoPressShiftIfAppropriate()
        {
            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && keyboardStateManager.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
            {
                Log.Debug("Auto-pressing shift.");
                keyboardStateManager.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Down;
                return true;
            }

            return false;
        }

        private string ModifyCapturedText(string capturedText)
        {
            //TODO Handle LeftAlt modified captures - LeftAlt+Code = unicode characters

            if (KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                keyboardStateManager.KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.Debug(string.Format("A key which prevents text capture is down - modifying '{0}' to null.", capturedText));
                return null;
            }

            if (!string.IsNullOrEmpty(capturedText))
            {
                if (keyboardStateManager.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
                {
                    var modifiedText = capturedText.FirstCharToUpper();
                    Log.Debug(string.Format("LeftShift is on so modifying '{0}' to '{1}.", capturedText, modifiedText));
                    return modifiedText;
                }

                if (keyboardStateManager.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown)
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