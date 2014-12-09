using System;
using System.Collections.Generic;
using System.Text;
using VirtualKeyCode = JuliusSweetland.ETTA.Native.Enums.VirtualKeyCode;

namespace JuliusSweetland.ETTA.Models
{
    public struct VirtualKeyCodeSet
    {
        public List<VirtualKeyCode> ModifierKeyCodes { get; set; }
        public List<VirtualKeyCode> KeyCodes { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (ModifierKeyCodes != null)
            {
                sb.Append("ModifierKeyCodes:");
                sb.Append(String.Join(",", ModifierKeyCodes));
            }

            if (KeyCodes != null)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" | ");
                }

                sb.Append("KeyCodes:");
                sb.Append(String.Join(",", KeyCodes));
            }

            return sb.ToString();
        }
    }
}
