using System.Collections.Generic;
using WindowsInput.Native;

namespace JuliusSweetland.ETTA.Models
{
    public struct VirtualKeyCodeSet
    {
        public IEnumerable<VirtualKeyCode> ModifierKeyCodes { get; set; }
        public IEnumerable<VirtualKeyCode> KeyCodes { get; set; }
    }
}
