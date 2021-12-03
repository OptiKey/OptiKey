// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JuliusSweetland.OptiKey.Services.Suggestions
{
    /// <summary>
    /// An auto suggest class using the n-gram algorithm.
    /// https://en.wikipedia.org/wiki/N-gram
    /// n-grams provide a quick way to do a fuzzy search that works decently across a wide range of languages.
    /// </summary>
    public class NGramAutoComplete : IManagedSuggestions
	{
		private readonly Dictionary<string, HashSet<DictionaryEntry>> entries = new Dictionary<string, HashSet<DictionaryEntry>>();
		private readonly HashSet<string> wordsIndex = new HashSet<string>();

		private readonly Func<string, string> normalize;
        private readonly int gramCount;
        private readonly string leadingSpaces;
        private readonly string trailingSpaces;

        private static readonly Func<string, string> DefaultNormaliseFunc = x => x.Normalise();
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Creates a n-gram auto-completer using the default settings.
        /// </summary>
        public NGramAutoComplete(int gramCount = 3, int leadingSpaceCount = 2, int trailingSpaceCount = 1)
            : this(DefaultNormaliseFunc, gramCount, leadingSpaceCount, trailingSpaceCount)
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

		public Dictionary<string, HashSet<DictionaryEntry>> GetEntries()
		{
			return entries;
		}

		public HashSet<string> GetWordsHashes()
		{
			return wordsIndex;
		}

		public void AddEntry(string entry, DictionaryEntry dictionaryEntry, string normalizedHash = "")
        {
            if (entry.Contains(" "))
            {
                //Entry is a phrase - also add with a dictionary entry hash (first letter of each word)
                var phraseAutoCompleteHash = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(false);
                AddEntry(phraseAutoCompleteHash, dictionaryEntry);
            }

			normalizedHash = string.IsNullOrWhiteSpace(normalizedHash)
								? entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(false)
								: normalizedHash;

			if (!string.IsNullOrWhiteSpace(normalizedHash))
			{
				AddToDictionaryWorker(entry, normalizedHash, dictionaryEntry);

				if (!wordsIndex.Contains(normalizedHash))
				{
					wordsIndex.Add(normalizedHash);
				}
			}

			var ngrams = ToNGrams(entry).ToList();
			var metaData = new EntryMetadata(dictionaryEntry.Entry, dictionaryEntry.UsageCount, ngrams.Count);

			foreach (var ngram in ngrams)
			{
				AddToDictionaryWorker(entry, ngram, metaData, true);
			}
		}

		private void AddToDictionaryWorker(string entry, string hash, DictionaryEntry dictEntry, bool isNGram = false)
		{
			if (string.IsNullOrWhiteSpace(entry) || string.IsNullOrWhiteSpace(hash) || dictEntry == null)
			{
				return;
			}

			if (entries.ContainsKey(hash))
			{
				if (isNGram || entries[hash].All(nwwuc => nwwuc.Entry != entry))
				{
					entries[hash].Add(dictEntry);
				}
			}
			else
			{
				entries.Add(hash, new HashSet<DictionaryEntry> { dictEntry });
			}
		}

		/// <summary>
		/// Removes all possible suggestions from the auto complete provider.
		/// </summary>
		public void Clear()
        {
            Log.Debug("Clear called.");
            entries.Clear();
        }

        public IEnumerable<string> GetSuggestions(string root, bool nextWord)
        {
            Log.DebugFormat("GetSuggestions called with root '{0}'.", root);

            var nGrams = ToNGrams(root).ToList();
            var nGramcount = nGrams.Count;

            return nGrams
                .Where(x => entries.ContainsKey(x))
                .SelectMany(x => entries[x])
                .GroupBy(x => x)
                .Select(x =>
				{ 
					double NGramCount = (x.Key is EntryMetadata) ? ((EntryMetadata)x.Key).NGramCount : 0;
					return new
					{
						MetaData = x.Key,
						Score = CalculateScore(x.Count(), nGramcount, NGramCount)
					};
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.MetaData.UsageCount)
                .Select(x => x.MetaData.Entry);
        }

        public void RemoveEntry(string entry)
        {
            foreach (var trigram in ToNGrams(entry))
			{
				RemoveEntryWorker(entry, trigram);
            }

			// also remove the normalized entry from the index and entries storage.
			var normalizedEntry = entry.NormaliseAndRemoveRepeatingCharactersAndHandlePhrases(log: false);
			RemoveEntryWorker(entry, normalizedEntry);
			wordsIndex.Remove(normalizedEntry);
		}

		private void RemoveEntryWorker(string entry, string hash)
		{
			if (!string.IsNullOrWhiteSpace(hash)
					&& entries.ContainsKey(hash))
			{
				var foundEntry = entries[hash].FirstOrDefault(ewuc => ewuc.Entry == entry);

				if (foundEntry != null)
				{
					entries[hash].Remove(foundEntry);

					if (!entries[hash].Any())
					{
						entries.Remove(hash);
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
		internal class EntryMetadata : DictionaryEntry
		{
			public EntryMetadata(string entry, int usageCount, int nGramCount)
				: base(entry, usageCount)
			{
				NGramCount = nGramCount;
			}

			public int NGramCount { get; }
		}
	}
}
