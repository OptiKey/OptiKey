using System;
using System.Reflection;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    internal class KeyboardOutputServiceState
    {
        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string text;
        private readonly string lastTextChange;
        private readonly bool lastTextChangeWasSuggestion;
        private readonly bool suppressNextAutoSpace;
        private readonly bool shiftStateSetAutomatically;
        private readonly Action<string> setText;
        private readonly Action<string> setLastTextChange;
        private readonly Action<bool> setLastTextChangeWasSuggestion;
        private readonly Action<bool> setSuppressNextAutoSpace;
        private readonly Action<bool> setShiftStateSetAutomatically;

        public KeyboardOutputServiceState(
            Func<string> getText, Action<string> setText,
            Func<string> getLastTextChange, Action<string> setLastTextChange,
            Func<bool> getLastTextChangeWasSuggestion, Action<bool> setLastTextChangeWasSuggestion,
            Func<bool> getSuppressNextAutoSpace, Action<bool> setSuppressNextAutoSpace,
            Func<bool> getShiftStateSetAutomatically, Action<bool> setShiftStateSetAutomatically)
        {
            text = getText();
            lastTextChange = getLastTextChange();
            lastTextChangeWasSuggestion = getLastTextChangeWasSuggestion();
            suppressNextAutoSpace = getSuppressNextAutoSpace();
            shiftStateSetAutomatically = getShiftStateSetAutomatically();

            Log.DebugFormat("Saving state. Text:'{0}', LastTextChange:'{1}', LastTextChangeWasSuggestion:'{2}', SuppressNextAutoSpace:'{3}', ShiftStateSetAutomatically:'{4}'",
                text, lastTextChange, lastTextChangeWasSuggestion, suppressNextAutoSpace, shiftStateSetAutomatically);

            this.setText = setText;
            this.setLastTextChange = setLastTextChange;
            this.setLastTextChangeWasSuggestion = setLastTextChangeWasSuggestion;
            this.setSuppressNextAutoSpace = setSuppressNextAutoSpace;
            this.setShiftStateSetAutomatically = setShiftStateSetAutomatically;
        }

        public void RestoreState()
        {
            Log.DebugFormat("Restoring state. Text:'{0}', LastTextChange:'{1}', LastTextChangeWasSuggestion:'{2}', SuppressNextAutoSpace:'{3}', ShiftStateSetAutomatically:'{4}'",
                text, lastTextChange, lastTextChangeWasSuggestion, suppressNextAutoSpace, shiftStateSetAutomatically);

            setText(text);
            setLastTextChange(lastTextChange);
            setLastTextChangeWasSuggestion(lastTextChangeWasSuggestion);
            setSuppressNextAutoSpace(suppressNextAutoSpace);
            setShiftStateSetAutomatically(shiftStateSetAutomatically);
        }
    }
}
