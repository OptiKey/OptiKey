using System.Collections.Generic;
using System.ComponentModel;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Models
{
    public interface IKeyboardStateInfo : INotifyPropertyChanged
    {
        bool CapturingMultiKeySelection { get; }
        NotifyingConcurrentDictionary<KeyDownStates> KeyDownStates { get; }
        List<string> Suggestions { get; }
        int SuggestionsPage { get; }
        int SuggestionsPerPage { get; }
    }
}
