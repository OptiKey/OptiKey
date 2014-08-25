using System.Collections.Generic;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Models
{
    public interface IKeyboardStateInfo
    {
        NotifyingConcurrentDictionary<KeyDownStates> KeyDownStates { get; }
        List<string> Suggestions { get; }
        int SuggestionsPage { get; }
        int SuggestionsPerPage { get; }
    }
}
