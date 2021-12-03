// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
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
        NotifyingConcurrentDictionary<KeyValue, bool> KeyRunningStates { get; }
        List<Tuple<KeyValue, KeyValue>> KeyFamily { get; }
        IDictionary<string, List<KeyValue>> KeyValueByGroup { get; }
        KeyEnabledStates KeyEnabledStates { get; }

        void ClearKeyHighlightStates();
        void ProgressKeyDownState(KeyValue keyValue);
    }
}
