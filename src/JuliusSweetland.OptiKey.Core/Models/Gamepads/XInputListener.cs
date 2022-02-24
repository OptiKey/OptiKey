using System;
using System.ComponentModel;
using System.Threading;
using SharpDX.XInput;
using System.Linq;
using log4net;
using System.Collections.Generic;
using System.Reflection;

namespace JuliusSweetland.OptiKey.Models.Gamepads
{
    public class XInputListener : IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Singleton instance allows multiple callers to subscribe to one or more controllers
        private static readonly Lazy<XInputListener> instance =
            new Lazy<XInputListener>(() => new XInputListener(UserIndex.Any));

        public static XInputListener Instance { get { return instance.Value; } }

        #region event-handling

        public class XInputButtonEventArgs : EventArgs
        {
            public XInputButtonEventArgs(UserIndex userIndex, EventType type, GamepadButtonFlags button)
            {
                this.userIndex = userIndex;
                this.eventType = type;
                this.button = button;
            }

            public UserIndex userIndex;
            public GamepadButtonFlags button;
            public EventType eventType;        
        }
        
        public event XInputButtonDownEventHandler ButtonDown;
        public event XInputButtonUpEventHandler ButtonUp;
        public delegate void XInputButtonDownEventHandler(object sender, XInputButtonEventArgs e);
        public delegate void XInputButtonUpEventHandler(object sender, XInputButtonEventArgs e);

        #endregion

        private List<XInputControllerState> connections = new List<XInputControllerState>();
        private UserIndex requestedUserIndex;

        private BackgroundWorker pollWorker;
        private int pollDelayMs = 20;

        public static bool IsDeviceAvailable(UserIndex userIndex)
        {           
            return new Controller(userIndex).IsConnected;
        }    

        public XInputListener(UserIndex userIndex)
        {            
            this.requestedUserIndex = userIndex;

            pollWorker = new BackgroundWorker();
            pollWorker.DoWork += pollGamepadButtons;
            pollWorker.WorkerSupportsCancellation = true;
            pollWorker.RunWorkerAsync();
        }

        public void Dispose()
        {
            pollWorker.CancelAsync();
            pollWorker.Dispose();
        }

        private void TryConnect(UserIndex index)
        {
            // Recurse for "Any" (not implemented in SharpDX so we manage it ourselves)
            if (index == UserIndex.Any)
            {
                // Connect all 
                foreach (UserIndex i in Enum.GetValues(typeof(UserIndex)))
                {
                    if (i != UserIndex.Any)
                    {
                        TryConnect(i);
                    }
                }
            }
            // Actually connect to individual controller
            else
            {
                var conn = new XInputControllerState(index);
                connections.Add(conn);
            }
        }

        private void pollGamepadButtons(object sender, DoWorkEventArgs e)
        {
            TryConnect(requestedUserIndex);

            while (true)
            {
                if (pollWorker.CancellationPending) { return; }

                foreach (var conn in connections)
                {
                    if (conn.UpdateButtons()) // will return false if not connected
                    {
                        GamepadButtonFlags changedButtons = conn.ChangedButtons;
                        if (changedButtons > 0)
                        {
                            var splitButtonsChanged = Enum.GetValues(typeof(GamepadButtonFlags))
                                                        .Cast<GamepadButtonFlags>()
                                                        .Where(b => b != GamepadButtonFlags.None && changedButtons.HasFlag(b));
                            foreach (GamepadButtonFlags b in splitButtonsChanged)
                            {
                                if ((conn.CurrentButtons & b) > 0)
                                    this.ButtonDown?.Invoke(this, new XInputButtonEventArgs(conn.UserIndex, EventType.DOWN, b));
                                else
                                    this.ButtonUp?.Invoke(this, new XInputButtonEventArgs(conn.UserIndex, EventType.UP, b));
                            }
                        }
                    }
                }
                Thread.Sleep(pollDelayMs);
            }
        }        
    }
}
