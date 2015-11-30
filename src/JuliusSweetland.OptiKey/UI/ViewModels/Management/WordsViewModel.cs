using System.Collections.Generic;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class WordsViewModel : BindableBase
    {
        private readonly IDictionaryService dictionaryService;

        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public WordsViewModel(IDictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;

            Load();
        }

        #endregion
        
        #region Properties
        
        public List<KeyValuePair<string, Languages>> Languages
        {
            get
            {
                return new List<KeyValuePair<string, Languages>>
                {
                    new KeyValuePair<string, Languages>(Resources.ENGLISH_CANADA, Enums.Languages.EnglishCanada),
                    new KeyValuePair<string, Languages>(Resources.ENGLISH_UK, Enums.Languages.EnglishUK),
                    new KeyValuePair<string, Languages>(Resources.ENGLISH_US, Enums.Languages.EnglishUS),
                    new KeyValuePair<string, Languages>(Resources.FRENCH_FRANCE, Enums.Languages.FrenchFrance),
                    new KeyValuePair<string, Languages>(Resources.GERMAN_GERMANY, Enums.Languages.GermanGermany),
                    new KeyValuePair<string, Languages>(Resources.DUTCH_BELGIUM, Enums.Languages.DutchBelgium)
                };
            }
        }
        
        private Languages keyboardLanguage;
        public Languages KeyboardLanguage
        {
            get { return keyboardLanguage; }
            set { SetProperty(ref this.keyboardLanguage, value); }
        }

        private Languages resourceLanguage;
        public Languages ResourceLanguage
        {
            get { return resourceLanguage; }
            set { SetProperty(ref this.resourceLanguage, value); }
        }
        
        private bool autoAddSpace;
        public bool AutoAddSpace
        {
            get { return autoAddSpace; }
            set { SetProperty(ref autoAddSpace, value); }
        }
        
        private bool autoCapitalise;
        public bool AutoCapitalise
        {
            get { return autoCapitalise; }
            set { SetProperty(ref autoCapitalise, value); }
        }

        private bool suppressAutoCapitaliseIntelligently;
        public bool SuppressAutoCapitaliseIntelligently
        {
            get { return suppressAutoCapitaliseIntelligently; }
            set { SetProperty(ref suppressAutoCapitaliseIntelligently, value); }
        }

        private bool autoCompleteWords;
        public bool AutoCompleteWords
        {
            get {  return autoCompleteWords; }
            set { SetProperty(ref autoCompleteWords, value); }
        }

        private bool multiKeySelectionEnabled;
        public bool MultiKeySelectionEnabled
        {
            get { return multiKeySelectionEnabled; }
            set { SetProperty(ref multiKeySelectionEnabled, value); }
        }

        private int multiKeySelectionMaxDictionaryMatches;
        public int MultiKeySelectionMaxDictionaryMatches
        {
            get { return multiKeySelectionMaxDictionaryMatches; }
            set { SetProperty(ref multiKeySelectionMaxDictionaryMatches, value); }
        }

        public bool ChangesRequireRestart
        {
            get { return false; }
        }
        
        #endregion
        
        #region Methods

        private void Load()
        {
            KeyboardLanguage = Settings.Default.KeyboardLanguage;
            ResourceLanguage = Settings.Default.ResourceLanguage;
            AutoAddSpace = Settings.Default.AutoAddSpace;
            AutoCapitalise = Settings.Default.AutoCapitalise;
            SuppressAutoCapitaliseIntelligently = Settings.Default.SuppressAutoCapitaliseIntelligently;
            AutoCompleteWords = Settings.Default.AutoCompleteWords;
            MultiKeySelectionEnabled = Settings.Default.MultiKeySelectionEnabled;
            MultiKeySelectionMaxDictionaryMatches = Settings.Default.MaxDictionaryMatchesOrSuggestions;
        }

        public void ApplyChanges()
        {
            bool reloadDictionary = Settings.Default.ResourceLanguage != ResourceLanguage;

            Settings.Default.KeyboardLanguage = KeyboardLanguage;
            Settings.Default.ResourceLanguage = ResourceLanguage;
            Settings.Default.AutoAddSpace = AutoAddSpace;
            Settings.Default.AutoCapitalise = AutoCapitalise;
            Settings.Default.SuppressAutoCapitaliseIntelligently = SuppressAutoCapitaliseIntelligently;
            Settings.Default.AutoCompleteWords = AutoCompleteWords;
            Settings.Default.MultiKeySelectionEnabled = MultiKeySelectionEnabled;
            Settings.Default.MaxDictionaryMatchesOrSuggestions = MultiKeySelectionMaxDictionaryMatches;
            
            if (reloadDictionary)
            {
                dictionaryService.LoadDictionary();
            }
        }

        #endregion
    }
}
