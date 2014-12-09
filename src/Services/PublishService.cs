using System;
using JuliusSweetland.ETTA.Models;
using log4net;

namespace JuliusSweetland.ETTA.Services
{
    public class PublishService : IPublishService
    {
        private readonly InputSimulator.InputSimulator inputSimulator;
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<Exception> Error;

        public PublishService()
        {
            inputSimulator = new InputSimulator.InputSimulator();
        }

        public void PublishModifiedKeyStroke(VirtualKeyCodeSet virtualKeyCodeSet)
        {
            try
            {
                Log.Debug(string.Format("Publishing virtualKeyCodeSet '{0}'", virtualKeyCodeSet));

                inputSimulator.Keyboard.ModifiedKeyStroke(
                    virtualKeyCodeSet.ModifierKeyCodes, virtualKeyCodeSet.KeyCodes);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void PublishText(string text)
        {
            try
            {
                Log.Debug(string.Format("Publishing text '{0}'", text));

                inputSimulator.Keyboard.TextEntry(text);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        private void PublishError(object sender, Exception ex)
        {
            if (Error != null)
            {
                Log.Error("Publishing Error event", ex);

                Error(sender, ex);
            }
        }
    }
}
