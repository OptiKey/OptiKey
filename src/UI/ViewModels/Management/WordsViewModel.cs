using System.Collections.Generic;
using System.Windows;
using System;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class WordsViewModel : BindableBase
    {
        private readonly IDictionaryService dictionaryService;

        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private LocalisationService LService = App.LService;
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
                List<KeyValuePair<string, Languages>> LanguagesList = new List<KeyValuePair<string, Languages>>();
                // getting all languages from enums
                 foreach (Languages lgg in System.Enum.GetValues(typeof(Languages)))
                 {
                     LanguagesList.Add(new KeyValuePair<string, Languages>(lgg.ToFullDescription(), lgg));
                 }
                return LanguagesList;
            }
        }
        
        private Languages language;
        public Languages Language
        {
            get { return language; }
            set { SetProperty(ref this.language, value); }
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
            Language = Settings.Default.Language;
            AutoAddSpace = Settings.Default.AutoAddSpace;
            AutoCapitalise = Settings.Default.AutoCapitalise;
            SuppressAutoCapitaliseIntelligently = Settings.Default.SuppressAutoCapitaliseIntelligently;
            AutoCompleteWords = Settings.Default.AutoCompleteWords;
            MultiKeySelectionEnabled = Settings.Default.MultiKeySelectionEnabled;
            MultiKeySelectionMaxDictionaryMatches = Settings.Default.MaxDictionaryMatchesOrSuggestions;
        }

        public void ApplyChanges()
        {
            bool reloadDictionary = Settings.Default.Language != Language;

            Settings.Default.Language = Language;
            Settings.Default.AutoAddSpace = AutoAddSpace;
            Settings.Default.AutoCapitalise = AutoCapitalise;
            Settings.Default.SuppressAutoCapitaliseIntelligently = SuppressAutoCapitaliseIntelligently;
            Settings.Default.AutoCompleteWords = AutoCompleteWords;
            Settings.Default.MultiKeySelectionEnabled = MultiKeySelectionEnabled;
            Settings.Default.MaxDictionaryMatchesOrSuggestions = MultiKeySelectionMaxDictionaryMatches;
            
            if (reloadDictionary)
            {
                dictionaryService.LoadDictionary();
                //apply translations
                ResourceDictionary dict = new ResourceDictionary();
                if (LService == null)
                {
                    LService = new LocalisationService(App.Current.Resources, Settings.Default.Language.ToDescription(), null, "Resources\\Localisation\\", "Strings");
                }
                string lgg = Settings.Default.Language.ToDescription();
                dict.Source = new Uri(LService.getURIFileBaseNames(LService.CurrentLggPath, lgg), UriKind.Relative);
                Application.Current.Resources.MergedDictionaries.Add(dict);          
            }
        }

        #endregion
    }
}
