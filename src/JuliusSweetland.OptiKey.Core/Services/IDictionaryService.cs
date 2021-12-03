// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
	public interface IDictionaryService : INotifyErrors
    {
        void LoadDictionary();
        bool ExistsInDictionary(string entryToFind);
        IEnumerable<DictionaryEntry> GetAllEntries();
        IEnumerable<string> GetSuggestions(string root, bool nextWord);
        void AddNewEntryToDictionary(string entry);
        void RemoveEntryFromDictionary(string entry);
        void IncrementEntryUsageCount(string entry);
        void DecrementEntryUsageCount(string entry);
		    void OnAppClosing(object sender, System.ComponentModel.CancelEventArgs e);
        Tuple<List<Point>, KeyValue, List<string>> MapCaptureToEntries(
            List<Timestamped<PointAndKeyValue>> timestampedPointAndKeyValues, 
            int minCount, string reliableFirstLetter, string reliableLastLetter,
            ref CancellationTokenSource cancellationTokenSource, Action<Exception> exceptionHandler);
    }
}
