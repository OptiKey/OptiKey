using System;
using System.Linq;
using System.Windows.Data;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Properties;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Models
{
    public class KeyValidStates : BindableBase
    {
        #region Fields

        private readonly IKeyboardStateInfo keyboardStateInfo;

        #endregion

        #region Ctor

        public KeyValidStates(IKeyboardStateInfo keyboardStateInfo)
        {
            this.keyboardStateInfo = keyboardStateInfo;

            Settings.Default.OnPropertyChanges(settings => settings.PublishingKeys).Subscribe(_ => NotifyStateChanged());
        }

        #endregion

        #region Indexer

        public bool this[string key]
        {
            get
            {
                //Key is publish only, but we are not publishing
                if (!Settings.Default.PublishingKeys)
                {
                    if (KeyValueCollections.PublishOnlyKeys.Select(kv => kv.Key).Contains(key))
                    {
                        return false;
                    }
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
                        || keyboardStateInfo.Suggestions.Count > ((keyboardStateInfo.SuggestionsPage * keyboardStateInfo.SuggestionsPerPage) + keyboardStateInfo.SuggestionsPerPage)))
                {
                    return false;
                }

                return true;
            }
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
