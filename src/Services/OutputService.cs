using System.Collections.Generic;
using System.Linq;
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
        private bool suppressAutoSpace = true;
        
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

        public void ProcessCapture(FunctionKeys? functionKey, string chars, List<string> suggestions)
        {
            if (functionKey == null && string.IsNullOrEmpty(chars)) return;

            bool calculateShiftState = false;

            if (functionKey != null)
            {
                switch (functionKey.Value)
                {
                    case FunctionKeys.BackMany:
                        ProcessBackMany();
                        
                        lastTextChange = null;
                        keyboardStateManager.Suggestions = null;
                        suppressAutoSpace = true;
                        calculateShiftState = true;
                        break;

                    case FunctionKeys.BackOne:
                        ProcessBackOne();

                        lastTextChange = null;
                        keyboardStateManager.Suggestions = null;
                        suppressAutoSpace = true;
                        calculateShiftState = true;
                        break;

                    case FunctionKeys.ClearOutput:
                        Text = null;

                        lastTextChange = null;
                        keyboardStateManager.Suggestions = null;
                        suppressAutoSpace = true;
                        calculateShiftState = true;
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
                        PublishKey(functionKey.Value, null);
                        ReleaseUnlockedModifiers();
                        lastTextChange = null;
                        keyboardStateManager.Suggestions = null;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(chars))
            {
                //Suppress auto space if... 
                if (string.IsNullOrEmpty(lastTextChange) //we have no text change history
                    || (lastTextChange.Length == 1 && chars.Length == 1) //we are capturing char by char (after 1st char)
                    || (chars.Length == 1 && !char.IsLetter(chars.First())) //we have captured a single char which is not a letter
                    || new[] { " ", "\n" }.Contains(lastTextChange) //the current capture follows a space or newline
                    )
                {
                    suppressAutoSpace = true;
                }

                var modifiedChars = ModifyCapturedText(chars);
                
                //Apply modifiers to suggestions also (if any) and set Suggestions property
                if (suggestions != null
                    && suggestions.Any())
                {
                    var modifiedSuggestions = suggestions
                        .Select(ModifyCapturedText)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                    keyboardStateManager.Suggestions = modifiedSuggestions.Any()
                            ? modifiedSuggestions
                            : null;
                }
                else
                {
                    keyboardStateManager.Suggestions = null;
                }

                if (!string.IsNullOrEmpty(modifiedChars))
                {
                    AutoAddSpace();
                    Text = string.Concat(Text, modifiedChars);
                }

                foreach (char c in chars)
                {
                    PublishKey(null, c);
                    ReleaseUnlockedModifiers();
                }
                
                lastTextChange = modifiedChars;
                suppressAutoSpace = false;
                calculateShiftState = true;
            }

            if (calculateShiftState)
            {
                AutoPressShiftIfAppropriate();
            }
        }

        #endregion

        #region Methods - private

        private void PublishKey(FunctionKeys? functionKey, char? character)
        {
            if (Settings.Default.PublishingKeys)
            {
                publishService.KeyPress(functionKey, character);
            }
        }

        private void ReleaseUnlockedModifiers()
        {
            if (keyboardStateManager.KeyDownStates[KeyValueKeys.ShiftKey].Value == KeyDownStates.On)
            {
                keyboardStateManager.KeyDownStates[KeyValueKeys.ShiftKey].Value = KeyDownStates.Off;
            }

            if (keyboardStateManager.KeyDownStates[KeyValueKeys.AltKey].Value == KeyDownStates.On)
            {
                keyboardStateManager.KeyDownStates[KeyValueKeys.AltKey].Value = KeyDownStates.Off;
            }

            if (keyboardStateManager.KeyDownStates[KeyValueKeys.CtrlKey].Value == KeyDownStates.On)
            {
                keyboardStateManager.KeyDownStates[KeyValueKeys.CtrlKey].Value = KeyDownStates.Off;
            }
        }

        private void ProcessBackOne()
        {
            if (keyboardStateManager.KeyDownStates[KeyValueKeys.AltKey].Value.IsOnOrLock()
                || keyboardStateManager.KeyDownStates[KeyValueKeys.CtrlKey].Value.IsOnOrLock())
            {
                //Ctrl/Alt modifiers are on - we will publish and release modifiers, but not modify Text
                PublishKey(FunctionKeys.BackOne, null);
                ReleaseUnlockedModifiers();
            }
            else
            {
                var backCount = 1;
                if (!string.IsNullOrEmpty(lastTextChange))
                {
                    backCount = lastTextChange.Length;
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    if (Text.Length < backCount)
                    {
                        backCount = Text.Length; //Coallesce backCount if somehow the Text length is less
                    }

                    Text = Text.Substring(0, Text.Length - backCount);

                    for (int i = 0; i < backCount; i++)
                    {
                        PublishKey(FunctionKeys.BackOne, null);
                        ReleaseUnlockedModifiers();
                    }
                }
            }
        }

        private void ProcessBackMany()
        {
            if (keyboardStateManager.KeyDownStates[KeyValueKeys.AltKey].Value.IsOnOrLock()
                || keyboardStateManager.KeyDownStates[KeyValueKeys.CtrlKey].Value.IsOnOrLock())
            {
                //Ctrl/Alt modifiers are on - we will publish and release modifiers, but not modify Text
                PublishKey(FunctionKeys.BackOne, null);
                ReleaseUnlockedModifiers();
            }
            else
            {
                var backCount = Text.CountBackToLastCharCategoryBoundary();

                Text = Text.Substring(0, Text.Length - backCount);

                for (int i = 0; i < backCount; i++)
                {
                    PublishKey(FunctionKeys.BackOne, null);
                    ReleaseUnlockedModifiers();
                }
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
                    keyboardStateManager.Suggestions = newSuggestions;
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
                    PublishKey(FunctionKeys.BackOne, null);
                    ReleaseUnlockedModifiers();
                }

                foreach (char c in suggestion)
                {
                    PublishKey(null, c);
                    ReleaseUnlockedModifiers();
                }

                lastTextChange = suggestion;
            }
        }

        private void AutoAddSpace()
        {
            if (Settings.Default.AutoAddSpace
                && Text != null
                && Text.Any()
                && !suppressAutoSpace)
            {
                PublishKey(null, ' ');
                Text = string.Concat(Text, " ");
            }
        }

        private void AutoPressShiftIfAppropriate()
        {
            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && keyboardStateManager.KeyDownStates[KeyValueKeys.ShiftKey].Value == KeyDownStates.Off)
            {
                keyboardStateManager.KeyDownStates[KeyValueKeys.ShiftKey].Value = KeyDownStates.On;
            }
        }

        private string ModifyCapturedText(string capture)
        {
            if (keyboardStateManager.KeyDownStates[KeyValueKeys.AltKey].Value.IsOnOrLock())
            {
                //TODO Handle Alt modified captures - Alt+Code = unicode characters
                return null;
            }

            if (keyboardStateManager.KeyDownStates[KeyValueKeys.CtrlKey].Value.IsOnOrLock())
            {
                return null;
            }

            if (!string.IsNullOrEmpty(capture))
            {
                if (keyboardStateManager.KeyDownStates[KeyValueKeys.ShiftKey].Value == KeyDownStates.On)
                {
                    return capture.FirstCharToUpper();
                }

                if (keyboardStateManager.KeyDownStates[KeyValueKeys.ShiftKey].Value == KeyDownStates.Lock)
                {
                    return capture.ToUpper();
                }
            }

            return capture;
        }

        #endregion
    }
}