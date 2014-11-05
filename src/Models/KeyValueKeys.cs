using JuliusSweetland.ETTA.Enums;

namespace JuliusSweetland.ETTA.Models
{
    public static class KeyValueKeys
    {
        public static readonly string AltKey = new KeyValue { FunctionKey = FunctionKeys.Alt }.Key;
        public static readonly string CtrlKey = new KeyValue { FunctionKey = FunctionKeys.Ctrl }.Key;
        public static readonly string ShiftKey = new KeyValue { FunctionKey = FunctionKeys.Shift }.Key;
        public static readonly string NextSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.NextSuggestions }.Key;
        public static readonly string PreviousSuggestionsKey = new KeyValue { FunctionKey = FunctionKeys.PreviousSuggestions }.Key;
        public static readonly string Suggestion1Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion1 }.Key;
        public static readonly string Suggestion2Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion2 }.Key;
        public static readonly string Suggestion3Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion3 }.Key;
        public static readonly string Suggestion4Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion4 }.Key;
        public static readonly string Suggestion5Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion5 }.Key;
        public static readonly string Suggestion6Key = new KeyValue { FunctionKey = FunctionKeys.Suggestion6 }.Key;
        public static readonly string TogglePublishKey = new KeyValue { FunctionKey = FunctionKeys.TogglePublish }.Key;
        public static readonly string ToggleMultiKeySelectionSupportedKey = new KeyValue { FunctionKey = FunctionKeys.ToggleMultiKeySelectionSupported }.Key;
    }
}
