using System;
using System.Linq;
using System.Windows.Data;
using JuliusSweetland.ETTA.Extensions;
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

            keyValueboardStateInfo.KeyDownStates[KeyValues.PublishKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());
            keyValueboardStateInfo.KeyDownStates[KeyValues.SleepKey].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged());

            KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.ForEach(kv =>
                keyValueboardStateInfo.KeyDownStates[kv].OnPropertyChanges(np => np.Value).Subscribe(_ => NotifyStateChanged()));
        }

        #endregion

        #region Indexer

        public bool this[KeyValue keyValue]
        {
            get
            {
                //Key is not Sleep, but we are sleeping
                if (keyValueboardStateInfo.KeyDownStates[KeyValues.SleepKey].Value.IsDownOrLockedDown()
                    && keyValue != KeyValues.SleepKey)
                {
                    return false;
                }

                //Key is publish only, but we are not publishing
                if (!keyValueboardStateInfo.KeyDownStates[KeyValues.PublishKey].Value.IsDownOrLockedDown()
                    && KeyValues.PublishOnlyKeys.Contains(keyValue))
                {
                    return false;
                }
                
                //Key is MultiKeySelection, but a key which prevents text capture is down or locked down
                if (keyValue == KeyValues.MultiKeySelectionEnabledKey
                    && KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                        keyValueboardStateInfo.KeyDownStates[kv].Value.IsDownOrLockedDown()))
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
                    && !KeyValues.LetterKeys.Contains(keyValue))
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
