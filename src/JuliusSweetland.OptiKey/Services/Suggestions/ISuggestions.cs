using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services.Suggestions
{
    /// <summary>
    ///     Supports fetching a list of auto-complete or next word suggestions given an initial root context
    /// </summary>
    public interface ISuggestions
    {
        /// <summary>
        ///     Returns an enumerable collection of suggestions.
        /// </summary>
        /// <param name="root">The initial word stem on which to base predictions.</param>
        /// <returns>An enumerable collection of words.</returns>
        IEnumerable<string> GetSuggestions(string root);
    }
}