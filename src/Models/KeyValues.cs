using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    public static class KeyValues
    {
        public static readonly KeyValue CalibrateKey = new KeyValue { FunctionKey = FunctionKeys.Calibrate };
        public static readonly KeyValue LeftAltKey = new KeyValue { FunctionKey = FunctionKeys.LeftAlt };
        public static readonly KeyValue LeftCtrlKey = new KeyValue { FunctionKey = FunctionKeys.LeftCtrl };
        public static readonly KeyValue LeftShiftKey = new KeyValue { FunctionKey = FunctionKeys.LeftShift };
        public static readonly KeyValue LeftWinKey = new KeyValue { FunctionKey = FunctionKeys.LeftWin };
        public static readonly KeyValue MouseMagnifierKey = new KeyValue { FunctionKey = FunctionKeys.MouseMagnifier };
        public static readonly KeyValue MultiKeySelectionEnabledKey = new KeyValue { FunctionKey = FunctionKeys.MultiKeySelectionEnabled };
        public static readonly KeyValue NextSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.NextSuggestions };
        public static readonly KeyValue PreviousSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.PreviousSuggestions };
        public static readonly KeyValue SimulateKeyStrokesKey = new KeyValue { FunctionKey = FunctionKeys.SimulateKeyStrokes };
        public static readonly KeyValue Suggestion1Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion1 };
        public static readonly KeyValue Suggestion2Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion2 };
        public static readonly KeyValue Suggestion3Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion3 };
        public static readonly KeyValue Suggestion4Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion4 };
        public static readonly KeyValue Suggestion5Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion5 };
        public static readonly KeyValue Suggestion6Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion6 };
        public static readonly KeyValue SleepKey = new KeyValue { FunctionKey = FunctionKeys.Sleep };

        public static List<KeyValue> KeysWhichCanBePressedDown
        {
            get
            {
                return new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftShiftKey,
                    LeftWinKey
                };
            }
        }

        public static List<KeyValue> KeysWhichCanBeLockedDown
        {
            get
            {
                return new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftShiftKey,
                    LeftWinKey,
                    MouseMagnifierKey,
                    MultiKeySelectionEnabledKey,
                    SimulateKeyStrokesKey,
                    SleepKey
                };
            }
        }

        public static List<KeyValue> KeysWhichCanBePressedOrLockedDown
        {
            get
            {
                return KeysWhichCanBePressedDown.Concat(KeysWhichCanBeLockedDown).Distinct().ToList();
            }
        }

        public static List<KeyValue> KeysWhichPreventTextCaptureIfDownOrLocked
        {
            get
            {
                return new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftWinKey
                };
            }
        }

        public static List<KeyValue> PublishOnlyKeys
        {
            get
            {
                return new List<KeyValue>
                {
                    new KeyValue {FunctionKey = FunctionKeys.LeftCtrl},
                    new KeyValue {FunctionKey = FunctionKeys.LeftWin},
                    new KeyValue {FunctionKey = FunctionKeys.LeftAlt},
                    new KeyValue {FunctionKey = FunctionKeys.F1},
                    new KeyValue {FunctionKey = FunctionKeys.F2},
                    new KeyValue {FunctionKey = FunctionKeys.F3},
                    new KeyValue {FunctionKey = FunctionKeys.F4},
                    new KeyValue {FunctionKey = FunctionKeys.F5},
                    new KeyValue {FunctionKey = FunctionKeys.F6},
                    new KeyValue {FunctionKey = FunctionKeys.F7},
                    new KeyValue {FunctionKey = FunctionKeys.F8},
                    new KeyValue {FunctionKey = FunctionKeys.F9},
                    new KeyValue {FunctionKey = FunctionKeys.F10},
                    new KeyValue {FunctionKey = FunctionKeys.F11},
                    new KeyValue {FunctionKey = FunctionKeys.F12},
                    new KeyValue {FunctionKey = FunctionKeys.PrintScreen},
                    new KeyValue {FunctionKey = FunctionKeys.ScrollLock},
                    new KeyValue {FunctionKey = FunctionKeys.NumberLock},
                    new KeyValue {FunctionKey = FunctionKeys.Menu},
                    new KeyValue {FunctionKey = FunctionKeys.ArrowUp},
                    new KeyValue {FunctionKey = FunctionKeys.ArrowLeft},
                    new KeyValue {FunctionKey = FunctionKeys.ArrowRight},
                    new KeyValue {FunctionKey = FunctionKeys.ArrowDown},
                    new KeyValue {FunctionKey = FunctionKeys.Break},
                    new KeyValue {FunctionKey = FunctionKeys.Insert},
                    new KeyValue {FunctionKey = FunctionKeys.Home},
                    new KeyValue {FunctionKey = FunctionKeys.PgUp},
                    new KeyValue {FunctionKey = FunctionKeys.PgDn},
                    new KeyValue {FunctionKey = FunctionKeys.Delete},
                    new KeyValue {FunctionKey = FunctionKeys.End},
                    new KeyValue {FunctionKey = FunctionKeys.Escape},
                    new KeyValue {FunctionKey = FunctionKeys.SelectAll},
                    new KeyValue {FunctionKey = FunctionKeys.Cut},
                    new KeyValue {FunctionKey = FunctionKeys.Copy},
                    new KeyValue {FunctionKey = FunctionKeys.Paste}
                };
            }
        }

        public static List<KeyValue> LetterKeys
        {
            get
            {
                return "abcdefghijklmnopqrstuvwxyz"
                    .ToCharArray()
                    .Select(c => new KeyValue { String = c.ToString(CultureInfo.InvariantCulture) })
                    .ToList();
            }
        }
    }
}
