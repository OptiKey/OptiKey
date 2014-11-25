using System.Collections.Generic;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Services
{
    public interface IOutputService
    {
        string Text { get; }
        
        void ProcessCapture(FunctionKeys functionKey);
        void ProcessCapture(string capturedText);
        void ProcessCapture(List<string> captureAndSuggestions);
    }
}
