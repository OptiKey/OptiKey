using System;
using System.Collections.Generic;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public interface IDictionaryService
    {
        event EventHandler<Exception> Error;

        void LoadDictionary();
        bool ExistsInDictionary(string entryToFind);
        IEnumerable<string> GetHashes();
        IEnumerable<DictionaryEntryWithUsageCount> GetAllEntriesWithUsageCounts();
        IEnumerable<string> GetEntries(string hash);
        void AddNewEntryToDictionary(string entry);
        void RemoveEntryFromDictionary(string entry);
        void RemoveEntryFromDictionary(DictionaryEntryWithUsageCount entryWithUsageCount);
        void IncrementEntryUsageCount(string entry);
        void DecrementEntryUsageCount(string entry);
    }
}
