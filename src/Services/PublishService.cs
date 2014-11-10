using WindowsInput;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public class PublishService : IPublishService
    {
        private readonly InputSimulator inputSimulator;

        public PublishService()
        {
            inputSimulator = new InputSimulator();
        }

        public void PublishModifiedKeyStroke(VirtualKeyCodeSet virtualKeyCodeSet)
        {
            inputSimulator.Keyboard.ModifiedKeyStroke(
                virtualKeyCodeSet.ModifierKeyCodes, virtualKeyCodeSet.KeyCodes);
        }

        public void PublishText(string text)
        {
            inputSimulator.Keyboard.TextEntry(text);
        }
    }
}
