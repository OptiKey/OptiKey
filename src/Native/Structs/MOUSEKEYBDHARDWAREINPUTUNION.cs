using System.Runtime.InteropServices;

namespace JuliusSweetland.ETTA.Native.Structs
{
    /// <summary>
    /// The combined/overlayed structure that includes Mouse, Keyboard and Hardware Input message data (see: http://msdn.microsoft.com/en-us/library/ms646270(VS.85).aspx)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct MOUSEKEYBDHARDWAREINPUTUNION
    {
        /// <summary>
        /// The <see cref="MOUSEINPUT"/> definition.
        /// </summary>
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;

        /// <summary>
        /// The <see cref="KEYBDINPUT"/> definition.
        /// </summary>
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;

        /// <summary>
        /// The <see cref="HARDWAREINPUT"/> definition.
        /// </summary>
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;

        public override string ToString()
        {
            return string.Format("\t\tMOUSEINPUT:\n{0}" +
                                 "\n\t\tKEYBDINPUT:\n{1}" +
                                 "\n\t\tHARDWAREINPUT:\n{2}", 
                                 Mouse, Keyboard, Hardware);
        }
    }
}
