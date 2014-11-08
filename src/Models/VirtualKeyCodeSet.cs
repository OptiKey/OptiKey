using System.Collections.Generic;
using WindowsInput.Native;

namespace JuliusSweetland.ETTA.Models
{
    public struct VirtualKeyCodeSet
    {
        public List<VirtualKeyCode> ModifierKeyCodes { get; set; }
        public List<VirtualKeyCode> KeyCodes { get; set; }
    }
}
