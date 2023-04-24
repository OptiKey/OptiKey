using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Models.Gamepads
{

    public struct RepeatableTriggerSignal
    {
        public RepeatableTriggerSignal(TriggerSignal triggerSignal, bool isRepeat)
        {
            this.triggerSignal = triggerSignal;
            this.isRepeat = isRepeat;
            this.isRepeatAllowed = false; // to be updated when context is known
        }

        public TriggerSignal triggerSignal;
        public bool isRepeat;
        public bool isRepeatAllowed;
    }
}
