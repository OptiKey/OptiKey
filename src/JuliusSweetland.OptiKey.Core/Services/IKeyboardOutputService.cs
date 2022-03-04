// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IKeyboardOutputService : INotifyPropertyChanged
    {
        string Text { get; set; }
        void ProcessFunctionKey(FunctionKeys functionKey);
        void ProcessSingleKeyText(string capturedText);
        Task ProcessSingleKeyPress(string key, KeyPressKeyValue.KeyPressType type);
        void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions);
    }
}
