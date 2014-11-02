using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Services
{
    public interface IPublishService
    {
        void SynchroniseKeyboardState();
        void KeyDown(FunctionKeys? functionKey, char? character);
        void KeyUp(FunctionKeys? functionKey, char? character);
        void KeyPress(FunctionKeys? functionKey, char? character);
    }
}
