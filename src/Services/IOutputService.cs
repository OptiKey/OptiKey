using System.Collections.Generic;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Services
{
    public interface IOutputService
    {
        string Text { get; }
        
        void ProcessCapture(FunctionKeys functionKey);
        void ProcessCapture(string textCapture);
        void ProcessCapture(List<string> captureAndSuggestions);
    }
}
