using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Models
{
    public static class KeyValues
    {
        public static readonly KeyValue CalibrateKey = new KeyValue(FunctionKeys.Calibrate);
        public static readonly KeyValue CollapseDockKey = new KeyValue(FunctionKeys.CollapseDock);
        public static readonly KeyValue ExpandDockKey = new KeyValue(FunctionKeys.ExpandDock);
        public static readonly KeyValue ExpandToBottomKey = new KeyValue(FunctionKeys.ExpandToBottom);
        public static readonly KeyValue ExpandToBottomAndLeftKey = new KeyValue(FunctionKeys.ExpandToBottomAndLeft);
        public static readonly KeyValue ExpandToBottomAndRightKey = new KeyValue(FunctionKeys.ExpandToBottomAndRight);
        public static readonly KeyValue ExpandToLeftKey = new KeyValue(FunctionKeys.ExpandToLeft);
        public static readonly KeyValue ExpandToRightKey = new KeyValue(FunctionKeys.ExpandToRight);
        public static readonly KeyValue ExpandToTopKey = new KeyValue(FunctionKeys.ExpandToTop);
        public static readonly KeyValue ExpandToTopAndLeftKey = new KeyValue(FunctionKeys.ExpandToTopAndLeft);
        public static readonly KeyValue ExpandToTopAndRightKey = new KeyValue(FunctionKeys.ExpandToTopAndRight);
        public static readonly KeyValue LeftAltKey = new KeyValue(FunctionKeys.LeftAlt);
        public static readonly KeyValue LeftCtrlKey = new KeyValue(FunctionKeys.LeftCtrl);
        public static readonly KeyValue LeftShiftKey = new KeyValue(FunctionKeys.LeftShift);
        public static readonly KeyValue LeftWinKey = new KeyValue(FunctionKeys.LeftWin);
        public static readonly KeyValue MouseDragKey = new KeyValue(FunctionKeys.MouseDrag);
        public static readonly KeyValue MouseLeftClickKey = new KeyValue(FunctionKeys.MouseLeftClick);
        public static readonly KeyValue MouseLeftDoubleClickKey = new KeyValue(FunctionKeys.MouseLeftDoubleClick);
        public static readonly KeyValue MouseLeftDownUpKey = new KeyValue(FunctionKeys.MouseLeftDownUp);
        public static readonly KeyValue MouseMagneticCursorKey = new KeyValue(FunctionKeys.MouseMagneticCursor);
        public static readonly KeyValue MouseMagnifierKey = new KeyValue(FunctionKeys.MouseMagnifier);
        public static readonly KeyValue MouseMiddleClickKey = new KeyValue(FunctionKeys.MouseMiddleClick);
        public static readonly KeyValue MouseMiddleDownUpKey = new KeyValue(FunctionKeys.MouseMiddleDownUp);
        public static readonly KeyValue MouseMoveAndLeftClickKey = new KeyValue(FunctionKeys.MouseMoveAndLeftClick);
        public static readonly KeyValue MouseMoveAndLeftDoubleClickKey = new KeyValue(FunctionKeys.MouseMoveAndLeftDoubleClick);
        public static readonly KeyValue MouseMoveAndMiddleClickKey = new KeyValue(FunctionKeys.MouseMoveAndMiddleClick);
        public static readonly KeyValue MouseMoveAndRightClickKey = new KeyValue(FunctionKeys.MouseMoveAndRightClick);
        public static readonly KeyValue MouseRightClickKey = new KeyValue(FunctionKeys.MouseRightClick);
        public static readonly KeyValue MouseRightDownUpKey = new KeyValue(FunctionKeys.MouseRightDownUp);
        public static readonly KeyValue MoveToBottomKey = new KeyValue(FunctionKeys.MoveToBottom);
        public static readonly KeyValue MoveToBottomAndLeftKey = new KeyValue(FunctionKeys.MoveToBottomAndLeft);
        public static readonly KeyValue MoveToBottomAndLeftBoundariesKey = new KeyValue(FunctionKeys.MoveToBottomAndLeftBoundaries);
        public static readonly KeyValue MoveToBottomAndRightKey = new KeyValue(FunctionKeys.MoveToBottomAndRight);
        public static readonly KeyValue MoveToBottomAndRightBoundariesKey = new KeyValue(FunctionKeys.MoveToBottomAndRightBoundaries);
        public static readonly KeyValue MoveToBottomBoundaryKey = new KeyValue(FunctionKeys.MoveToBottomBoundary);
        public static readonly KeyValue MoveToLeftKey = new KeyValue(FunctionKeys.MoveToLeft);
        public static readonly KeyValue MoveToLeftBoundaryKey = new KeyValue(FunctionKeys.MoveToLeftBoundary);
        public static readonly KeyValue MoveToRightKey = new KeyValue(FunctionKeys.MoveToRight);
        public static readonly KeyValue MoveToRightBoundaryKey = new KeyValue(FunctionKeys.MoveToRightBoundary);
        public static readonly KeyValue MoveToTopKey = new KeyValue(FunctionKeys.MoveToTop);
        public static readonly KeyValue MoveToTopAndLeftKey = new KeyValue(FunctionKeys.MoveToTopAndLeft);
        public static readonly KeyValue MoveToTopAndLeftBoundariesKey = new KeyValue(FunctionKeys.MoveToTopAndLeftBoundaries);
        public static readonly KeyValue MoveToTopAndRightKey = new KeyValue(FunctionKeys.MoveToTopAndRight);
        public static readonly KeyValue MoveToTopAndRightBoundariesKey = new KeyValue(FunctionKeys.MoveToTopAndRightBoundaries);
        public static readonly KeyValue MoveToTopBoundaryKey = new KeyValue(FunctionKeys.MoveToTopBoundary);
        public static readonly KeyValue MultiKeySelectionKey = new KeyValue(FunctionKeys.MultiKeySelectionIsOn);
        public static readonly KeyValue NextSuggestionsKey = new KeyValue(FunctionKeys.NextSuggestions);
        public static readonly KeyValue PreviousSuggestionsKey = new KeyValue(FunctionKeys.PreviousSuggestions);
        public static readonly KeyValue RepeatLastMouseActionKey = new KeyValue(FunctionKeys.RepeatLastMouseAction);
        public static readonly KeyValue ShrinkFromBottomKey = new KeyValue(FunctionKeys.ShrinkFromBottom);
        public static readonly KeyValue ShrinkFromBottomAndLeftKey = new KeyValue(FunctionKeys.ShrinkFromBottomAndLeft);
        public static readonly KeyValue ShrinkFromBottomAndRightKey = new KeyValue(FunctionKeys.ShrinkFromBottomAndRight);
        public static readonly KeyValue ShrinkFromLeftKey = new KeyValue(FunctionKeys.ShrinkFromLeft);
        public static readonly KeyValue ShrinkFromRightKey = new KeyValue(FunctionKeys.ShrinkFromRight);
        public static readonly KeyValue ShrinkFromTopKey = new KeyValue(FunctionKeys.ShrinkFromTop);
        public static readonly KeyValue ShrinkFromTopAndLeftKey = new KeyValue(FunctionKeys.ShrinkFromTopAndLeft);
        public static readonly KeyValue ShrinkFromTopAndRightKey = new KeyValue(FunctionKeys.ShrinkFromTopAndRight);
        public static readonly KeyValue SpeakKey = new KeyValue(FunctionKeys.Speak);
        public static readonly KeyValue Suggestion1Key = new KeyValue(FunctionKeys.Suggestion1);
        public static readonly KeyValue Suggestion2Key = new KeyValue(FunctionKeys.Suggestion2);
        public static readonly KeyValue Suggestion3Key = new KeyValue(FunctionKeys.Suggestion3);
        public static readonly KeyValue Suggestion4Key = new KeyValue(FunctionKeys.Suggestion4);
        public static readonly KeyValue Suggestion5Key = new KeyValue(FunctionKeys.Suggestion5);
        public static readonly KeyValue Suggestion6Key = new KeyValue(FunctionKeys.Suggestion6);
        public static readonly KeyValue SleepKey = new KeyValue(FunctionKeys.Sleep);

        private static readonly Dictionary<Languages, List<KeyValue>> multiKeySelectionKeys;

        static KeyValues()
        {
            var defaultList = "abcdefghijklmnopqrstuvwxyz"
                .ToCharArray()
                .Select(c => new KeyValue(c.ToString(CultureInfo.InvariantCulture)))
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
                                                .Select(c => new KeyValue (c.ToString(CultureInfo.InvariantCulture) ))
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
                    new KeyValue(FunctionKeys.LeftCtrl),
                    new KeyValue(FunctionKeys.LeftWin),
                    new KeyValue(FunctionKeys.LeftAlt),
                    new KeyValue(FunctionKeys.F1),
                    new KeyValue(FunctionKeys.F2),
                    new KeyValue(FunctionKeys.F3),
                    new KeyValue(FunctionKeys.F4),
                    new KeyValue(FunctionKeys.F5),
                    new KeyValue(FunctionKeys.F6),
                    new KeyValue(FunctionKeys.F7),
                    new KeyValue(FunctionKeys.F8),
                    new KeyValue(FunctionKeys.F9),
                    new KeyValue(FunctionKeys.F10),
                    new KeyValue(FunctionKeys.F11),
                    new KeyValue(FunctionKeys.F12),
                    new KeyValue(FunctionKeys.PrintScreen),
                    new KeyValue(FunctionKeys.ScrollLock),
                    new KeyValue(FunctionKeys.NumberLock),
                    new KeyValue(FunctionKeys.Menu),
                    new KeyValue(FunctionKeys.ArrowUp),
                    new KeyValue(FunctionKeys.ArrowLeft),
                    new KeyValue(FunctionKeys.ArrowRight),
                    new KeyValue(FunctionKeys.ArrowDown),
                    new KeyValue(FunctionKeys.Break),
                    new KeyValue(FunctionKeys.Insert),
                    new KeyValue(FunctionKeys.Home),
                    new KeyValue(FunctionKeys.PgUp),
                    new KeyValue(FunctionKeys.PgDn),
                    new KeyValue(FunctionKeys.Delete),
                    new KeyValue(FunctionKeys.End),
                    new KeyValue(FunctionKeys.Escape),
                    new KeyValue(FunctionKeys.SelectAll),
                    new KeyValue(FunctionKeys.Cut),
                    new KeyValue(FunctionKeys.Copy),
                    new KeyValue(FunctionKeys.Paste)
                };
            }
        }

        public static List<KeyValue> MultiKeySelectionKeys
        {
            get { return multiKeySelectionKeys[Settings.Default.KeyboardAndDictionaryLanguage]; }
        }
    }
}
