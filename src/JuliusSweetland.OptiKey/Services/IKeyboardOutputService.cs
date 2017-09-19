using System.Collections.Generic;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IKeyboardOutputService
    {
        string Text { get; }

        void ProcessFunctionKey(FunctionKeys functionKey);
        void ProcessSingleKeyText(string capturedText);
        void ProcessSingleKeyPress(string key, KeyPressKeyValue.KeyPressType type, int delayMs = 0);
        void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions);
    }
}
