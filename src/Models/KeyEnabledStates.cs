using System;
using System.Linq;
using System.Windows.Data;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.UI.Controls;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Models
{
    public class KeyEnabledStates : BindableBase
    {
        #region Fields

        private readonly IKeyboardStateManager keyValueboardStateInfo;
        
        #endregion

        #region Ctor

        public KeyEnabledStates(IKeyboardStateManager keyValueboardStateInfo)
        {
            this.keyValueboardStateInfo = keyValueboardStateInfo;

            keyValueboardStateInfo.OnPropertyChanges(ksi => ksi.CapturingMultiKeySelection).Subscribe(_ => NotifyStateChanged());
            keyValueboardStateInfo.OnPropertyChanges(ksi => ksi.Suggestions).Subscribe(_ => NotifyStateChanged());
            keyValueboardStateInfo.OnPropertyChanges(ksi => ksi.SuggestionsPage).Subscribe(_ => NotifyStateChanged());
            keyValueboardStateInfo.OnPropertyChanges(ksi => ksi.SuggestionsPerPage).Subscribe(_ => NotifyStateChanged());

            keyValueboardStateInfo.KeyDownStates[KeyValues.AltKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyValueboardStateInfo.KeyDownStates[KeyValues.CtrlKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            
            Settings.Default.OnPropertyChanges(s => s.PublishingKeys).Subscribe(_ => NotifyStateChanged());
            Settings.Default.OnPropertyChanges(s => s.Sleeping).Subscribe(_ => NotifyStateChanged());
        }

        #endregion

        #region Indexer

        public bool this[KeyValue keyValue]
        {
            get
            {
                //Key is not Sleep, but we are sleeping
                if (Settings.Default.Sleeping
                    && keyValue != KeyValues.SleepKey)
                {
                    return false;
                }

                //Key is publish only, but we are not publishing
                if (!Settings.Default.PublishingKeys
                    && KeyValueCollections.PublishOnlyKeys.Contains(keyValue))
                {
                    return false;
                }
                
                //Key is MultiKeySelection, but Alt/Ctrl/Win are on or locked 
                if (keyValue == KeyValues.ToggleMultiKeySelectionSupportedKey
                    && (keyValueboardStateInfo.KeyDownStates[KeyValues.AltKey].Value.IsOnOrLock()
                        || keyValueboardStateInfo.KeyDownStates[KeyValues.CtrlKey].Value.IsOnOrLock()
                        || keyValueboardStateInfo.KeyDownStates[KeyValues.WinKey].Value.IsOnOrLock()))
                {
                    return false;
                }

                //Previous suggestions if no suggestions, or on page 1
                if (keyValue == KeyValues.PreviousSuggestionsKey
                    && (keyValueboardStateInfo.Suggestions == null
                        || !keyValueboardStateInfo.Suggestions.Any()
                        || keyValueboardStateInfo.SuggestionsPage == 0))
                {
                    return false;
                }

                //Next suggestions if no suggestions, or on last page
                if (keyValue == KeyValues.NextSuggestionsKey
                    && (keyValueboardStateInfo.Suggestions == null
                        || !keyValueboardStateInfo.Suggestions.Any()
                        || keyValueboardStateInfo.Suggestions.Count <= ((keyValueboardStateInfo.SuggestionsPage * keyValueboardStateInfo.SuggestionsPerPage) + keyValueboardStateInfo.SuggestionsPerPage)))
                {
                    return false;
                }

                //Suggestion 1 is only valid if suggestions exist for the appropriate index
                if (keyValue == KeyValues.Suggestion1Key
                    && !SuggestionKeyIsValid(0))
                {
                    return false;
                }

                //Suggestion 2 is only valid if suggestions exist for the appropriate index
                if (keyValue == KeyValues.Suggestion2Key
                    && !SuggestionKeyIsValid(1))
                {
                    return false;
                }

                //Suggestion 3 is only valid if suggestions exist for the appropriate index
                if (keyValue == KeyValues.Suggestion3Key
                    && !SuggestionKeyIsValid(2))
                {
                    return false;
                }

                //Suggestion 4 is only valid if suggestions exist for the appropriate index
                if (keyValue == KeyValues.Suggestion4Key
                    && !SuggestionKeyIsValid(3))
                {
                    return false;
                }

                //Suggestion 5 is only valid if suggestions exist for the appropriate index
                if (keyValue == KeyValues.Suggestion5Key
                    && !SuggestionKeyIsValid(4))
                {
                    return false;
                }

                //Suggestion 6 is only valid if suggestions exist for the appropriate index
                if (keyValue == KeyValues.Suggestion6Key
                    && !SuggestionKeyIsValid(5))
                {
                    return false;
                }
                
                //Key is not a letter, but we're capturing a multi-keyValue selection (which must be ended by selecting a letter)
                if (keyValueboardStateInfo.CapturingMultiKeySelection
                    && !KeyValueCollections.LetterKeys.Contains(keyValue))
                {
                    return false;
                }

                return true;
            }
        }

        private bool SuggestionKeyIsValid(int index)
        {
            return keyValueboardStateInfo.Suggestions != null 
                && keyValueboardStateInfo.Suggestions.Any() 
                && keyValueboardStateInfo.Suggestions.Count > (keyValueboardStateInfo.SuggestionsPage * keyValueboardStateInfo.SuggestionsPerPage + index);
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
