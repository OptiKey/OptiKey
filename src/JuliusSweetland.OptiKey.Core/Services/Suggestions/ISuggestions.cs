// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
        /// <param name="root">The initial text stem on which to base predictions.</param>
        /// <param name="nextWord">Whether to suggest the next word</param>
        /// <returns>An enumerable collection of words.</returns>
        IEnumerable<string> GetSuggestions(string root, bool nextWord = false);
    }
}