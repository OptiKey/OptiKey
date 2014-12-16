using WindowsInput.Native;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class FunctionKeysExtensions
    {
        //http://inputsimulator.codeplex.com/SourceControl/latest#WindowsInput/Native/VirtualKeyCode.cs
        //http://msdn.microsoft.com/en-gb/library/windows/desktop/dd375731(v=vs.85).aspx
        public static VirtualKeyCode? ToVirtualKeyCode(this FunctionKeys functionKey)
        {
            switch (functionKey)
            {
                case FunctionKeys.BackOne:
                    return VirtualKeyCode.BACK;

                case FunctionKeys.LeftShift:
                    return VirtualKeyCode.LSHIFT;

                case FunctionKeys.LeftAlt:
                    return VirtualKeyCode.LMENU; //This is not a typo ALT=MENU

                case FunctionKeys.LeftCtrl:
                    return VirtualKeyCode.LCONTROL;

                case FunctionKeys.LeftWin:
                    return VirtualKeyCode.LWIN;

                case FunctionKeys.F1:
                    return VirtualKeyCode.F1;

                case FunctionKeys.F2:
                    return VirtualKeyCode.F2;

                case FunctionKeys.F3:
                    return VirtualKeyCode.F3;

                case FunctionKeys.F4:
                    return VirtualKeyCode.F4;

                case FunctionKeys.F5:
                    return VirtualKeyCode.F5;

                case FunctionKeys.F6:
                    return VirtualKeyCode.F6;

                case FunctionKeys.F7:
                    return VirtualKeyCode.F7;

                case FunctionKeys.F8:
                    return VirtualKeyCode.F8;

                case FunctionKeys.F9:
                    return VirtualKeyCode.F9;

                case FunctionKeys.F10:
                    return VirtualKeyCode.F10;

                case FunctionKeys.F11:
                    return VirtualKeyCode.F11;

                case FunctionKeys.F12:
                    return VirtualKeyCode.F12;

                case FunctionKeys.PrintScreen:
                    return VirtualKeyCode.SNAPSHOT; //This is not a typo

                case FunctionKeys.ScrollLock:
                    return VirtualKeyCode.SCROLL;

                case FunctionKeys.NumberLock:
                    return VirtualKeyCode.NUMLOCK;

                case FunctionKeys.Menu:
                    return VirtualKeyCode.APPS; //This is not a typo

                case FunctionKeys.ArrowUp:
                    return VirtualKeyCode.UP;

                case FunctionKeys.ArrowDown:
                    return VirtualKeyCode.DOWN;

                case FunctionKeys.ArrowLeft:
                    return VirtualKeyCode.LEFT;

                case FunctionKeys.ArrowRight:
                    return VirtualKeyCode.RIGHT;

                case FunctionKeys.Break:
                    return VirtualKeyCode.CANCEL;

                case FunctionKeys.Insert:
                    return VirtualKeyCode.INSERT;

                case FunctionKeys.Home:
                    return VirtualKeyCode.HOME;

                case FunctionKeys.PgUp:
                    return VirtualKeyCode.PRIOR;

                case FunctionKeys.PgDn:
                    return VirtualKeyCode.NEXT;

                case FunctionKeys.Delete:
                    return VirtualKeyCode.DELETE;

                case FunctionKeys.End:
                    return VirtualKeyCode.END;

                case FunctionKeys.Escape:
                    return VirtualKeyCode.ESCAPE;

                default:
                    return null;
            }
        }
    }
}
