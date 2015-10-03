using JuliusSweetland.OptiKey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
    public class NGramAutoComplete : IManageAutoComplete<DictionaryEntry>
    {
        private readonly Dictionary<string, HashSet<EntryMetadata>> entries = new Dictionary<string, HashSet<EntryMetadata>>();
        private readonly Func<string, string> normalize;
        private readonly string leadingSpaces;
        private readonly string trailingSpaces;

        public static readonly Func<string, string> DefaultNormalizeFunc = x => x.Trim().Normalize(NormalizationForm.FormKD).ToUpperInvariant();

        /// <summary>
        /// An auto suggest class using the trigram algorithm.
        /// https://en.wikipedia.org/wiki/N-gram
        /// n-grams provide a quick way to do a fuzzy search that works decently across a wide range of languages.
        /// </summary>
        public NGramAutoComplete()
            : this(DefaultNormalizeFunc) { }

        /// <summary>
        /// An auto suggest class using the trigram algorithm.
        /// https://en.wikipedia.org/wiki/N-gram
        /// n-grams provide a quick way to do a fuzzy search that works decently across a wide range of languages.
        /// </summary>
        /// <param name="normalizeFunc">A function to normalize input. This function should convert the string into a base form for comparison.</param>
        public NGramAutoComplete(Func<string, string> normalizeFunc)
            : this(normalizeFunc, 3, 2, 1) { }

        /// <summary>
        /// An auto suggest class using the n-gram algorithm.
        /// https://en.wikipedia.org/wiki/N-gram
        /// n-grams provide a quick way to do a fuzzy search that works decently across a wide range of languages.
        /// </summary>
        /// <param name="normalizeFunc">A function to normalize input. This function should convert the string into a base form for comparison.</param>
        /// <param name="gramCount">The size of each gram.</param>
        /// <param name="leadingSpaceCount">Number of leading spaces. The more spaces the higher priority the start of the string has.</param>
        /// <param name="trailingSpaceCount">Number of trailing spaces. The more spaces the higher priority the end of the string has.</param>
        public NGramAutoComplete(Func<string, string> normalizeFunc, int gramCount, int leadingSpaceCount, int trailingSpaceCount)
        {
            if (gramCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(gramCount), gramCount, "Must be greater than 0");
            }
            if (leadingSpaceCount < 0 || leadingSpaceCount >= gramCount)
            {
                throw new ArgumentOutOfRangeException(nameof(leadingSpaceCount), leadingSpaceCount, "Must be 0 or larger, and less than gramCount");
            }
            if (trailingSpaceCount < 0 || trailingSpaceCount >= gramCount)
            {
                throw new ArgumentOutOfRangeException(nameof(trailingSpaceCount), trailingSpaceCount, "Must be 0 or larger, and less than gramCount");
            }

            normalize = normalizeFunc;
            leadingSpaces = new string(' ', leadingSpaceCount);
            trailingSpaces = new string(' ', trailingSpaceCount);

        }

        public void AddEntry(string entry, DictionaryEntry dictionaryEntry)
        {
            var ngrams = ToNGrams(entry);
            var metaData = new EntryMetadata(dictionaryEntry, ngrams.Count());

            foreach(var ngram in ngrams)
            {
                if (entries.ContainsKey(ngram))
                {
                    entries[ngram].Add(metaData);
                } else
                {
                    entries[ngram] = new HashSet<EntryMetadata> { metaData };
                }

            }
        }

        public IEnumerable<string> GetSuggestions(string root)
        {
            var nGrams = ToNGrams(root);
            var nGramcount = nGrams.Count();

            return nGrams
                .Where(x => entries.ContainsKey(x))
                .SelectMany(x => entries[x])
                .GroupBy(x => x)
                .Select(x => new {
                    MetaData = x.Key,
                    Score = CalculateScore(x.Count(), nGramcount, x.Key.NGramCount)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.MetaData.DictionaryEntry.UsageCount)
                .Select(x => x.MetaData.DictionaryEntry.Entry);
        }

        public void RemoveEntry(string entry)
        {
            foreach(var trigram in ToNGrams(entry))
            {
                if (entries.ContainsKey(trigram))
                {
                    entries[trigram].RemoveWhere(x => x.DictionaryEntry.Entry == entry);
                }
            }
        }

        private IEnumerable<string> ToNGrams(string word)
        {
            var normalizedWord = leadingSpaces + normalize(word) + trailingSpaces;
            for (int i = 0; i < normalizedWord.Length-2; i++)
            {
                yield return normalizedWord.Substring(i, 3);
            }
        }

        private double CalculateScore(double numberOfMatches, double numberOfRootNGrams, double numberOfEntryNGrams)
        {
            return 2 * numberOfMatches / (numberOfRootNGrams + numberOfEntryNGrams);
        }

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
