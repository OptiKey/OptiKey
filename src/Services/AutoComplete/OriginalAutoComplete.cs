using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
    public class OriginalAutoComplete : IAutoComplete
    {
        private Dictionary<string, List<DictionaryEntry>> entriesForAutoComplete = new Dictionary<string, List<DictionaryEntry>>();

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<string> GetSuggestions(string root)
        {
            Log.DebugFormat("GetAutoCompleteSuggestions called with root '{0}'", root);

            if (entriesForAutoComplete != null)
            {
                var simplifiedRoot = root.CreateAutoCompleteDictionaryEntryHash();

                if (!string.IsNullOrWhiteSpace(simplifiedRoot))
                {
                    var enumerator =
                        new List<DictionaryEntry> { new DictionaryEntry (root) } //Include the typed root as first result
                        .Union(entriesForAutoComplete
                                .Where(kvp => kvp.Key.StartsWith(simplifiedRoot))
                                .SelectMany(kvp => kvp.Value)
                                .Where(de => de.Entry.Length > root.Length)
                                .Distinct() //Phrases are stored in entriesForAutoComplete with multiple hashes (one the full version of the phrase and one the first letter of each word so you can look them up by either)
                                .OrderByDescending(de => de.UsageCount)
                                .ThenBy(de => de.Entry.Length))
                        .Select(de => de.Entry)
                        .GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }

                yield break; //Not strictly necessary
            }
        }

        public void AddEntry(string entry, string autoCompleteHash, DictionaryEntry newEntryWithUsageCount)
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
                    entriesForAutoComplete.Add(autoCompleteHash, new List<DictionaryEntry> { newEntryWithUsageCount });
                }
            }
        }

        public void RemoveEntry(string entry)
        {
            var autoCompleteHash = entry.CreateAutoCompleteDictionaryEntryHash(log: false);
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
