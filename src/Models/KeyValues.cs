using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Models
{
    public static class KeyValues
    {
        public static readonly KeyValue AltKey = new KeyValue { FunctionKey = FunctionKeys.Alt };
        public static readonly KeyValue CtrlKey = new KeyValue { FunctionKey = FunctionKeys.Ctrl };
        public static readonly KeyValue ShiftKey = new KeyValue { FunctionKey = FunctionKeys.Shift };
        public static readonly KeyValue WinKey = new KeyValue { FunctionKey = FunctionKeys.Win };
        public static readonly KeyValue NextSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.NextSuggestions };
        public static readonly KeyValue PreviousSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.PreviousSuggestions };
        public static readonly KeyValue SleepKey = new KeyValue { FunctionKey = FunctionKeys.Sleep };
        public static readonly KeyValue Suggestion1Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion1 };
        public static readonly KeyValue Suggestion2Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion2 };
        public static readonly KeyValue Suggestion3Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion3 };
        public static readonly KeyValue Suggestion4Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion4 };
        public static readonly KeyValue Suggestion5Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion5 };
        public static readonly KeyValue Suggestion6Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion6 };
        public static readonly KeyValue TogglePublishKey = new KeyValue { FunctionKey = FunctionKeys.TogglePublish };
        public static readonly KeyValue ToggleMultiKeySelectionSupportedKey = new KeyValue { FunctionKey = FunctionKeys.ToggleMultiKeySelectionSupported };
    }
}
