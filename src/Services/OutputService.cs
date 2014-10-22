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

        private NotifyingProxy<KeyDownStates> ShiftKeyProxy
        { 
            get
            {
                return keyboardStateManager.KeyDownStates[new KeyValue {FunctionKey = FunctionKeys.Shift}.Key];
            } 
        } 

        #endregion

        #region Methods

        public void ClearText()
        {
            ShiftKeyProxy.Value = KeyDownStates.On;
            lastTextChange = null;
            Text = null;
        }

        public void ProcessCapture(string capture)
        {
            if (string.IsNullOrEmpty(capture)) return;

            //Suppress auto space if... 
            if (string.IsNullOrEmpty(lastTextChange) //we have no text change history
                || (lastTextChange.Length == 1 && capture.Length == 1) //we are capturing char by char (after 1st char)
                || (capture.Length == 1 && !char.IsLetter(capture.First())) //we have captured a single char which is not a letter
                || new [] { " ", "\n" }.Contains(lastTextChange) //the current capture follows a space or newline
                )
            {
                suppressAutoSpace = true;
            }

            AutoPrependSpace();

            var casedCapture = CaseCapture(capture);
            Text = string.Concat(Text, casedCapture);

            AutoCapitalise();

            lastTextChange = casedCapture;
            suppressAutoSpace = false;
        }

        public void ProcessCapture(FunctionKeys capture)
        {
            switch (capture)
            {
                case FunctionKeys.Enter:
                    ProcessCapture("\n");
                    break;
            }
        }

        public void ProcessBackOne()
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
            }

            AutoCapitalise();

            suppressAutoSpace = true;
        }

        public void ProcessBackMany()
        {
            throw new NotImplementedException();

            AutoCapitalise();

            suppressAutoSpace = true;
        }

        public void SwapLastCaptureForSuggestion(string suggestion)
        {
            throw new NotImplementedException();
        }

        private void AutoPrependSpace()
        {
            if (Settings.Default.AutoAddSpace
                && Text != null
                && Text.Any()
                && !suppressAutoSpace)
            {
                Text = string.Concat(Text, " ");
            }
        }

        private void AutoCapitalise()
        {
            if (ShiftKeyProxy.Value == KeyDownStates.On)
            {
                ShiftKeyProxy.Value = KeyDownStates.Off;
            }

            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && ShiftKeyProxy.Value == KeyDownStates.Off)
            {
                ShiftKeyProxy.Value = KeyDownStates.On;
            }
        }

        private string CaseCapture(string capture)
        {
            if (!string.IsNullOrEmpty(capture))
            {
                if (ShiftKeyProxy.Value == KeyDownStates.On)
                {
                    return capture.FirstCharToUpper();
                }

                if (ShiftKeyProxy.Value == KeyDownStates.Lock)
                {
                    return capture.ToUpper();
                }
            }

            return capture;
        }

        #endregion
    }
}