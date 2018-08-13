using System;
using System.ComponentModel;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IKeyStateService : INotifyPropertyChanged
    {
        bool SimulateKeyStrokes { get; set; }
        NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get; }
        NotifyingConcurrentDictionary<KeyValue, KeyDownStates> KeyDownStates { get; }
        NotifyingConcurrentDictionary<KeyValue, bool> KeyHighlightStates { get; }
        KeyEnabledStates KeyEnabledStates { get; }

        void SetKeyHighlightState(KeyValue keyValue, bool highlight);
        void ProgressKeyDownState(KeyValue keyValue);
    }
}
