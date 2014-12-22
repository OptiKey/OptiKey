using System.ComponentModel;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public interface IKeyboardService : INotifyPropertyChanged
    {
        NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get; }
        NotifyingConcurrentDictionary<KeyValue, KeyDownStates> KeyDownStates { get; }
        KeyEnabledStates KeyEnabledStates { get; }

        void ProgressKeyDownState(KeyValue keyValue);
    }
}
