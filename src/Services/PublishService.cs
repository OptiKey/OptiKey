using System;
using System.Linq;
using WindowsInput.Native;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class PublishService : IPublishService
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly WindowsInput.InputSimulator inputSimulator;
        private readonly WindowsInput.WindowsInputDeviceStateAdaptor inputDeviceStateAdaptor;

        public event EventHandler<Exception> Error;
        
        public PublishService()
        {
            inputSimulator = new WindowsInput.InputSimulator();
            inputDeviceStateAdaptor = new WindowsInput.WindowsInputDeviceStateAdaptor();
        }

        public void ReleaseAllDownKeys()
        {
            try
            {
                Log.Debug(string.Format("Checking all virtual key codes and releasing any which are down."));
                foreach (var virtualKeyCode in Enum.GetValues(typeof(VirtualKeyCode)).Cast<VirtualKeyCode>())
                {
                    if (inputDeviceStateAdaptor.IsHardwareKeyDown(virtualKeyCode))
                    {
                        Log.Debug(string.Format("{0} is down - calling PublishKeyUp", virtualKeyCode));
                        PublishKeyUp(virtualKeyCode);
                    }
                }
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void PublishKeyDown(VirtualKeyCode virtualKeyCode)
        {
            try
            {
                Log.Debug(string.Format("Publishing key down {0}", virtualKeyCode));
                inputSimulator.Keyboard.KeyDown(virtualKeyCode);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void PublishKeyUp(VirtualKeyCode virtualKeyCode)
        {
            try
            {
                Log.Debug(string.Format("Publishing key up: {0}", virtualKeyCode));
                inputSimulator.Keyboard.KeyUp(virtualKeyCode);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void PublishKeyPress(VirtualKeyCode virtualKeyCode)
        {
            try
            {
                Log.Debug(string.Format("Publishing key press: {0}", virtualKeyCode));
                inputSimulator.Keyboard.KeyPress(virtualKeyCode);
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
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }
    }
}
