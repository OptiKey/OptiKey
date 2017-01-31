using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services.Suggestions
{
    /// <summary>
    ///     Defines a management interface on top of an <see cref="ISuggestions" /> implementation. It allows dictionary
    ///     entries to be added and removed, thus affecting the returned suggestions.
    /// </summary>
    /// <remarks>This class is for management of an underlying provider and so is declared <c>internal</c>.</remarks>
    internal interface IManagedSuggestions : ISuggestions
    {
        void AddEntry(string entry, DictionaryEntry metaData);

        /// <summary>
        /// Removes all possible suggestions from the suggestions provider.
        /// </summary>
        void Clear();

        void RemoveEntry(string entry);
    }
}