using System.Collections.Generic;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IKeyboardOutputService
    {
        string Text { get; }

        bool AutoPressShiftIfAppropriate();
        void ProcessFunctionKey(FunctionKeys functionKey);
        void ProcessSingleKeyText(string capturedText);
        void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions);
    }
}
