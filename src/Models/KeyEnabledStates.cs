using System;
using System.Linq;
using System.Windows.Data;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Properties;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Models
{
    public class KeyEnabledStates : BindableBase
    {
        #region Fields

        private readonly IKeyboardStateManager keyboardStateInfo;
        private readonly string altKey = new KeyValue { FunctionKey = FunctionKeys.Alt }.Key;
        private readonly string ctrlKey = new KeyValue { FunctionKey = FunctionKeys.Ctrl }.Key;

        #endregion

        #region Ctor

        public KeyEnabledStates(IKeyboardStateManager keyboardStateInfo)
        {
            this.keyboardStateInfo = keyboardStateInfo;

            keyboardStateInfo.OnPropertyChanges(ksi => ksi.CapturingMultiKeySelection).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.OnPropertyChanges(ksi => ksi.Suggestions).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.OnPropertyChanges(ksi => ksi.SuggestionsPage).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.OnPropertyChanges(ksi => ksi.SuggestionsPerPage).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.KeyDownStates[altKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.KeyDownStates[ctrlKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            
            Settings.Default.OnPropertyChanges(s => s.PublishingKeys).Subscribe(_ => NotifyStateChanged());
        }

        #endregion

        #region Indexer

        public bool this[string key]
        {
            get
            {
                //Key is publish only, but we are not publishing
                if (!Settings.Default.PublishingKeys
                    && KeyValueCollections.PublishOnlyKeys.Select(kv => kv.Key).Contains(key))
                {
                    return false;
                }

                //Key is BackOne/BackMany and the Alt or Ctrl modifier key is down
                if ((key == new KeyValue {FunctionKey = FunctionKeys.BackOne}.Key 
                    || key == new KeyValue {FunctionKey = FunctionKeys.BackMany}.Key)
                    && (keyboardStateInfo.KeyDownStates[altKey].Value == KeyDownStates.On
                        || keyboardStateInfo.KeyDownStates[altKey].Value == KeyDownStates.Lock
                        || keyboardStateInfo.KeyDownStates[ctrlKey].Value == KeyDownStates.On
                        || keyboardStateInfo.KeyDownStates[ctrlKey].Value == KeyDownStates.Lock))
                {
                    return false;
                }

                //Previous suggestions if no suggestions, or on page 1
                if (key == new KeyValue {FunctionKey = FunctionKeys.PreviousSuggestions}.Key
                    && (keyboardStateInfo.Suggestions == null
                        || !keyboardStateInfo.Suggestions.Any()
                        || keyboardStateInfo.SuggestionsPage == 0))
                {
                    return false;
                }

                //Next suggestions if no suggestions, or on last page
                if (key == new KeyValue { FunctionKey = FunctionKeys.NextSuggestions }.Key
                    && (keyboardStateInfo.Suggestions == null
                        || !keyboardStateInfo.Suggestions.Any()
                        || keyboardStateInfo.Suggestions.Count <= ((keyboardStateInfo.SuggestionsPage * keyboardStateInfo.SuggestionsPerPage) + keyboardStateInfo.SuggestionsPerPage)))
                {
                    return false;
                }

                //Suggestion 1 is only valid if suggestions exist for the appropriate index
                if (key == new KeyValue {FunctionKey = FunctionKeys.Suggestion1}.Key
                    && !SuggestionKeyIsValid(0))
                {
                    return false;
                }

                //Suggestion 2 is only valid if suggestions exist for the appropriate index
                if (key == new KeyValue { FunctionKey = FunctionKeys.Suggestion2 }.Key
                    && !SuggestionKeyIsValid(1))
                {
                    return false;
                }

                //Suggestion 3 is only valid if suggestions exist for the appropriate index
                if (key == new KeyValue { FunctionKey = FunctionKeys.Suggestion3 }.Key
                    && !SuggestionKeyIsValid(2))
                {
                    return false;
                }

                //Suggestion 4 is only valid if suggestions exist for the appropriate index
                if (key == new KeyValue { FunctionKey = FunctionKeys.Suggestion4 }.Key
                    && !SuggestionKeyIsValid(3))
                {
                    return false;
                }

                //Suggestion 5 is only valid if suggestions exist for the appropriate index
                if (key == new KeyValue { FunctionKey = FunctionKeys.Suggestion5 }.Key
                    && !SuggestionKeyIsValid(4))
                {
                    return false;
                }

                //Suggestion 6 is only valid if suggestions exist for the appropriate index
                if (key == new KeyValue { FunctionKey = FunctionKeys.Suggestion6 }.Key
                    && !SuggestionKeyIsValid(5))
                {
                    return false;
                }
                
                //Key is not a letter, but we're capturing a multi-key selection (which must be ended by selecting a letter)
                if (keyboardStateInfo.CapturingMultiKeySelection
                    && !KeyValueCollections.LetterKeys.Select(kv => kv.Key).Contains(key))
                {
                    return false;
                }

                return true;
            }
        }

        private bool SuggestionKeyIsValid(int index)
        {
            return keyboardStateInfo.Suggestions != null 
                && keyboardStateInfo.Suggestions.Any() 
                && keyboardStateInfo.Suggestions.Count > (keyboardStateInfo.SuggestionsPage * keyboardStateInfo.SuggestionsPerPage + index);
        }

        #endregion

        #region Notify State Changed

        public void NotifyStateChanged()
        {
            OnPropertyChanged(Binding.IndexerName);
        }

        #endregion
    }
}
