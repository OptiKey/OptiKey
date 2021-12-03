// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Linq;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class PublishService : IPublishService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly InputSimulator inputSimulator;
        private readonly WindowsInputDeviceStateAdaptor inputDeviceStateAdaptor;

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
                Log.InfoFormat("Releasing all keys (with virtual key codes) which are down.");
                foreach (var virtualKeyCode in Enum.GetValues(typeof(VirtualKeyCode)).Cast<VirtualKeyCode>())
                {
                    if (inputDeviceStateAdaptor.IsHardwareKeyDown(virtualKeyCode))
                    {
                        Log.DebugFormat("{0} is down - calling KeyUp", virtualKeyCode);
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
                Log.DebugFormat("Simulating key down {0}", virtualKeyCode);
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
                Log.DebugFormat("Simulating key up: {0}", virtualKeyCode);
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
                Log.DebugFormat("Simulating key press (down & up): {0}", virtualKeyCode);
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
                Log.DebugFormat("Simulating typing text '{0}'", text);
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
                Log.DebugFormat("Simulating moving mouse to point '{0}'", point);
                
                //N.B. InputSimulator does not deal in pixels. The position should be a scaled point between 0 and 65535. 
                //https://inputsimulator.codeplex.com/discussions/86530

                inputSimulator.Mouse.MoveMouseTo(
                    Math.Ceiling(65535 * (point.X / Graphics.PrimaryScreenWidthInPixels)),
                    Math.Ceiling(65535 * (point.Y / Graphics.PrimaryScreenHeightInPixels)));
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void LeftMouseButtonClick()
        {
            try
            {
                Log.Info("Simulating clicking the left mouse button click");
                inputSimulator.Mouse.LeftButtonClick();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void LeftMouseButtonDoubleClick()
        {
            try
            {
                Log.Info("Simulating pressing the left mouse button down twice");
                inputSimulator.Mouse.LeftButtonDoubleClick();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void LeftMouseButtonDown()
        {
            try
            {
                Log.Info("Simulating pressing the left mouse button down");
                inputSimulator.Mouse.LeftButtonDown();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void LeftMouseButtonUp()
        {
            try
            {
                Log.Info("Simulating releasing the left mouse button down");
                inputSimulator.Mouse.LeftButtonUp();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void MiddleMouseButtonClick()
        {
            try
            {
                Log.Info("Simulating clicking the middle mouse button click");
                inputSimulator.Mouse.MiddleButtonClick();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void MiddleMouseButtonDown()
        {
            try
            {
                Log.Info("Simulating pressing the middle mouse button down");
                inputSimulator.Mouse.MiddleButtonDown();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void MiddleMouseButtonUp()
        {
            try
            {
                Log.Info("Simulating releasing the middle mouse button down");
                inputSimulator.Mouse.MiddleButtonUp();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void RightMouseButtonClick()
        {
            try
            {
                Log.Info("Simulating pressing the right mouse button down");
                inputSimulator.Mouse.RightButtonClick();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void RightMouseButtonDown()
        {
            try
            {
                Log.Info("Simulating pressing the right mouse button down");
                inputSimulator.Mouse.RightButtonDown();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void RightMouseButtonUp()
        {
            try
            {
                Log.Info("Simulating releasing the right mouse button down");
                inputSimulator.Mouse.RightButtonUp();
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void ScrollMouseWheelUp(int clicks)
        {
            try
            {
                Log.DebugFormat("Simulating scrolling the vertical mouse wheel up by {0} clicks", clicks);
                inputSimulator.Mouse.VerticalScroll(clicks);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void ScrollMouseWheelDown(int clicks)
        {
            try
            {
                Log.DebugFormat("Simulating scrolling the vertical mouse wheel down by {0} clicks", clicks);
                inputSimulator.Mouse.VerticalScroll(0 - clicks);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void ScrollMouseWheelLeft(int clicks)
        {
            try
            {
                Log.DebugFormat("Simulating scrolling the horizontal mouse wheel left by {0} clicks", clicks);
                inputSimulator.Mouse.HorizontalScroll(0 - clicks);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void ScrollMouseWheelRight(int clicks)
        {
            try
            {
                Log.DebugFormat("Simulating scrolling the horizontal mouse wheel right by {0} clicks", clicks);
                inputSimulator.Mouse.HorizontalScroll(clicks);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void ScrollMouseWheelAbsoluteHorizontal(int amount)
        {
            try
            {
                Log.DebugFormat("Simulating scrolling the horizontal mouse wheel by {0} units", amount);
                var tmpMouseWheelClickSize = inputSimulator.Mouse.MouseWheelClickSize;
                inputSimulator.Mouse.MouseWheelClickSize = amount;
                inputSimulator.Mouse.HorizontalScroll(1); //Scroll by one click, which is the absolute amount temporarily set in MouseWheelClickSize
                inputSimulator.Mouse.MouseWheelClickSize = tmpMouseWheelClickSize;
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public void ScrollMouseWheelAbsoluteVertical(int amount)
        {
            try
            {
                Log.DebugFormat("Simulating scrolling the vertical mouse wheel by {0} units", amount);
                var tmpMouseWheelClickSize = inputSimulator.Mouse.MouseWheelClickSize;
                inputSimulator.Mouse.MouseWheelClickSize = amount;
                inputSimulator.Mouse.VerticalScroll(1); //Scroll by one click, which is the absolute amount temporarily set in MouseWheelClickSize
                inputSimulator.Mouse.MouseWheelClickSize = tmpMouseWheelClickSize;
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
