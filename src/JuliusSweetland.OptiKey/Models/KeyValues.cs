using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Models
{
    public static class KeyValues
    {
        public static readonly KeyValue CalibrateKey = new KeyValue { FunctionKey = FunctionKeys.Calibrate };
        public static readonly KeyValue CollapseDockKey = new KeyValue { FunctionKey = FunctionKeys.CollapseDock };
        public static readonly KeyValue ExpandDockKey = new KeyValue { FunctionKey = FunctionKeys.ExpandDock };
        public static readonly KeyValue ExpandToBottomKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToBottom };
        public static readonly KeyValue ExpandToBottomAndLeftKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToBottomAndLeft };
        public static readonly KeyValue ExpandToBottomAndRightKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToBottomAndRight };
        public static readonly KeyValue ExpandToLeftKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToLeft };
        public static readonly KeyValue ExpandToRightKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToRight };
        public static readonly KeyValue ExpandToTopKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToTop };
        public static readonly KeyValue ExpandToTopAndLeftKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToTopAndLeft };
        public static readonly KeyValue ExpandToTopAndRightKey = new KeyValue { FunctionKey = FunctionKeys.ExpandToTopAndRight };
        public static readonly KeyValue LeftAltKey = new KeyValue { FunctionKey = FunctionKeys.LeftAlt };
        public static readonly KeyValue LeftCtrlKey = new KeyValue { FunctionKey = FunctionKeys.LeftCtrl };
        public static readonly KeyValue LeftShiftKey = new KeyValue { FunctionKey = FunctionKeys.LeftShift };
        public static readonly KeyValue LeftWinKey = new KeyValue { FunctionKey = FunctionKeys.LeftWin };
        public static readonly KeyValue MouseDragKey = new KeyValue { FunctionKey = FunctionKeys.MouseDrag };
        public static readonly KeyValue MouseLeftClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseLeftClick };
        public static readonly KeyValue MouseLeftDoubleClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseLeftDoubleClick };
        public static readonly KeyValue MouseLeftDownUpKey = new KeyValue { FunctionKey = FunctionKeys.MouseLeftDownUp };
        public static readonly KeyValue MouseMagneticCursorKey = new KeyValue { FunctionKey = FunctionKeys.MouseMagneticCursor };
        public static readonly KeyValue MouseMagnifierKey = new KeyValue { FunctionKey = FunctionKeys.MouseMagnifier };
        public static readonly KeyValue MouseMiddleClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseMiddleClick };
        public static readonly KeyValue MouseMiddleDownUpKey = new KeyValue { FunctionKey = FunctionKeys.MouseMiddleDownUp };
        public static readonly KeyValue MouseMoveAndLeftClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseMoveAndLeftClick };
        public static readonly KeyValue MouseMoveAndLeftDoubleClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseMoveAndLeftDoubleClick };
        public static readonly KeyValue MouseMoveAndMiddleClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseMoveAndMiddleClick };
        public static readonly KeyValue MouseMoveAndRightClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseMoveAndRightClick };
        public static readonly KeyValue MouseRightClickKey = new KeyValue { FunctionKey = FunctionKeys.MouseRightClick };
        public static readonly KeyValue MouseRightDownUpKey = new KeyValue { FunctionKey = FunctionKeys.MouseRightDownUp };
        public static readonly KeyValue MoveToBottomKey = new KeyValue { FunctionKey = FunctionKeys.MoveToBottom };
        public static readonly KeyValue MoveToBottomAndLeftKey = new KeyValue { FunctionKey = FunctionKeys.MoveToBottomAndLeft };
        public static readonly KeyValue MoveToBottomAndLeftBoundariesKey = new KeyValue { FunctionKey = FunctionKeys.MoveToBottomAndLeftBoundaries };
        public static readonly KeyValue MoveToBottomAndRightKey = new KeyValue { FunctionKey = FunctionKeys.MoveToBottomAndRight };
        public static readonly KeyValue MoveToBottomAndRightBoundariesKey = new KeyValue { FunctionKey = FunctionKeys.MoveToBottomAndRightBoundaries };
        public static readonly KeyValue MoveToBottomBoundaryKey = new KeyValue { FunctionKey = FunctionKeys.MoveToBottomBoundary };
        public static readonly KeyValue MoveToLeftKey = new KeyValue { FunctionKey = FunctionKeys.MoveToLeft };
        public static readonly KeyValue MoveToLeftBoundaryKey = new KeyValue { FunctionKey = FunctionKeys.MoveToLeftBoundary };
        public static readonly KeyValue MoveToRightKey = new KeyValue { FunctionKey = FunctionKeys.MoveToRight };
        public static readonly KeyValue MoveToRightBoundaryKey = new KeyValue { FunctionKey = FunctionKeys.MoveToRightBoundary };
        public static readonly KeyValue MoveToTopKey = new KeyValue { FunctionKey = FunctionKeys.MoveToTop };
        public static readonly KeyValue MoveToTopAndLeftKey = new KeyValue { FunctionKey = FunctionKeys.MoveToTopAndLeft };
        public static readonly KeyValue MoveToTopAndLeftBoundariesKey = new KeyValue { FunctionKey = FunctionKeys.MoveToTopAndLeftBoundaries };
        public static readonly KeyValue MoveToTopAndRightKey = new KeyValue { FunctionKey = FunctionKeys.MoveToTopAndRight };
        public static readonly KeyValue MoveToTopAndRightBoundariesKey = new KeyValue { FunctionKey = FunctionKeys.MoveToTopAndRightBoundaries };
        public static readonly KeyValue MoveToTopBoundaryKey = new KeyValue { FunctionKey = FunctionKeys.MoveToTopBoundary };
        public static readonly KeyValue MultiKeySelectionKey = new KeyValue { FunctionKey = FunctionKeys.MultiKeySelectionIsOn };
        public static readonly KeyValue NextSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.NextSuggestions };
        public static readonly KeyValue PreviousSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.PreviousSuggestions };
        public static readonly KeyValue RepeatLastMouseActionKey = new KeyValue { FunctionKey = FunctionKeys.RepeatLastMouseAction };
        public static readonly KeyValue ShrinkFromBottomKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromBottom };
        public static readonly KeyValue ShrinkFromBottomAndLeftKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromBottomAndLeft };
        public static readonly KeyValue ShrinkFromBottomAndRightKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromBottomAndRight };
        public static readonly KeyValue ShrinkFromLeftKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromLeft };
        public static readonly KeyValue ShrinkFromRightKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromRight };
        public static readonly KeyValue ShrinkFromTopKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromTop };
        public static readonly KeyValue ShrinkFromTopAndLeftKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromTopAndLeft };
        public static readonly KeyValue ShrinkFromTopAndRightKey = new KeyValue { FunctionKey = FunctionKeys.ShrinkFromTopAndRight };
        public static readonly KeyValue SpeakKey = new KeyValue { FunctionKey = FunctionKeys.Speak };
        public static readonly KeyValue Suggestion1Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion1 };
        public static readonly KeyValue Suggestion2Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion2 };
        public static readonly KeyValue Suggestion3Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion3 };
        public static readonly KeyValue Suggestion4Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion4 };
        public static readonly KeyValue Suggestion5Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion5 };
        public static readonly KeyValue Suggestion6Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion6 };
        public static readonly KeyValue SleepKey = new KeyValue { FunctionKey = FunctionKeys.Sleep };

        private static readonly Dictionary<Languages, List<KeyValue>> multiKeySelectionKeys;

        static KeyValues()
        {
            var defaultList = "abcdefghijklmnopqrstuvwxyz"
                .ToCharArray()
                .Select(c => new KeyValue {String = c.ToString(CultureInfo.InvariantCulture)})
                .ToList();

            multiKeySelectionKeys = new Dictionary<Languages, List<KeyValue>>
            {
                { Languages.EnglishCanada, defaultList },
                { Languages.EnglishUK, defaultList },
                { Languages.EnglishUS, defaultList },
                { Languages.FrenchFrance, defaultList }, //Could be customised to include àçéèù
                { Languages.GermanGermany, defaultList }, //Could be customised to include äöüß
				{ Languages.RussianRussia, "абвгдеёжзийклмнопрстуфхцчшщъыьэюя"
				                                .ToCharArray()
				                                .Select(c => new KeyValue { String = c.ToString(CultureInfo.InvariantCulture) })
				                                .ToList() }
            };
        }

        public static List<KeyValue> KeysWhichCanBePressedDown
        {
            get
            {
                return new List<KeyValue>
                {
                    LeftAltKey,
                    LeftCtrlKey,
                    LeftShiftKey,
                    LeftWinKey,
                    MouseLeftDownUpKey,
                    MouseMagnifierKey,
                    MouseMiddleDownUpKey,
                    MouseRightDownUpKey,
                    MultiKeySelectionKey
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
                    MouseMagneticCursorKey,
                    MouseMagnifierKey,
                    MultiKeySelectionKey,
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

        public static List<KeyValue> MultiKeySelectionKeys
        {
            get { return multiKeySelectionKeys[Settings.Default.KeyboardLanguage]; }
        }
    }
}
