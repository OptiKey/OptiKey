// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JuliusSweetland.OptiKey.Services.Suggestions
{
    public class BasicAutoComplete : IManagedSuggestions
    {
        private readonly Dictionary<string, HashSet<DictionaryEntry>> entries = new Dictionary<string, HashSet<DictionaryEntry>>();
        private readonly HashSet<string> wordsIndex = new HashSet<string>();

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Removes all possible suggestions from the auto complete provider.
        /// </summary>
        public void Clear()
        {
            Log.Debug("Clear called.");
            entries.Clear();
        }

        public virtual IEnumerable<string> GetSuggestions(string root, bool nextWord)
        {
            Log.DebugFormat("GetAutoCompleteSuggestions called with root '{0}'", root);

            if (entries != null)
            {
                var inProgressWord = root == null ? null : root.InProgressWord(root.Length);
                var simplifiedRoot = root.Normalise();

                if (!string.IsNullOrEmpty(inProgressWord)
                            && char.IsLetter(inProgressWord.First())) //A word must start with a letter
                {
                    return
                        entries
                            .Where(kvp => kvp.Key.StartsWith(simplifiedRoot, StringComparison.Ordinal))
                            .SelectMany(kvp => kvp.Value)
                            .Where(de => de.Entry.Length >= root.Length)
                            .Distinct()
                            // Phrases are stored in entriesForAutoComplete with multiple hashes (one the full version
                            // of the phrase and one the first letter of each word so you can look them up by either)
                            .OrderByDescending(de => de.UsageCount)
                            .ThenBy(de => de.Entry.Length)
                            .Select(de => de.Entry);
                }
            }

            return Enumerable.Empty<string>();
        }

        public void AddEntry(string entry, DictionaryEntry newEntryWithUsageCount, string normalizedHash = "")
        {
            if (!string.IsNullOrWhiteSpace(entry) && entry.Contains(" "))
            {
                //Entry is a phrase - also add with a dictionary entry hash (first letter of each word)
                var phraseAutoCompleteHash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);
                AddEntry(phraseAutoCompleteHash, newEntryWithUsageCount);
            }

            //Also add to entries for auto complete
            var autoCompleteHash = entry.Normalise(log: false);
            
            AddToDictionary(entry, autoCompleteHash, newEntryWithUsageCount);
            if (!wordsIndex.Contains(autoCompleteHash))
            {
                wordsIndex.Add(autoCompleteHash);
            }
        }

        private void AddToDictionary (string entry, string autoCompleteHash, DictionaryEntry newEntryWithUsageCount)
        {
            if (!string.IsNullOrWhiteSpace(autoCompleteHash))
            {
                if (entries.ContainsKey(autoCompleteHash))
                {
                    if (entries[autoCompleteHash].All(nwwuc => nwwuc.Entry != entry))
                    {
                        entries[autoCompleteHash].Add(newEntryWithUsageCount);
                    }
                }
                else
                {
                    entries.Add(autoCompleteHash, new HashSet<DictionaryEntry> { newEntryWithUsageCount });
                }
            }
        }

        public void RemoveEntry(string entry)
        {
            var autoCompleteHash = entry.Normalise(log: false);
            RemoveEntryWorker(entry, autoCompleteHash);

            var normalizedHash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(false);
            RemoveEntryWorker(entry, autoCompleteHash);
            wordsIndex.Remove(normalizedHash);
        }

        private void RemoveEntryWorker(string entry, string hash)
        {
            if (!string.IsNullOrWhiteSpace(hash)
                    && entries.ContainsKey(hash))
            {
                var foundEntryForAutoComplete = entries[hash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                if (foundEntryForAutoComplete != null)
                {
                    entries[hash].Remove(foundEntryForAutoComplete);

                    if (!entries[hash].Any())
                    {
                        entries.Remove(hash);
                    }
                }
            }
        }

        public Dictionary<string, HashSet<DictionaryEntry>> GetEntries()
        {
            return entries;
        }

        public HashSet<string> GetWordsHashes()
        {
            return wordsIndex;
        }
    }
}
