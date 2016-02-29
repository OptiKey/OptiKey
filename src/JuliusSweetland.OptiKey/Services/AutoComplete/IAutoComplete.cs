using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
    /// <summary>
    ///     Supports fetching a list of auto-complete suggestions, given an initial word.
    /// </summary>
    public interface IAutoComplete
    {
        /// <summary>
        ///     Returns an enumerable collection of auto-complete suggestions.
        /// </summary>
        /// <param name="root">The initial word stem on which to base auto-complete predictions.</param>
        /// <returns>An enumerable collection of words.</returns>
        IEnumerable<string> GetSuggestions(string root);
    }
}