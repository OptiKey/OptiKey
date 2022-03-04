using log4net;
using SharpDX.XInput;

namespace JuliusSweetland.OptiKey.Models.Gamepads
{
    class XInputControllerState
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public XInputControllerState(UserIndex userIndex)
        {
            this.controller = new Controller(userIndex);
            this.UserIndex = userIndex;            
            this.CurrentButtons = GamepadButtonFlags.None;
            this.PreviousButtons = GamepadButtonFlags.None;
            this.state = new State();

            IsConnected = controller.IsConnected;
        }

        private Controller controller;
        private State state;

        public UserIndex UserIndex { get; private set; }
        public GamepadButtonFlags CurrentButtons { get; private set; }
        public GamepadButtonFlags PreviousButtons { get; private set; }
        public GamepadButtonFlags ChangedButtons { get { return CurrentButtons ^ PreviousButtons; } }

        private bool isConnected;
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
            private set
            {
                // Log any changes to Connected state
                if (!isConnected && value == true)
                    Log.Info($"Connected to XInput controller with UserIndex {UserIndex}");
                else if (isConnected && value == false)
                    Log.Info($"XInput controller with UserIndex {UserIndex} is disconnected");
                isConnected = value;
            }
        }

        // Return value = success boolean
        public bool UpdateButtons()
        {
            if (controller.GetState(out state)) // returns false if not connected
            {
                IsConnected = true;
                PreviousButtons = CurrentButtons;
                CurrentButtons = state.Gamepad.Buttons;
            }
            else
            {
                IsConnected = false;
            }
            return IsConnected;
        }
    }
}
