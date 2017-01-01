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
        private readonly Dictionary<string, HashSet<DictionaryEntry>> entriesForAutoComplete = new Dictionary<string, HashSet<DictionaryEntry>>();

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Removes all possible suggestions from the auto complete provider.
        /// </summary>
        public void Clear()
        {
            Log.Debug("Clear called.");
            entriesForAutoComplete.Clear();
        }

        public IEnumerable<string> GetSuggestions(string root)
        {
            Log.DebugFormat("GetAutoCompleteSuggestions called with root '{0}'", root);

            if (entriesForAutoComplete != null)
            {
                var inProgressWord = root == null ? null : root.InProgressWord(root.Length);

                if (!string.IsNullOrEmpty(inProgressWord)
                            && char.IsLetter(inProgressWord.First())) //A word must start with a letter
                {
                    var simplifiedRoot = inProgressWord.Normalise();

                    if (!string.IsNullOrWhiteSpace(simplifiedRoot))
                    {
                        return
                            entriesForAutoComplete
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
            }

            return Enumerable.Empty<string>();
        }

        public void AddEntry(string entry, DictionaryEntry newEntryWithUsageCount)
        {

            //Also add to entries for auto complete
            var autoCompleteHash = entry.Normalise(log: false);
            AddToDictionary(entry, autoCompleteHash, newEntryWithUsageCount);
            if (!string.IsNullOrWhiteSpace(entry) && entry.Contains(" "))
            {
                //Entry is a phrase - also add with a dictionary entry hash (first letter of each word)
                var phraseAutoCompleteHash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);
                AddToDictionary(entry, phraseAutoCompleteHash, newEntryWithUsageCount);
            }


        }

        private void AddToDictionary (string entry, string autoCompleteHash, DictionaryEntry newEntryWithUsageCount)
        { 

            if (!string.IsNullOrWhiteSpace(autoCompleteHash))
            {
                if (entriesForAutoComplete.ContainsKey(autoCompleteHash))
                {
                    if (entriesForAutoComplete[autoCompleteHash].All(nwwuc => nwwuc.Entry != entry))
                    {
                        entriesForAutoComplete[autoCompleteHash].Add(newEntryWithUsageCount);
                    }
                }
                else
                {
                    entriesForAutoComplete.Add(autoCompleteHash, new HashSet<DictionaryEntry> { newEntryWithUsageCount });
                }
            }
        }

        public void RemoveEntry(string entry)
        {
            var autoCompleteHash = entry.Normalise(log: false);
            if (!string.IsNullOrWhiteSpace(autoCompleteHash)
                && entriesForAutoComplete.ContainsKey(autoCompleteHash))
            {
                var foundEntryForAutoComplete = entriesForAutoComplete[autoCompleteHash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                if (foundEntryForAutoComplete != null)
                {
                    entriesForAutoComplete[autoCompleteHash].Remove(foundEntryForAutoComplete);

                    if (!entriesForAutoComplete[autoCompleteHash].Any())
                    {
                        entriesForAutoComplete.Remove(autoCompleteHash);
                    }
                }
            }

        }
    }
}
