using System.Collections.Generic;
using System.ComponentModel;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;

namespace JuliusSweetland.ETTA.Models
{
    public interface IKeyboardStateManager : INotifyPropertyChanged
    {
        bool CapturingMultiKeySelection { get; }
        NotifyingConcurrentDictionary<KeyDownStates> KeyDownStates { get; }
        List<string> Suggestions { get; }
        int SuggestionsPage { get; }
        int SuggestionsPerPage { get; }
        IKeyboard Keyboard { set; }
    }
}
