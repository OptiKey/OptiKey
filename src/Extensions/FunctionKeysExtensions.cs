using WindowsInput.Native;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class FunctionKeysExtensions
    {
        //http://inputsimulator.codeplex.com/SourceControl/latest#WindowsInput/Native/VirtualKeyCode.cs
        public static VirtualKeyCodeSet? ToVirtualKeyCodeSet(this FunctionKeys functionKey)
        {
            switch (functionKey)
            {
                case FunctionKeys.BackOne:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.BACK}};

                case FunctionKeys.Shift:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.SHIFT}};

                case FunctionKeys.Alt:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.MENU}}; //This is not a typo

                case FunctionKeys.Ctrl:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.CONTROL}};

                case FunctionKeys.Win:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.LWIN}};

                case FunctionKeys.F1:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F1}};

                case FunctionKeys.F2:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F2}};

                case FunctionKeys.F3:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F3}};

                case FunctionKeys.F4:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F4}};

                case FunctionKeys.F5:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F5}};

                case FunctionKeys.F6:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F6}};

                case FunctionKeys.F7:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F7}};

                case FunctionKeys.F8:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F8}};

                case FunctionKeys.F9:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F9}};

                case FunctionKeys.F10:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F10}};

                case FunctionKeys.F11:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F11}};

                case FunctionKeys.F12:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.F12}};

                case FunctionKeys.PrintScreen:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.SNAPSHOT}}; //This is not a typo

                case FunctionKeys.ScrollLock:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.SCROLL}};

                case FunctionKeys.NumberLock:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.NUMLOCK}};

                case FunctionKeys.Menu:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.APPS}}; //This is not a typo

                case FunctionKeys.ArrowUp:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.UP}};

                case FunctionKeys.ArrowDown:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.DOWN}};

                case FunctionKeys.ArrowLeft:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.LEFT}};

                case FunctionKeys.ArrowRight:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.RIGHT}};

                case FunctionKeys.Break:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.CANCEL}};

                case FunctionKeys.Insert:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.INSERT}};

                case FunctionKeys.Home:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.HOME}};

                case FunctionKeys.PgUp:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.PRIOR}};

                case FunctionKeys.PgDn:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.NEXT}};

                case FunctionKeys.Delete:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.DELETE}};

                case FunctionKeys.End:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.END}};

                case FunctionKeys.Escape:
                    return new VirtualKeyCodeSet {KeyCodes = new[] {VirtualKeyCode.ESCAPE}};

                case FunctionKeys.SelectAll:
                    return new VirtualKeyCodeSet
                    {
                        ModifierKeyCodes = new[] {VirtualKeyCode.CONTROL},
                        KeyCodes = new[] {VirtualKeyCode.VK_A}
                    };

                case FunctionKeys.Cut:
                    return new VirtualKeyCodeSet
                    {
                        ModifierKeyCodes = new[] {VirtualKeyCode.CONTROL},
                        KeyCodes = new[] {VirtualKeyCode.VK_X}
                    };

                case FunctionKeys.Copy:
                    return new VirtualKeyCodeSet
                    {
                        ModifierKeyCodes = new[] {VirtualKeyCode.CONTROL},
                        KeyCodes = new[] {VirtualKeyCode.VK_C}
                    };

                case FunctionKeys.Paste:
                    return new VirtualKeyCodeSet
                    {
                        ModifierKeyCodes = new[] {VirtualKeyCode.CONTROL},
                        KeyCodes = new[] {VirtualKeyCode.VK_V}
                    };

                default:
                    return null;
            }
        }
    }
}
