using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IDictionaryService : INotifyErrors
    {
        void LoadDictionary();
        bool ExistsInDictionary(string entryToFind);
        IEnumerable<string> GetHashes();
        IEnumerable<DictionaryEntryWithUsageCount> GetAllEntriesWithUsageCounts();
        IEnumerable<string> GetEntries(string hash);
        void AddNewEntryToDictionary(string entry);
        void RemoveEntryFromDictionary(string entry);
        void IncrementEntryUsageCount(string entry);
        void DecrementEntryUsageCount(string entry);
        List<string> MapCaptureToEntries(List<Timestamped<PointAndKeyValue>> timestampedPointAndKeyValues,
            string reducedCapture, bool firstSequenceLetterIsReliable, bool lastSequenceLetterIsReliable, 
            ref CancellationTokenSource cancellationTokenSource, Action<Exception> exceptionHandler);
    }
}
