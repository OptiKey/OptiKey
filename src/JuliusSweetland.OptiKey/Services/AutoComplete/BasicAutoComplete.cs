using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
	public class BasicAutoComplete : IManageAutoComplete
    {
        private readonly Dictionary<string, HashSet<DictionaryEntry>> entries = new Dictionary<string, HashSet<DictionaryEntry>>();

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Removes all possible suggestions from the auto complete provider.
        /// </summary>
        public void Clear()
        {
            Log.Debug("Clear called.");
            entries.Clear();
        }

        public IEnumerable<string> GetSuggestions(string root)
        {
            Log.DebugFormat("GetAutoCompleteSuggestions called with root '{0}'", root);

            if (entries != null)
            {
                var simplifiedRoot = root.Normalise();

                if (!string.IsNullOrWhiteSpace(simplifiedRoot))
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

        public void AddEntry(string entry, DictionaryEntry newEntryWithUsageCount)
		{
			if (!string.IsNullOrWhiteSpace(entry) && entry.Contains(" "))
			{
				//Entry is a phrase - also add with a dictionary entry hash (first letter of each word)
				var phraseAutoCompleteHash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);
				AddToDictionary(entry, phraseAutoCompleteHash, newEntryWithUsageCount);
			}

			//Also add to entries for auto complete
			var autoCompleteHash = entry.Normalise(log: false);
            AddToDictionary(entry, autoCompleteHash, newEntryWithUsageCount);
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
            if (!string.IsNullOrWhiteSpace(autoCompleteHash)
                && entries.ContainsKey(autoCompleteHash))
            {
                var foundEntryForAutoComplete = entries[autoCompleteHash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                if (foundEntryForAutoComplete != null)
                {
                    entries[autoCompleteHash].Remove(foundEntryForAutoComplete);

                    if (!entries[autoCompleteHash].Any())
                    {
                        entries.Remove(autoCompleteHash);
                    }
                }
            }

        }

		public Dictionary<string, HashSet<DictionaryEntry>> GetEntries()
		{
			return entries;
		}
    }
}
