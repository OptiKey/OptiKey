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
        
        #endregion

        #region Ctor

        public KeyEnabledStates(IKeyboardStateManager keyboardStateInfo)
        {
            this.keyboardStateInfo = keyboardStateInfo;

            keyboardStateInfo.OnPropertyChanges(ksi => ksi.CapturingMultiKeySelection).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.OnPropertyChanges(ksi => ksi.Suggestions).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.OnPropertyChanges(ksi => ksi.SuggestionsPage).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.OnPropertyChanges(ksi => ksi.SuggestionsPerPage).Subscribe(_ => NotifyStateChanged());

            keyboardStateInfo.KeyDownStates[KeyValueKeys.AltKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyboardStateInfo.KeyDownStates[KeyValueKeys.CtrlKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            
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

                //Key is MultiKeySelection, but Ctrl or Alt are on or locked 
                if (key == KeyValueKeys.ToggleMultiKeySelectionSupportedKey
                    && (keyboardStateInfo.KeyDownStates[KeyValueKeys.AltKey].Value.IsOnOrLock()
                        || keyboardStateInfo.KeyDownStates[KeyValueKeys.CtrlKey].Value.IsOnOrLock()))
                {
                    return false;
                }

                //Previous suggestions if no suggestions, or on page 1
                if (key == KeyValueKeys.PreviousSuggestionsKey
                    && (keyboardStateInfo.Suggestions == null
                        || !keyboardStateInfo.Suggestions.Any()
                        || keyboardStateInfo.SuggestionsPage == 0))
                {
                    return false;
                }

                //Next suggestions if no suggestions, or on last page
                if (key == KeyValueKeys.NextSuggestionsKey
                    && (keyboardStateInfo.Suggestions == null
                        || !keyboardStateInfo.Suggestions.Any()
                        || keyboardStateInfo.Suggestions.Count <= ((keyboardStateInfo.SuggestionsPage * keyboardStateInfo.SuggestionsPerPage) + keyboardStateInfo.SuggestionsPerPage)))
                {
                    return false;
                }

                //Suggestion 1 is only valid if suggestions exist for the appropriate index
                if (key == KeyValueKeys.Suggestion1Key
                    && !SuggestionKeyIsValid(0))
                {
                    return false;
                }

                //Suggestion 2 is only valid if suggestions exist for the appropriate index
                if (key == KeyValueKeys.Suggestion2Key
                    && !SuggestionKeyIsValid(1))
                {
                    return false;
                }

                //Suggestion 3 is only valid if suggestions exist for the appropriate index
                if (key == KeyValueKeys.Suggestion3Key
                    && !SuggestionKeyIsValid(2))
                {
                    return false;
                }

                //Suggestion 4 is only valid if suggestions exist for the appropriate index
                if (key == KeyValueKeys.Suggestion4Key
                    && !SuggestionKeyIsValid(3))
                {
                    return false;
                }

                //Suggestion 5 is only valid if suggestions exist for the appropriate index
                if (key == KeyValueKeys.Suggestion5Key
                    && !SuggestionKeyIsValid(4))
                {
                    return false;
                }

                //Suggestion 6 is only valid if suggestions exist for the appropriate index
                if (key == KeyValueKeys.Suggestion6Key
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
