using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Models.Gamepads
{
    public enum EventType
    {
        DOWN,
        UP
    }

    public struct GamepadButtonEvent<ButtonType>
    {   
        public GamepadButtonEvent(EventType type, ButtonType button, bool isRepeat = false)
        {
            this.eventType = type;
            this.button = button;
            this.isRepeat = isRepeat;
        }

        public ButtonType button;
        public bool isRepeat;
        public EventType eventType;
    }
}
