using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Models
{
    public static class KeyValueCollections
    {
        public static List<KeyValue> PublishOnlyKeys
        {
            get
            {
                return new List<KeyValue>
                {
                    new KeyValue {FunctionKey = FunctionKeys.Tab},
                    new KeyValue {FunctionKey = FunctionKeys.Ctrl},
                    new KeyValue {FunctionKey = FunctionKeys.Win},
                    new KeyValue {FunctionKey = FunctionKeys.Alt},
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
                    .Select(c => new KeyValue {String = c.ToString()})
                    .ToList();
            }
        }
    }
}
