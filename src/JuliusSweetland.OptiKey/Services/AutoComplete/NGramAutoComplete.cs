using JuliusSweetland.OptiKey.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JuliusSweetland.OptiKey.Extensions;
using log4net;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
    /// <summary>
    /// An auto suggest class using the n-gram algorithm.
    /// https://en.wikipedia.org/wiki/N-gram
    /// n-grams provide a quick way to do a fuzzy search that works decently across a wide range of languages.
    /// </summary>
    public class NGramAutoComplete : IManageAutoComplete
    {
        private readonly Dictionary<string, HashSet<EntryMetadata>> entries = new Dictionary<string, HashSet<EntryMetadata>>();
        private readonly Dictionary<string, HashSet<DictionaryEntry>> phrases = new Dictionary<string, HashSet<DictionaryEntry>>();
        private readonly Func<string, string> normalize;
        private readonly int gramCount;
        private readonly string leadingSpaces;
        private readonly string trailingSpaces;

        private static readonly Func<string, string> DefaultNormalizeFunc = x => x.Normalise();
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Creates a n-gram auto-completer using the default settings.
        /// </summary>
        public NGramAutoComplete(int gramCount = 3, int leadingSpaceCount = 2, int trailingSpaceCount = 1)
            : this(DefaultNormalizeFunc, gramCount, leadingSpaceCount, trailingSpaceCount)
        {
        }

        /// <summary>
        /// Creates a n-gram auto-completer, allowing tuning of the parameters used.
        /// </summary>
        /// <param name="normalizeFunc">A function to normalize input. This function should convert the string into a base form for comparison.</param>
        /// <param name="gramCount">The size of each gram.</param>
        /// <param name="leadingSpaceCount">Number of leading spaces. The more spaces the higher priority the start of the string has.</param>
        /// <param name="trailingSpaceCount">Number of trailing spaces. The more spaces the higher priority the end of the string has.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="gramCount"/> must be greater than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="leadingSpaceCount"/> must be non-negative but less than <paramref name="gramCount"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="trailingSpaceCount"/> must be non-negative but less than <paramref name="gramCount"/>.</exception>
        public NGramAutoComplete(Func<string, string> normalizeFunc, int gramCount, int leadingSpaceCount, int trailingSpaceCount)
        {
            if (gramCount < 1)
            {
                throw new ArgumentOutOfRangeException("gramCount", gramCount, @"Must be greater than 0");
            }
            if ((leadingSpaceCount < 0) || (leadingSpaceCount >= gramCount))
            {
                throw new ArgumentOutOfRangeException("leadingSpaceCount", leadingSpaceCount,
                    @"Must be 0 or larger, and less than gramCount");
            }
            if ((trailingSpaceCount < 0) || (trailingSpaceCount >= gramCount))
            {
                throw new ArgumentOutOfRangeException("trailingSpaceCount", trailingSpaceCount,
                    @"Must be 0 or larger, and less than gramCount");
            }

            normalize = normalizeFunc;
            this.gramCount = gramCount;
            leadingSpaces = new string(' ', leadingSpaceCount);
            trailingSpaces = new string(' ', trailingSpaceCount);
        }

        public void AddEntry(string entry, DictionaryEntry entryWithUsageCount)
        {
            var ngrams = ToNGrams(entry).ToList();
            var entryMetaData = new EntryMetadata(entryWithUsageCount, ngrams.Count);

            foreach (var ngram in ngrams)
            {
                if (entries.ContainsKey(ngram))
                {
                    entries[ngram].Add(entryMetaData);
                }
                else
                {
                    entries[ngram] = new HashSet<EntryMetadata> {entryMetaData};
                }
            }

            if (!string.IsNullOrWhiteSpace(entry) && entry.Contains(" "))
            {
                //Entry is a phrase - also add with a dictionary entry hash (first letter of each word)
                var phraseAutoCompleteHash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);
                if (!string.IsNullOrWhiteSpace(phraseAutoCompleteHash))
                {
                    if (phrases.ContainsKey(phraseAutoCompleteHash))
                    {
                        if (phrases[phraseAutoCompleteHash].All(nwwuc => nwwuc.Entry != entry))
                        {
                            phrases[phraseAutoCompleteHash].Add(entryWithUsageCount);
                        }
                    }
                    else
                    {
                        phrases.Add(phraseAutoCompleteHash, new HashSet<DictionaryEntry> { entryWithUsageCount });
                    }
                }
            }
        }

        /// <summary>
        /// Removes all possible suggestions from the auto complete provider.
        /// </summary>
        public void Clear()
        {
            Log.Debug("Clear called.");
            entries.Clear();
            phrases.Clear();
        }

        public IEnumerable<string> GetSuggestions(string root)
        {
            Log.DebugFormat("GetSuggestions called with root '{0}'.", root);

            var nGrams = ToNGrams(root).ToList();
            var nGramcount = nGrams.Count;

            var matches = nGrams
                .Where(x => entries.ContainsKey(x))
                .SelectMany(x => entries[x])
                .GroupBy(x => x)
                .Select(x => new
                {
                    MetaData = x.Key,
                    Score = CalculateScore(x.Count(), nGramcount, x.Key.NGramCount)
                })
                .Select(x => new { Entry = x.MetaData.DictionaryEntry.Entry, UsageCount = x.MetaData.DictionaryEntry.UsageCount, Score = x.Score });

            //Also find phrase matches
            var phraseHash = root.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log:false);
            if (!string.IsNullOrWhiteSpace(phraseHash))
            {
                matches = matches.Union(phrases
                    .Where(kvp => kvp.Key.Equals(phraseHash))
                    .SelectMany(kvp => kvp.Value)
                    .Where(de => de.Entry.Length >= root.Length)
                    .Select(de => new { Entry = de.Entry, UsageCount = de.UsageCount, Score = double.MaxValue }));
            }

            return matches
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.UsageCount)
                .ThenBy(x => x.Entry.Length)
                .Select(x => x.Entry)
                .Distinct();
        }

        public void RemoveEntry(string entry)
        {
            foreach (var ngram in ToNGrams(entry))
            {
                if (entries.ContainsKey(ngram))
                {
                    entries[ngram].RemoveWhere(x => x.DictionaryEntry.Entry == entry);

                    if (!entries[ngram].Any())
                    {
                        entries.Remove(ngram);
                    }
                }
            }

            //Also remove if entry is a phrase
            if (!string.IsNullOrWhiteSpace(entry) && entry.Contains(" "))
            {
                var phraseHash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);
                if (!string.IsNullOrWhiteSpace(phraseHash)
                    && phrases.ContainsKey(phraseHash))
                {
                    var foundEntryForAutoComplete = phrases[phraseHash].FirstOrDefault(ewuc => ewuc.Entry == entry);

                    if (foundEntryForAutoComplete != null)
                    {
                        phrases[phraseHash].Remove(foundEntryForAutoComplete);

                        if (!phrases[phraseHash].Any())
                        {
                            phrases.Remove(phraseHash);
                        }
                    }
                }
            }
        }
        private static double CalculateScore(double numberOfMatches, double numberOfRootNGrams, double numberOfEntryNGrams)
        {
            return 2 * numberOfMatches / (numberOfRootNGrams + numberOfEntryNGrams);
        }

        private IEnumerable<string> ToNGrams(string word)
        {
            var normalizedWord = leadingSpaces + normalize(word) + trailingSpaces;
            for (var i = 0; i < normalizedWord.Length - gramCount + 1; i++)
            {
                yield return normalizedWord.Substring(i, gramCount);
            }
        }

        [DebuggerDisplay("'{DictionaryEntry.Entry}' used {DictionaryEntry.UsageCount} (ngrams: {NGramCount})")]
        private class EntryMetadata
        {
            public EntryMetadata(DictionaryEntry dictionaryEntry, int nGramCount)
            {
                DictionaryEntry = dictionaryEntry;
                NGramCount = nGramCount;
            }

            public DictionaryEntry DictionaryEntry { get; }
            public int NGramCount { get; }
        }
    }
}
