// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.ComponentModel;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IKeyboardOutputService : INotifyPropertyChanged
    {
        string Text { get; }

        void ProcessFunctionKey(FunctionKeys functionKey);
        void ProcessSingleKeyText(string capturedText);
        void ProcessSingleKeyPress(string key, KeyPressKeyValue.KeyPressType type, int delayMs = 0);
        void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions);
    }
}
