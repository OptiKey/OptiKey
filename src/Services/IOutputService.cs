using System.Collections.Generic;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Services
{
    public interface IOutputService
    {
        string Text { get; }
        
        void ProcessFunctionKey(FunctionKeys functionKey);
        void ProcessSingleKeyText(string capturedText);
        void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions);
    }
}
