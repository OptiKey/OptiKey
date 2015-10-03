using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IDictionaryService : INotifyErrors
    {
        void LoadDictionary();
        bool ExistsInDictionary(string entryToFind);
        IEnumerable<DictionaryEntry> GetAllEntries();
        IEnumerable<string> GetAutoCompleteSuggestions(string root);
        void AddNewEntryToDictionary(string entry);
        void RemoveEntryFromDictionary(string entry);
        void IncrementEntryUsageCount(string entry);
        void DecrementEntryUsageCount(string entry);
        Tuple<List<Point>, FunctionKeys?, string, List<string>> MapCaptureToEntries(
            List<Timestamped<PointAndKeyValue>> timestampedPointAndKeyValues, 
            int minCount, string reliableFirstLetter, string reliableLastLetter,
            ref CancellationTokenSource cancellationTokenSource, Action<Exception> exceptionHandler);
    }
}
