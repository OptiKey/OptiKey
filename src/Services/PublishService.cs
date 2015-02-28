using System;
using System.Linq;
using System.Windows;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Static;
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
                Log.Debug(string.Format("Checking all virtual key codes and simulating release of any which are down."));
                foreach (var virtualKeyCode in Enum.GetValues(typeof(VirtualKeyCode)).Cast<VirtualKeyCode>())
                {
                    if (inputDeviceStateAdaptor.IsHardwareKeyDown(virtualKeyCode))
                    {
                        Log.Debug(string.Format("{0} is down - calling KeyUp", virtualKeyCode));
                        KeyUp(virtualKeyCode);
                    }
                }
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void KeyDown(VirtualKeyCode virtualKeyCode)
        {
            try
            {
                Log.Debug(string.Format("Simulating key down {0}", virtualKeyCode));
                inputSimulator.Keyboard.KeyDown(virtualKeyCode);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void KeyUp(VirtualKeyCode virtualKeyCode)
        {
            try
            {
                Log.Debug(string.Format("Simulating key up: {0}", virtualKeyCode));
                inputSimulator.Keyboard.KeyUp(virtualKeyCode);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void KeyDownUp(VirtualKeyCode virtualKeyCode)
        {
            try
            {
                Log.Debug(string.Format("Simulating key press (down & up): {0}", virtualKeyCode));
                inputSimulator.Keyboard.KeyPress(virtualKeyCode);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void TypeText(string text)
        {
            try
            {
                Log.Debug(string.Format("Simulating typing text '{0}'", text));
                inputSimulator.Keyboard.TextEntry(text);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void MouseMouseToPoint(Point point)
        {
            try
            {
                Log.Debug(string.Format("Simulating moving mouse to point '{0}'", point));

                var scaledVirtualScreenWidth = SystemParameters.VirtualScreenWidth * ((double) Graphics.DpiX / (double) 96);
                var scaledVirtualScreenHeight = SystemParameters.VirtualScreenHeight * ((double)Graphics.DpiY / (double)96);

                inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(
                    ((double)65535 * point.X) / (double)scaledVirtualScreenWidth,
                    ((double)65535 * point.Y) / (double)scaledVirtualScreenHeight);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void LeftButtonClick()
        {
            try
            {
                Log.Debug("Simulating clicking the left mouse button click");
                inputSimulator.Mouse.LeftButtonClick();
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
