namespace JuliusSweetland.ETTA.Native.Enums
{
    /// <summary>
    /// Specifies the type of the input event. This member can be one of the following values. 
    /// </summary>
    internal enum MapVirtualKeyType : uint // UInt32
    {
        /// <summary>
        /// uCode is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the function returns 0.
        /// </summary>
        VirtualKeyCodeToScanCode = 0x0,

        /// <summary>
        /// uCode is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys. If there is no translation, the function returns 0.
        /// </summary>
        ScanCodeToVirtualKeyCode = 0x1,

        /// <summary>
        /// uCode is a virtual-key code and is translated into an unshifted character value in the low-order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation, the function returns 0.
        /// </summary>
        VirtualKeyCodeToCharacter = 0x2,

        /// <summary>
        /// uCode is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys. If there is no translation, the function returns 0.
        /// </summary>
        ScanCodeToVirtualKeyCodeExtended = 0x3,
    }
}
