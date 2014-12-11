using System;
using System.Runtime.InteropServices;
using System.Text;
using JuliusSweetland.ETTA.Native.Enums;

namespace JuliusSweetland.ETTA.Native.Structs
{
    /// <summary>
    /// The MOUSEINPUT structure contains information about a simulated mouse event. (see: http://msdn.microsoft.com/en-us/library/ms646273(VS.85).aspx)
    /// Declared in Winuser.h, include Windows.h
    /// </summary>
    /// <remarks>
    /// If the mouse has moved, indicated by MOUSEEVENTF_MOVE, dx and dy specify information about that movement. The information is specified as absolute or relative integer values. 
    /// If MOUSEEVENTF_ABSOLUTE value is specified, dx and dy contain normalized absolute coordinates between 0 and 65,535. The event procedure maps these coordinates onto the display surface. Coordinate (0,0) maps onto the upper-left corner of the display surface; coordinate (65535,65535) maps onto the lower-right corner. In a multimonitor system, the coordinates map to the primary monitor. 
    /// Windows 2000/XP: If MOUSEEVENTF_VIRTUALDESK is specified, the coordinates map to the entire virtual desktop.
    /// If the MOUSEEVENTF_ABSOLUTE value is not specified, dx and dy specify movement relative to the previous mouse event (the last reported position). Positive values mean the mouse moved right (or down); negative values mean the mouse moved left (or up). 
    /// Relative mouse motion is subject to the effects of the mouse speed and the two-mouse threshold values. A user sets these three values with the Pointer Speed slider of the Control Panel's Mouse Properties sheet. You can obtain and set these values using the SystemParametersInfo function. 
    /// The system applies two tests to the specified relative mouse movement. If the specified distance along either the x or y axis is greater than the first mouse threshold value, and the mouse speed is not zero, the system doubles the distance. If the specified distance along either the x or y axis is greater than the second mouse threshold value, and the mouse speed is equal to two, the system doubles the distance that resulted from applying the first threshold test. It is thus possible for the system to multiply specified relative mouse movement along the x or y axis by up to four times.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        /// <summary>
        /// Specifies the absolute position of the mouse, or the amount of motion since the last mouse event was generated, depending on the value of the dwFlags member. Absolute data is specified as the x coordinate of the mouse; relative data is specified as the number of pixels moved. 
        /// </summary>
        public int X;

        /// <summary>
        /// Specifies the absolute position of the mouse, or the amount of motion since the last mouse event was generated, depending on the value of the dwFlags member. Absolute data is specified as the y coordinate of the mouse; relative data is specified as the number of pixels moved. 
        /// </summary>
        public int Y;

        /// <summary>
        /// If dwFlags contains MOUSEEVENTF_WHEEL, then mouseData specifies the amount of wheel movement. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user. One wheel click is defined as WHEEL_DELTA, which is 120. 
        /// Windows Vista: If dwFlags contains MOUSEEVENTF_HWHEEL, then dwData specifies the amount of wheel movement. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left. One wheel click is defined as WHEEL_DELTA, which is 120.
        /// Windows 2000/XP: IfdwFlags does not contain MOUSEEVENTF_WHEEL, MOUSEEVENTF_XDOWN, or MOUSEEVENTF_XUP, then mouseData should be zero. 
        /// If dwFlags contains MOUSEEVENTF_XDOWN or MOUSEEVENTF_XUP, then mouseData specifies which X buttons were pressed or released. This value may be any combination of the following flags. 
        /// </summary>
        public uint MouseData;

        /// <summary>
        /// A set of bit flags that specify various aspects of mouse motion and button clicks. The bits in this member can be any reasonable combination of the following values. 
        /// The bit flags that specify mouse button status are set to indicate changes in status, not ongoing conditions. For example, if the left mouse button is pressed and held down, MOUSEEVENTF_LEFTDOWN is set when the left button is first pressed, but not for subsequent motions. Similarly, MOUSEEVENTF_LEFTUP is set only when the button is first released. 
        /// You cannot specify both the MOUSEEVENTF_WHEEL flag and either MOUSEEVENTF_XDOWN or MOUSEEVENTF_XUP flags simultaneously in the dwFlags parameter, because they both require use of the mouseData field. 
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Time stamp for the event, in milliseconds. If this parameter is 0, the system will provide its own time stamp. 
        /// </summary>
        public uint Time;

        /// <summary>
        /// Specifies an additional value associated with the mouse event. An application calls GetMessageExtraInfo to obtain this extra information. 
        /// </summary>
        public IntPtr ExtraInfo;

        public override string ToString()
        {
            var moveFlag = (Flags & (UInt32)MouseFlag.Move) != 0;
            var leftDownFlag = (Flags & (UInt32)MouseFlag.LeftDown) != 0;
            var leftUpFlag = (Flags & (UInt32)MouseFlag.LeftUp) != 0;
            var rightDownFlag = (Flags & (UInt32)MouseFlag.RightDown) != 0;
            var rightUpFlag = (Flags & (UInt32)MouseFlag.RightUp) != 0;
            var middleDownFlag = (Flags & (UInt32)MouseFlag.MiddleDown) != 0;
            var middleUpFlag = (Flags & (UInt32)MouseFlag.MiddleUp) != 0;
            var xDownFlag = (Flags & (UInt32)MouseFlag.XDown) != 0;
            var xUpFlag = (Flags & (UInt32)MouseFlag.XUp) != 0;
            var verticalWheelFlag = (Flags & (UInt32)MouseFlag.VerticalWheel) != 0;
            var horizontalWheelFlag = (Flags & (UInt32)MouseFlag.HorizontalWheel) != 0;
            var virtualDeskFlag = (Flags & (UInt32)MouseFlag.VirtualDesk) != 0;
            var absoluteFlag = (Flags & (UInt32)MouseFlag.Absolute) != 0;

            var flagSb = new StringBuilder();

            if (moveFlag) { flagSb.Append("Move"); }

            if (leftDownFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("LeftDown");
            }

            if (leftUpFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("LeftUp");
            }

            if (rightDownFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("RightDown");
            }

            if (rightUpFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("RightUp");
            }

            if (middleDownFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("MiddleDown");
            }

            if (middleUpFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("MiddleUp");
            }

            if (xDownFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("XDown");
            }

            if (xUpFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("XUp");
            }

            if (verticalWheelFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("VerticalWheel");
            }

            if (horizontalWheelFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("HorizontalWheel");
            }

            if (virtualDeskFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("VirtualDesk");
            }

            if (absoluteFlag)
            {
                if (flagSb.Length > 0) { flagSb.Append(" | "); }
                flagSb.Append("Absolute");
            }

            return string.Format("\t\t\t\tX:{0}" +
                                 "\n\t\t\t\tY:{1}" +
                                 "\n\t\t\t\tMouseData:{2}" +
                                 "\n\t\t\t\tFlags:{3} ({4})" +
                                 "\n\t\t\t\tTime:{5}",
                                 X, Y, MouseData, Flags, flagSb, Time);
        }
    }
}