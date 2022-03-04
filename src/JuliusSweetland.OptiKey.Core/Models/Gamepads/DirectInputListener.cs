using System;
using System.ComponentModel;
using System.Threading;
using SharpDX.DirectInput;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace JuliusSweetland.OptiKey.Models.Gamepads
{
    public class DirectInputListener : IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Singleton instance allows multiple callers to subscribe to one or more controllers
        private static readonly Lazy<DirectInputListener> instance =
            new Lazy<DirectInputListener>(() => new DirectInputListener(Guid.Empty));
        public static DirectInputListener Instance { get { return instance.Value; } }

        #region event-handling

        public class DirectInputButtonEventArgs : EventArgs
        {
            public DirectInputButtonEventArgs(Guid controller, EventType type, int button)
            {
                this.controller = controller;
                this.eventType = type;
                this.button = button;
            }

            public Guid controller;
            public int button;
            public EventType eventType;
        }

        public event DirectInputButtonDownEventHandler ButtonDown;
        public event DirectInputButtonUpEventHandler ButtonUp;
        public delegate void DirectInputButtonDownEventHandler(object sender, DirectInputButtonEventArgs e);
        public delegate void DirectInputButtonUpEventHandler(object sender, DirectInputButtonEventArgs e);

        #endregion

        private Guid requestedGuid;
        Dictionary<Guid, DirectInputControllerState> controllers = new Dictionary<Guid, DirectInputControllerState>();
        private JoystickState state = new JoystickState();

        private BackgroundWorker pollWorker;
        private DateTime lastTimeScanned;
        private int pollDelayMs;
        private int scanDelayMs;        

        public DirectInputListener(Guid controllerGuid, int pollDelayMs = 20, int scanDelayMs = 2000)
        {
            this.pollDelayMs = pollDelayMs;
            this.scanDelayMs = scanDelayMs;
            this.requestedGuid = controllerGuid;

            pollWorker = new BackgroundWorker();
            pollWorker.DoWork += pollGamepadButtons;
            pollWorker.WorkerSupportsCancellation = true;
            pollWorker.RunWorkerAsync();
        }

        public static List<KeyValuePair<Guid, string>> GetConnectedControllers()
        {
            var controllers = new List<KeyValuePair<Guid, string>>();

            using (var directInput = new DirectInput())
            {
                List<DeviceInstance> devices = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices).Concat(
                                                directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices)).ToList();

                foreach (var deviceInstance in devices)
                {
                    // To distinguish controllers with the same product name, we append a short version of the guid to the product name
                    string shortGuid = deviceInstance.InstanceGuid.ToString().Substring(0, 8);
                    controllers.Add(new KeyValuePair<Guid, string>(deviceInstance.InstanceGuid, $"{ deviceInstance.ProductName } [{shortGuid}]"));
                }
            }

            return controllers;
        }

        public void Dispose()
        {
            Log.Info("Disposing DirectInputListener");

            foreach (var controller in controllers.Values)
            {
                controller.Dispose();
            }
            pollWorker?.CancelAsync();
            pollWorker?.Dispose();
        }

        private void TryConnect(Guid guid)
        {
            // We will interpret Empty as "all available controllers")
            if (guid == Guid.Empty)
            {
                TryConnectAll();
            }
            else
            {
                var conn = new DirectInputControllerState(guid);
                controllers.Add(guid, conn);
            }
        }

        private void TryConnectAll()
        {
            List<DeviceInstance> devices = new List<DeviceInstance>();
            using (var directInput = new DirectInput())
            {
                devices = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices).Concat(
                             directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices)).ToList();
            }
            foreach (var deviceInstance in devices)
            {
                Guid guid = deviceInstance.InstanceGuid;
                if (!controllers.ContainsKey(guid))
                {
                    TryConnect(guid);
                }
            }
            lastTimeScanned = DateTime.Now;
        }        

        private void pollGamepadButtons(object sender, DoWorkEventArgs e)
        {
            TryConnect(requestedGuid);

            while (true)
            {
                if (pollWorker.CancellationPending) { return; }

                // If polling all controllers, periodically re-scan for new ones
                if (requestedGuid == Guid.Empty && DateTime.Now.Subtract(lastTimeScanned).TotalMilliseconds > scanDelayMs)
                {
                    TryConnectAll();
                }

                // Now poll any controllers we are watching
                foreach (DirectInputControllerState controller in controllers.Values)
                {
                    if (controller.UpdateButtons())
                    {
                        var changedButtons = controller.ChangedButtons;
                        if (changedButtons.Count(b => b) > 0)
                        {
                            var buttonsWithIndex = changedButtons
                                                    .Select((bit, index) => new { Bit = bit, Index = index }); // projection, we will save bit indices

                            // now we will get indices of all true(1) bits [from left to right]
                            var indicesChanged = buttonsWithIndex.Where(x => x.Bit == true).Select(x => x.Index).ToArray();
                            //Log.Info($"indices changed: { String.Join(", ",indicesChanged) }");
                            foreach (var i in indicesChanged)
                            {
                                if (controller.CurrentButtons[i])
                                {
                                    this.ButtonDown?.Invoke(this, new DirectInputButtonEventArgs(controller.InstanceGuid, EventType.DOWN, i + 1));
                                }
                                else
                                {
                                    this.ButtonUp?.Invoke(this, new DirectInputButtonEventArgs(controller.InstanceGuid, EventType.UP, i + 1));
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(pollDelayMs);
            }
        }

        #region Properties

        public bool IsConnected
        {
            get; set;
        }

        #endregion

    }
}
