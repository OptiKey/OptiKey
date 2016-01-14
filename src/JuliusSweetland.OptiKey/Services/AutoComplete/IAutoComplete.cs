using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Services.AutoComplete
{
    internal interface IAutoComplete
    {
        IEnumerable<string> GetSuggestions(string root);
    }
}