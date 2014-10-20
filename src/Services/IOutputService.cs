using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Services
{
    public interface IOutputService
    {
        string Text { get; }
        void ClearText();
        void ProcessCapture(string capture);
        void ProcessCapture(FunctionKeys capture);
        void ProcessBackOne();
        void ProcessBackMany();
        void SwapLastCaptureForSuggestion(string suggestion);
    }
}
