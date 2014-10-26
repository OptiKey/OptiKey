using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Services
{
    public interface IOutputService
    {
        string Text { get; }
        void ProcessCapture(FunctionKeys? functionKey, string chars);
    }
}
