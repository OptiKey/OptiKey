using System;
using System.Linq;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Services
{
    public class OutputService : BindableBase, IOutputService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IKeyboardStateManager keyboardStateManager;
        private readonly string shiftKey = new KeyValue { FunctionKey = FunctionKeys.Shift }.Key;
        private readonly string altKey = new KeyValue { FunctionKey = FunctionKeys.Alt }.Key;
        private readonly string ctrlKey = new KeyValue { FunctionKey = FunctionKeys.Ctrl }.Key;

        private string lastTextChange;
        private bool suppressAutoSpace = true;
        
        #endregion

        #region Ctor

        public OutputService(IKeyboardStateManager keyboardStateManager)
        {
            this.keyboardStateManager = keyboardStateManager;

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

        private KeyDownStates shiftKeyDownState;
        private KeyDownStates ShiftKeyDownState
        { 
            get { return shiftKeyDownState; }
            set
            {
                if (shiftKeyDownState != value)
                {
                    var previousValue = shiftKeyDownState;

                    shiftKeyDownState = value;
                    keyboardStateManager.KeyDownStates[shiftKey].Value = value;

                    if (Settings.Default.PublishingKeys)
                    {
                        if (previousValue == KeyDownStates.Off
                        && (value == KeyDownStates.On || value == KeyDownStates.Lock))
                        {
                            //TODO Publish shift down
                        }
                        else if ((previousValue == KeyDownStates.On || previousValue == KeyDownStates.Lock)
                            && value == KeyDownStates.Off)
                        {
                            //TODO Publish shift up
                        }
                    }
                }
            }
        }

        private KeyDownStates altKeyDownState;
        private KeyDownStates AltKeyDownState
        {
            get { return altKeyDownState; }
            set
            {
                if (altKeyDownState != value)
                {
                    var previousValue = altKeyDownState;

                    altKeyDownState = value;
                    keyboardStateManager.KeyDownStates[altKey].Value = value;

                    if (Settings.Default.PublishingKeys)
                    {
                        if (previousValue == KeyDownStates.Off
                            && (value == KeyDownStates.On || value == KeyDownStates.Lock))
                        {
                            //TODO Publish alt down
                        }
                        else if ((previousValue == KeyDownStates.On || previousValue == KeyDownStates.Lock)
                                 && value == KeyDownStates.Off)
                        {
                            //TODO Publish alt up
                        }
                    }
                }
            }
        }

        private KeyDownStates ctrlKeyDownState;
        private KeyDownStates CtrlKeyDownState
        {
            get { return ctrlKeyDownState; }
            set
            {
                if (ctrlKeyDownState != value)
                {
                    var previousValue = ctrlKeyDownState;

                    ctrlKeyDownState = value;
                    keyboardStateManager.KeyDownStates[ctrlKey].Value = value;

                    if (Settings.Default.PublishingKeys)
                    {
                        if (previousValue == KeyDownStates.Off
                            && (value == KeyDownStates.On || value == KeyDownStates.Lock))
                        {
                            //TODO Publish ctrl down
                        }
                        else if ((previousValue == KeyDownStates.On || previousValue == KeyDownStates.Lock)
                                 && value == KeyDownStates.Off)
                        {
                            //TODO Publish ctrl up
                        }
                    }
                }
            }
        } 

        #endregion

        #region Methods - IOutputService

        public void ProcessCapture(FunctionKeys? functionKey, string chars)
        {
            if (functionKey == null && string.IsNullOrEmpty(chars)) return;

            bool calculateShiftState = false;

            if (functionKey != null)
            {
                switch (functionKey.Value)
                {
                    case FunctionKeys.Alt:
                        AltKeyDownState = AltKeyDownState == KeyDownStates.Off
                            ? AltKeyDownState = KeyDownStates.On
                            : AltKeyDownState == KeyDownStates.On
                                ? AltKeyDownState = KeyDownStates.Lock
                                : AltKeyDownState = KeyDownStates.Off;
                        break;

                    case FunctionKeys.BackMany:
                        ProcessBackMany();

                        lastTextChange = null;
                        suppressAutoSpace = true;
                        calculateShiftState = true;
                        break;

                    case FunctionKeys.BackOne:
                        ProcessBackOne();

                        lastTextChange = null;
                        suppressAutoSpace = true;
                        calculateShiftState = true;
                        break;

                    case FunctionKeys.ClearOutput:
                        Text = null;

                        lastTextChange = null;
                        suppressAutoSpace = true;
                        calculateShiftState = true;
                        break;

                    case FunctionKeys.Ctrl:
                        CtrlKeyDownState = CtrlKeyDownState == KeyDownStates.Off
                            ? CtrlKeyDownState = KeyDownStates.On
                            : CtrlKeyDownState == KeyDownStates.On
                                ? CtrlKeyDownState = KeyDownStates.Lock
                                : CtrlKeyDownState = KeyDownStates.Off;
                        break;

                    case FunctionKeys.Shift:
                        ShiftKeyDownState = ShiftKeyDownState == KeyDownStates.Off
                            ? ShiftKeyDownState = KeyDownStates.On
                            : ShiftKeyDownState == KeyDownStates.On
                                ? ShiftKeyDownState = KeyDownStates.Lock
                                : ShiftKeyDownState = KeyDownStates.Off;
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
                        //TODO process all other function keys

                        ProcessSingleElementOfCapture(functionKey.Value, null);
                        lastTextChange = null;
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

                var modifiedChars = ApplyModifiers(chars);
                if (!string.IsNullOrEmpty(modifiedChars))
                {
                    var prefix = GeneratePrefix();
                    Text = string.Concat(Text, prefix, modifiedChars);
                }

                foreach (char c in chars)
                {
                    ProcessSingleElementOfCapture(null, c);
                }
                
                lastTextChange = modifiedChars;
                suppressAutoSpace = false;
                calculateShiftState = true;
            }

            if (calculateShiftState)
            {
                CalculateShiftState();
            }
        }

        #endregion

        #region Methods - private

        private void ProcessSingleElementOfCapture(FunctionKeys? functionKey, char? character)
        {
            if (Settings.Default.PublishingKeys)
            {
                //TODO Publish key

            }

            //Release modifiers which are not locked
            if (ShiftKeyDownState == KeyDownStates.On) ShiftKeyDownState = KeyDownStates.Off;
            if (AltKeyDownState == KeyDownStates.On) AltKeyDownState = KeyDownStates.Off; 
            if (CtrlKeyDownState == KeyDownStates.On) CtrlKeyDownState = KeyDownStates.Off;
        }

        private void ProcessBackOne()
        {
            if (AltKeyDownState == KeyDownStates.On
                || AltKeyDownState == KeyDownStates.Lock
                || CtrlKeyDownState == KeyDownStates.On
                || CtrlKeyDownState == KeyDownStates.Lock)
            {
                //Ctrl+Backspace = back a whole word (in some applications?)
                //Shift+Backspace = undo (in some applications?)
                //As the expected behaviour varies we will not handle a modified backspace
                //TODO Audible error tone = we should not get here and user should understand that something went wrong
                return;
            }
            
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
                    ProcessSingleElementOfCapture(FunctionKeys.BackOne, null);
                }
            }
        }

        private void ProcessBackMany()
        {
            throw new NotImplementedException();

            //TODO Apply modifiers? (Ctrl+Backspace = whole word. Shift+Backspace = undo)
        }

        private void SwapLastCaptureForSuggestion(int index)
        {
            var suggestionIndex = (keyboardStateManager.SuggestionsPage * keyboardStateManager.SuggestionsPerPage) + index;
            if (keyboardStateManager.Suggestions.Count > suggestionIndex)
            {
                SwapLastCaptureForSuggestion(keyboardStateManager.Suggestions[suggestionIndex]);
            }
        }

        private void SwapLastCaptureForSuggestion(string suggestion)
        {
            throw new NotImplementedException();
        }

        private string GeneratePrefix()
        {
            return Settings.Default.AutoAddSpace
                   && Text != null
                   && Text.Any()
                   && !suppressAutoSpace
                ? " "
                : null;
        }

        private void CalculateShiftState()
        {
            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && ShiftKeyDownState == KeyDownStates.Off)
            {
                ShiftKeyDownState = KeyDownStates.On;
            }
        }

        private string ApplyModifiers(string capture)
        {
            if (AltKeyDownState == KeyDownStates.On
                || AltKeyDownState == KeyDownStates.Lock)
            {
                //TODO Handle Alt modified captures - Alt+Code = unicode characters
                return null;
            }

            if (CtrlKeyDownState == KeyDownStates.On
                || CtrlKeyDownState == KeyDownStates.Lock)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(capture))
            {
                if (ShiftKeyDownState == KeyDownStates.On)
                {
                    return capture.FirstCharToUpper();
                }

                if (ShiftKeyDownState == KeyDownStates.Lock)
                {
                    return capture.ToUpper();
                }
            }

            return capture;
        }

        #endregion
    }
}