// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Reflection;
using JuliusSweetland.OptiKey.Extensions;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    internal class KeyboardOutputServiceState
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string text;
        private readonly string lastTextChange;
        private readonly bool lastTextChangeWasSuggestion;
        private readonly bool suppressNextAutoSpace;
        private readonly bool shiftStateSetAutomatically;
        private readonly bool simulateKeyStrokes;
        private readonly List<string> suggestions;
        private readonly Action<string> setText;
        private readonly Action<string> setLastTextChange;
        private readonly Action<bool> setLastTextChangeWasSuggestion;
        private readonly Action<bool> setSuppressNextAutoSpace;
        private readonly Action<bool> setShiftStateSetAutomatically;
        private readonly Action<List<string>> setSuggestions;
        
        public KeyboardOutputServiceState(
            bool simulateKeyStrokes,
            Func<string> getText, Action<string> setText,
            Func<string> getLastTextChange, Action<string> setLastTextChange,
            Func<bool> getLastTextChangeWasSuggestion, Action<bool> setLastTextChangeWasSuggestion,
            Func<bool> getSuppressNextAutoSpace, Action<bool> setSuppressNextAutoSpace,
            Func<bool> getShiftStateSetAutomatically, Action<bool> setShiftStateSetAutomatically,
            Func<List<string>> getSuggestions, Action<List<string>> setSuggestions)
        {
            text = getText();
            lastTextChange = getLastTextChange();
            lastTextChangeWasSuggestion = getLastTextChangeWasSuggestion();
            suppressNextAutoSpace = getSuppressNextAutoSpace();
            shiftStateSetAutomatically = getShiftStateSetAutomatically();
            suggestions = getSuggestions();

            Log.InfoFormat("Saving KeyboardOutputService state for SimulateKeyStrokes={0}. Text:'{1}', LastTextChange:'{2}', LastTextChangeWasSuggestion:'{3}', SuppressNextAutoSpace:'{4}', ShiftStateSetAutomatically:'{5}', Suggestions:'{6}'",
                simulateKeyStrokes, text, lastTextChange, lastTextChangeWasSuggestion, suppressNextAutoSpace, shiftStateSetAutomatically, suggestions.ToString("(null)"));

            this.setText = setText;
            this.setLastTextChange = setLastTextChange;
            this.setLastTextChangeWasSuggestion = setLastTextChangeWasSuggestion;
            this.setSuppressNextAutoSpace = setSuppressNextAutoSpace;
            this.setShiftStateSetAutomatically = setShiftStateSetAutomatically;
            this.setSuggestions = setSuggestions;
            this.simulateKeyStrokes = simulateKeyStrokes;
        }

        public void RestoreState()
        {
            Log.InfoFormat("Restoring KeyboardOutputService state for SimulateKeyStrokes={0}. Text:'{1}', LastTextChange:'{2}', LastTextChangeWasSuggestion:'{3}', SuppressNextAutoSpace:'{4}', ShiftStateSetAutomatically:'{5}', Suggestions:'{6}'",
                simulateKeyStrokes, text, lastTextChange, lastTextChangeWasSuggestion, suppressNextAutoSpace, shiftStateSetAutomatically, suggestions.ToString("(null)"));

            setText(text);
            setLastTextChange(lastTextChange);
            setLastTextChangeWasSuggestion(lastTextChangeWasSuggestion);
            setSuppressNextAutoSpace(suppressNextAutoSpace);
            setShiftStateSetAutomatically(shiftStateSetAutomatically);
            setSuggestions(suggestions);
        }
    }
}
