// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Models;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services.Suggestions
{
    /// <summary>
    ///     Defines a management interface on top of an <see cref="ISuggestions" /> implementation. It allows dictionary
    ///     entries to be added and removed, thus affecting the returned suggestions.
    /// </summary>
    /// <remarks>This class is for management of an underlying provider and so is declared <c>internal</c>.</remarks>
    internal interface IManagedSuggestions : ISuggestions
    {
		/// <summary>
		///  Add entry to the managed suggestion entry storage
		/// </summary>
		/// <param name="entry">new entry string</param>
		/// <param name="metaData">dictionary entry object</param>
		/// <param name="normalizedHash">if the entry has already been normalized, provide it to save performance</param>
        void AddEntry(string entry, DictionaryEntry metaData, string normalizedHash = "");

        /// <summary>
        ///  Removes all possible suggestions from the suggestions provider.
        /// </summary>
        void Clear();

		/// <summary>
		///  Attempt to remove an entry from the storage
		/// </summary>
		/// <param name="entry">the entry to remove</param>
        void RemoveEntry(string entry);

		/// <summary>
		///  Get words hashes set where entries have been normalized and stored at.
		/// </summary>
		/// <returns>hash set containing normalized entries</returns>
		HashSet<string> GetWordsHashes();

		/// <summary>
		///  Get full dictionary entries set.
		/// </summary>
		/// <returns>dictionary where key is the word hash, and value is the hashset containing all entries with the same hash</returns>
		Dictionary<string, HashSet<DictionaryEntry>> GetEntries();
	}
}