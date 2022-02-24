using log4net;
using SharpDX.DirectInput;
using System;
using System.Reflection;

namespace JuliusSweetland.OptiKey.Models.Gamepads
{
    class DirectInputControllerState : IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DirectInputControllerState(Guid instanceGuid, long reconnectMs = 1000)
        {
            this.reconnectMs = reconnectMs;

            this.InstanceGuid = instanceGuid;
            this.PreviousButtons = new bool[128];
            this.CurrentButtons = new bool[128];
            this.ChangedButtons = new bool[128];

            // Instantiate the joystick
            TryConnect(true);
        }

        private void TryConnect(bool logFailure)
        {
            try
            {
                using (var directInput = new DirectInput())
                {
                    controller = new Joystick(directInput, InstanceGuid);
                    controller.Properties.BufferSize = 128;
                    controller.Acquire();
                    IsConnected = true;
                }
            }
            catch (SharpDX.SharpDXException exception)
            {
                if (logFailure)
                {
                    Log.Info($"Exception connecting to DirectInput Joystick/Gamepad with GUID: { InstanceGuid }");
                    Log.Info($"Exception: {exception.Descriptor.ApiCode}");
                }
                IsConnected = false;
                return;
            }            
        }
        
        private Joystick controller;
        private JoystickState state = new JoystickState();
        private long reconnectMs;
        private DateTime disconnectedTime;

        public Guid InstanceGuid { get; private set; }

        public bool[] CurrentButtons { get; private set; }
        public bool[] PreviousButtons { get; private set; }

        private bool[] changedButtons;
        public bool[] ChangedButtons
        {
            get
            {
                arrayXor(PreviousButtons, CurrentButtons, ref changedButtons);
                return changedButtons;
            }
            private set { changedButtons = value; }
        }

        private bool isConnected;
        public bool IsConnected
        {
            get
            {
                // Attempt reconnect if enough time has passed since last attempt
                if (!isConnected && DateTime.Now.Subtract(disconnectedTime).TotalMilliseconds > reconnectMs)
                {
                    TryConnect(false);
                }
                return isConnected;
            }
            private set
            {
                // Log any changes
                if (!isConnected && value == true)
                {                    
                    Log.Info($"Connected to DirectInput controller with Guid {InstanceGuid}");
                }
                else if (isConnected && value == false)
                {
                    Log.Info($"DirectInput controller with Guid {InstanceGuid} is disconnected");
                    disconnectedTime = DateTime.Now;
                }

                isConnected = value;
            }
        }

        // Return value = success boolean
        public bool UpdateButtons()
        {
            if (IsConnected)
            {
                try
                {
                    Array.Copy(CurrentButtons, PreviousButtons, CurrentButtons.Length);
                    controller.GetCurrentState(ref state);
                    CurrentButtons = state.Buttons;

                    IsConnected = true;
                }
                catch (SharpDX.SharpDXException ex)
                {
                    //TODO: log.debug?
                    //Log.Info($"SharpDX exception: {ex.Descriptor.ApiCode}"); // check which apicode?
                    IsConnected = false;
                }
            }
            return IsConnected;
        }

        private static void arrayXor(bool[] arr1, bool[] arr2, ref bool[] xor)
        {
            int length = Math.Min(xor.Length, Math.Min(arr1.Length, arr2.Length));
            for (int i = 0; i < length; i++)
            {
                xor[i] = arr1[i] ^ arr2[i];
            }
        }

        public void Dispose()
        {
            try
            {
                controller?.Dispose();
                controller = null;
            }
            catch (SharpDX.SharpDXException ex)
            {
                Log.Info($"SharpDX exception attempting to release device:\n{ex.Descriptor.ApiCode}");
            }
        }
    }
}
