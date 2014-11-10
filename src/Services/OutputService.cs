using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.UI.Controls;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Services
{
    public class OutputService : BindableBase, IOutputService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IKeyboardStateManager keyboardStateManager;
        private readonly IPublishService publishService;

        private string lastTextChange;
        private bool suppressNextAutoSpace = true;
        
        #endregion

        #region Ctor

        public OutputService(
            IKeyboardStateManager keyboardStateManager,
            IPublishService publishService)
        {
            this.keyboardStateManager = keyboardStateManager;
            this.publishService = publishService;

            //TESTING START
            //Text = "This is some test output. I will make it arbitrarily long so we can see what is going on.";

            //Observable.Interval(TimeSpan.FromMilliseconds(500))
            //    .ObserveOnDispatcher()
            //    .Subscribe(l => Text = Text + " " + l);
            //TESTING END

            //Auto capitalise - when previous text is ". ", "! ", "? ", "\n". Backspace removes auto capitalise if not manual.
            //Auto add to dictionary
            //Clear suggestions (unless swapping suggestions)
            //Auto complete word
            //Publish keys (need to reset physical keyboard state on ctor or Publish setting to true)
        }

        #endregion

        #region Properties

        private string text;
        public string Text
        {
            get { return text; }
            private set { SetProperty(ref text, value); }
        }

        #endregion

        #region Methods - IOutputService

        public void ProcessCapture(FunctionKeys functionKey)
        {
            switch (functionKey)
            {
                case FunctionKeys.BackMany:
                    var backManyCount = Text.CountBackToLastCharCategoryBoundary();
                    
                    Text = Text.Substring(0, Text.Length - backManyCount);
                    
                    for (int i = 0; i < backManyCount; i++)
                    {
                        PublishKeyStroke(FunctionKeys.BackOne, null);
                        ReleaseUnlockedModifiers();
                    }

                    StoreLastTextChange(null);
                    StoreSuggestions(null);
                    AutoPressShiftIfAppropriate();
                    suppressNextAutoSpace = true;
                    break;

                case FunctionKeys.BackOne:
                    var backOneCount = string.IsNullOrEmpty(lastTextChange)
                        ? 1
                        : lastTextChange.Length;

                    if (!string.IsNullOrEmpty(Text))
                    {
                        if (Text.Length < backOneCount)
                        {
                            backOneCount = Text.Length; //Coallesce backCount if somehow the Text length is less
                        }

                        Text = Text.Substring(0, Text.Length - backOneCount);

                        for (int i = 0; i < backOneCount; i++)
                        {
                            PublishKeyStroke(FunctionKeys.BackOne, null);
                            ReleaseUnlockedModifiers();
                        }
                    }

                    StoreLastTextChange(null);
                    StoreSuggestions(null);
                    AutoPressShiftIfAppropriate();
                    suppressNextAutoSpace = true;
                    break;

                case FunctionKeys.ClearOutput:
                    Text = null;
                    StoreLastTextChange(null);
                    StoreSuggestions(null);
                    AutoPressShiftIfAppropriate();
                    suppressNextAutoSpace = true;
                    break;

                case FunctionKeys.Suggestion1:
                    SwapLastCaptureForSuggestion(0);
                    break;

                case FunctionKeys.Suggestion2:
                    SwapLastCaptureForSuggestion(1);
                    break;

                case FunctionKeys.Suggestion3:
                    SwapLastCaptureForSuggestion(2);
                    break;

                case FunctionKeys.Suggestion4:
                    SwapLastCaptureForSuggestion(3);
                    break;

                case FunctionKeys.Suggestion5:
                    SwapLastCaptureForSuggestion(4);
                    break;

                case FunctionKeys.Suggestion6:
                    SwapLastCaptureForSuggestion(5);
                    break;

                default:
                    //No Text modification from any other function key
                    PublishKeyStroke(functionKey, null);
                    ReleaseUnlockedModifiers();
                    StoreLastTextChange(null);
                    StoreSuggestions(null);
                    break;
            }
        }

        public void ProcessCapture(string textCapture)
        {
            if (string.IsNullOrEmpty(textCapture)) return;

            //Suppress auto space if... 
            if (string.IsNullOrEmpty(lastTextChange) //we have no text change history
                || (lastTextChange.Length == 1 && textCapture.Length == 1) //we are capturing char by char (after 1st char)
                || (textCapture.Length == 1 && !char.IsLetter(textCapture.First())) //we have captured a single char which is not a letter
                || new[] { " ", "\n" }.Contains(lastTextChange)) //the current capture follows a space or newline
            {
                suppressNextAutoSpace = true;
            }

            //Modify the capture and apply to Text
            var modifiedText = ModifyCapturedText(textCapture);
            if (!string.IsNullOrEmpty(modifiedText))
            {
                AutoAddSpace();
                Text = string.Concat(Text, modifiedText);
            }

            //Publish each character (if publishing), releasing on, but unlocked modifier keys as appropriate
            foreach (char c in textCapture)
            {
                PublishKeyStroke(null, c);
                ReleaseUnlockedModifiers();
            }

            StoreLastTextChange(modifiedText);
            AutoPressShiftIfAppropriate();
            suppressNextAutoSpace = false;
        }

        public void ProcessCapture(List<string> captureAndSuggestions)
        {
            if (captureAndSuggestions == null || !captureAndSuggestions.Any()) return;

            var bestMatch = captureAndSuggestions.First();
            ProcessCapture(bestMatch);

            var suggestions = captureAndSuggestions.Count > 1
                ? captureAndSuggestions.Skip(1).ToList()
                : null;

            if (suggestions != null
                && suggestions.Any())
            {
                StoreSuggestions(suggestions
                    .Select(ModifyCapturedText) //Apply modifiers to suggestion text
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList());
            }
            else
            {
                StoreSuggestions(null);
            }
        }

        #endregion

        #region Methods - private

        private void StoreLastTextChange(string text)
        {
            lastTextChange = text;
        }

        private void StoreSuggestions(List<string> suggestions)
        {
            keyboardStateManager.Suggestions = suggestions != null && suggestions.Any()
                ? suggestions
                : null;
        }

        private void PublishKeyStroke(FunctionKeys? functionKey, char? character)
        {
            if (Settings.Default.PublishingKeys)
            {
                if (functionKey != null)
                {
                    var virtualKeyCodeSet = functionKey.Value.ToVirtualKeyCodeSet();
                    if (virtualKeyCodeSet != null)
                    {
                        PublishModifiedVirtualKeyCodeSet(virtualKeyCodeSet.Value);
                    }
                }

                if (character != null)
                {
                    var virtualKeyCodeSet = character.Value.ToVirtualKeyCodeSet();
                    if (virtualKeyCodeSet != null)
                    {
                        PublishModifiedVirtualKeyCodeSet(virtualKeyCodeSet.Value);
                    }
                    else
                    {
                        publishService.PublishText(character.ToString());
                    }
                }
            }
        }

        private void PublishModifiedVirtualKeyCodeSet(VirtualKeyCodeSet virtualKeyCodeSet)
        {
            if (virtualKeyCodeSet.ModifierKeyCodes == null)
            {
                virtualKeyCodeSet.ModifierKeyCodes = new List<VirtualKeyCode>();
            }

            var altVirtualKeyCode = FunctionKeys.Alt.ToVirtualKeyCodeSet().Value.KeyCodes.First();
            if (keyboardStateManager.KeyDownStates[KeyValues.AltKey].Value.IsOnOrLock()
                && !virtualKeyCodeSet.ModifierKeyCodes.Contains(altVirtualKeyCode))
            {
                virtualKeyCodeSet.ModifierKeyCodes.Add(altVirtualKeyCode);
            }

            var ctrlVirtualKeyCode = FunctionKeys.Ctrl.ToVirtualKeyCodeSet().Value.KeyCodes.First();
            if (keyboardStateManager.KeyDownStates[KeyValues.CtrlKey].Value.IsOnOrLock()
                && !virtualKeyCodeSet.ModifierKeyCodes.Contains(ctrlVirtualKeyCode))
            {
                virtualKeyCodeSet.ModifierKeyCodes.Add(ctrlVirtualKeyCode);
            }

            var shiftVirtualKeyCode = FunctionKeys.Shift.ToVirtualKeyCodeSet().Value.KeyCodes.First();
            if (keyboardStateManager.KeyDownStates[KeyValues.ShiftKey].Value.IsOnOrLock()
                && !virtualKeyCodeSet.ModifierKeyCodes.Contains(shiftVirtualKeyCode))
            {
                virtualKeyCodeSet.ModifierKeyCodes.Add(shiftVirtualKeyCode);
            }

            publishService.PublishModifiedKeyStroke(virtualKeyCodeSet);
        }

        private void ReleaseUnlockedModifiers()
        {
            if (keyboardStateManager.KeyDownStates[KeyValues.AltKey].Value == KeyDownStates.On)
            {
                keyboardStateManager.KeyDownStates[KeyValues.AltKey].Value = KeyDownStates.Off;
            }

            if (keyboardStateManager.KeyDownStates[KeyValues.CtrlKey].Value == KeyDownStates.On)
            {
                keyboardStateManager.KeyDownStates[KeyValues.CtrlKey].Value = KeyDownStates.Off;
            }

            if (keyboardStateManager.KeyDownStates[KeyValues.ShiftKey].Value == KeyDownStates.On)
            {
                keyboardStateManager.KeyDownStates[KeyValues.ShiftKey].Value = KeyDownStates.Off;
            }
        }

        private void SwapLastCaptureForSuggestion(int index)
        {
            if (!string.IsNullOrEmpty(lastTextChange))
            {
                var suggestionIndex = (keyboardStateManager.SuggestionsPage * keyboardStateManager.SuggestionsPerPage) + index;
                if (keyboardStateManager.Suggestions.Count > suggestionIndex)
                {
                    var replacedText = lastTextChange;
                    SwapLastCaptureForSuggestion(keyboardStateManager.Suggestions[suggestionIndex]);
                    var newSuggestions = keyboardStateManager.Suggestions.ToList();
                    newSuggestions[suggestionIndex] = replacedText;
                    StoreSuggestions(newSuggestions);
                }
            }
        }

        private void SwapLastCaptureForSuggestion(string suggestion)
        {
            if (!string.IsNullOrEmpty(lastTextChange)
                && !string.IsNullOrEmpty(suggestion))
            {
                Text = Text.Substring(0, Text.Length - lastTextChange.Length);
                Text = string.Concat(Text, suggestion);

                for (int i = 0; i < lastTextChange.Length; i++)
                {
                    PublishKeyStroke(FunctionKeys.BackOne, null);
                    ReleaseUnlockedModifiers();
                }

                foreach (char c in suggestion)
                {
                    PublishKeyStroke(null, c);
                    ReleaseUnlockedModifiers();
                }

                StoreLastTextChange(suggestion);
            }
        }

        private void AutoAddSpace()
        {
            if (Settings.Default.AutoAddSpace
                && Text != null
                && Text.Any()
                && !suppressNextAutoSpace)
            {
                PublishKeyStroke(null, ' ');
                Text = string.Concat(Text, " ");
            }
        }

        private void AutoPressShiftIfAppropriate()
        {
            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && keyboardStateManager.KeyDownStates[KeyValues.ShiftKey].Value == KeyDownStates.Off)
            {
                keyboardStateManager.KeyDownStates[KeyValues.ShiftKey].Value = KeyDownStates.On;
            }
        }

        private string ModifyCapturedText(string capture)
        {
            if (keyboardStateManager.KeyDownStates[KeyValues.AltKey].Value.IsOnOrLock())
            {
                //TODO Handle Alt modified captures - Alt+Code = unicode characters
                return null;
            }

            if (keyboardStateManager.KeyDownStates[KeyValues.CtrlKey].Value.IsOnOrLock())
            {
                return null;
            }

            if (!string.IsNullOrEmpty(capture))
            {
                if (keyboardStateManager.KeyDownStates[KeyValues.ShiftKey].Value == KeyDownStates.On)
                {
                    return capture.FirstCharToUpper();
                }

                if (keyboardStateManager.KeyDownStates[KeyValues.ShiftKey].Value == KeyDownStates.Lock)
                {
                    return capture.ToUpper();
                }
            }

            return capture;
        }

        #endregion
    }
}