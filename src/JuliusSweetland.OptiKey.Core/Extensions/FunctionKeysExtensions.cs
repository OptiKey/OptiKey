// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;
using log4net;
using System.Reflection;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class FunctionKeysExtensions
    {

        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //http://inputsimulator.codeplex.com/SourceControl/latest#WindowsInput/Native/VirtualKeyCode.cs
        //http://msdn.microsoft.com/en-gb/library/windows/desktop/dd375731(v=vs.85).aspx
        public static VirtualKeyCode? ToVirtualKeyCode(this FunctionKeys functionKey)
        {
            switch (functionKey)
            {
                case FunctionKeys.BackOne:
                    return VirtualKeyCode.BACK;

                case FunctionKeys.BrowserBack:
                    return VirtualKeyCode.BROWSER_BACK;

                case FunctionKeys.BrowserForward:
                    return VirtualKeyCode.BROWSER_FORWARD;

                case FunctionKeys.BrowserHome:
                    return VirtualKeyCode.BROWSER_HOME;

                case FunctionKeys.BrowserSearch:
                    return VirtualKeyCode.BROWSER_SEARCH;

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

                case FunctionKeys.F13:
                    return VirtualKeyCode.F13;

                case FunctionKeys.F14:
                    return VirtualKeyCode.F14;

                case FunctionKeys.F15:
                    return VirtualKeyCode.F15;

                case FunctionKeys.F16:
                    return VirtualKeyCode.F16;
                
                case FunctionKeys.F17:
                    return VirtualKeyCode.F17;

                case FunctionKeys.F18:
                    return VirtualKeyCode.F18;

                case FunctionKeys.F19:
                    return VirtualKeyCode.F19;

                case FunctionKeys.F20:
                    return VirtualKeyCode.F20;

                case FunctionKeys.F21:
                    return VirtualKeyCode.F21;

                case FunctionKeys.F22:
                    return VirtualKeyCode.F22;

                case FunctionKeys.F23:
                    return VirtualKeyCode.F23;

                case FunctionKeys.F24:
                    return VirtualKeyCode.F24;

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

                case FunctionKeys.NumPad0:
                    return VirtualKeyCode.NUMPAD0;

                case FunctionKeys.NumPad1:
                    return VirtualKeyCode.NUMPAD1;

                case FunctionKeys.NumPad2:
                    return VirtualKeyCode.NUMPAD2;

                case FunctionKeys.NumPad3:
                    return VirtualKeyCode.NUMPAD3;

                case FunctionKeys.NumPad4:
                    return VirtualKeyCode.NUMPAD4;

                case FunctionKeys.NumPad5:
                    return VirtualKeyCode.NUMPAD5;

                case FunctionKeys.NumPad6:
                    return VirtualKeyCode.NUMPAD6;

                case FunctionKeys.NumPad7:
                    return VirtualKeyCode.NUMPAD7;

                case FunctionKeys.NumPad8:
                    return VirtualKeyCode.NUMPAD8;

                case FunctionKeys.NumPad9:
                    return VirtualKeyCode.NUMPAD9;                

                case FunctionKeys.PgUp:
                    return VirtualKeyCode.PRIOR;

                case FunctionKeys.PgDn:
                    return VirtualKeyCode.NEXT;

                case FunctionKeys.Delete:
                    return VirtualKeyCode.DELETE;

                case FunctionKeys.End:
                    return VirtualKeyCode.END;

                case FunctionKeys.Enter:
                    return VirtualKeyCode.RETURN;

                case FunctionKeys.Escape:
                    return VirtualKeyCode.ESCAPE;

                case FunctionKeys.Tab:
                    return VirtualKeyCode.TAB;

                default:
                    return null;
            }
        }

        public static FunctionKeys? FromString(string keyString)
        {
            FunctionKeys fKey;
            if (System.Enum.TryParse(keyString, out fKey))
            {
                return fKey;
            }
            else
            {
                Log.ErrorFormat("Could not parse {0} as function key", keyString);
                return null;
            }
        }
    }
}
