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
        private readonly bool simulateKeyStrokes;

        public KeyboardOutputServiceState(
            bool simulateKeyStrokes,
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

            Log.DebugFormat("Saving KeyboardOutputService state for SimulateKeyStrokes={0}. Text:'{1}', LastTextChange:'{2}', LastTextChangeWasSuggestion:'{3}', SuppressNextAutoSpace:'{4}', ShiftStateSetAutomatically:'{5}'",
                simulateKeyStrokes, text, lastTextChange, lastTextChangeWasSuggestion, suppressNextAutoSpace, shiftStateSetAutomatically);

            this.setText = setText;
            this.setLastTextChange = setLastTextChange;
            this.setLastTextChangeWasSuggestion = setLastTextChangeWasSuggestion;
            this.setSuppressNextAutoSpace = setSuppressNextAutoSpace;
            this.setShiftStateSetAutomatically = setShiftStateSetAutomatically;
            this.simulateKeyStrokes = simulateKeyStrokes;
        }

        public void RestoreState()
        {
            Log.DebugFormat("Restoring KeyboardOutputService state for SimulateKeyStrokes={0}. Text:'{1}', LastTextChange:'{2}', LastTextChangeWasSuggestion:'{3}', SuppressNextAutoSpace:'{4}', ShiftStateSetAutomatically:'{5}'",
                simulateKeyStrokes, text, lastTextChange, lastTextChangeWasSuggestion, suppressNextAutoSpace, shiftStateSetAutomatically);

            setText(text);
            setLastTextChange(lastTextChange);
            setLastTextChangeWasSuggestion(lastTextChangeWasSuggestion);
            setSuppressNextAutoSpace(suppressNextAutoSpace);
            setShiftStateSetAutomatically(shiftStateSetAutomatically);
        }
    }
}
