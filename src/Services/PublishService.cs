using WindowsInput;
using JuliusSweetland.ETTA.Models;
using log4net;

namespace JuliusSweetland.ETTA.Services
{
    public class PublishService : IPublishService
    {
        private readonly InputSimulator inputSimulator;
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PublishService()
        {
            inputSimulator = new InputSimulator();
        }

        public void PublishModifiedKeyStroke(VirtualKeyCodeSet virtualKeyCodeSet)
        {
            Log.Debug(string.Format("Publishing virtualKeyCodeSet '{0}'", virtualKeyCodeSet));

            inputSimulator.Keyboard.ModifiedKeyStroke(
                virtualKeyCodeSet.ModifierKeyCodes, virtualKeyCodeSet.KeyCodes);
        }

        public void PublishText(string text)
        {
            Log.Debug(string.Format("Publishing text '{0}'", text));

            inputSimulator.Keyboard.TextEntry(text);
        }
    }
}
