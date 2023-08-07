// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using log4net;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class WordsViewModel : BindableBase
    {
        private readonly IDictionaryService dictionaryService;

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public WordsViewModel(IDictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;

            Load();
        }

        #endregion

        #region Properties

        public static List<KeyValuePair<string, Languages>> Languages
        {
            get
            {
                return new List<KeyValuePair<string, Languages>>
                {
                    new KeyValuePair<string, Languages>(Resources.CATALAN_SPAIN, Enums.Languages.CatalanSpain),
                    new KeyValuePair<string, Languages>(Resources.CROATIAN_CROATIA, Enums.Languages.CroatianCroatia),
                    new KeyValuePair<string, Languages>(Resources.CZECH_CZECH_REPUBLIC, Enums.Languages.CzechCzechRepublic),
                    new KeyValuePair<string, Languages>(Resources.DANISH_DENMARK, Enums.Languages.DanishDenmark),
                    new KeyValuePair<string, Languages>(Resources.DUTCH_BELGIUM, Enums.Languages.DutchBelgium),
                    new KeyValuePair<string, Languages>(Resources.DUTCH_NETHERLANDS, Enums.Languages.DutchNetherlands),
                    new KeyValuePair<string, Languages>(Resources.ENGLISH_CANADA, Enums.Languages.EnglishCanada),
                    new KeyValuePair<string, Languages>(Resources.ENGLISH_UK, Enums.Languages.EnglishUK),
                    new KeyValuePair<string, Languages>(Resources.ENGLISH_US, Enums.Languages.EnglishUS),
                    new KeyValuePair<string, Languages>(Resources.FINNISH_FINLAND, Enums.Languages.FinnishFinland),
                    new KeyValuePair<string, Languages>(Resources.FRENCH_CANADA, Enums.Languages.FrenchCanada),
                    new KeyValuePair<string, Languages>(Resources.FRENCH_FRANCE, Enums.Languages.FrenchFrance),
                    new KeyValuePair<string, Languages>(Resources.GEORGIAN_GEORGIA, Enums.Languages.GeorgianGeorgia),
                    new KeyValuePair<string, Languages>(Resources.GERMAN_GERMANY, Enums.Languages.GermanGermany),
                    new KeyValuePair<string, Languages>(Resources.GREEK_GREECE, Enums.Languages.GreekGreece),
                    new KeyValuePair<string, Languages>(Resources.HEBREW_ISRAEL, Enums.Languages.HebrewIsrael),
                    new KeyValuePair<string, Languages>(Resources.HINDI_INDIA, Enums.Languages.HindiIndia),
                    new KeyValuePair<string, Languages>(Resources.HUNGARIAN_HUNGARY, Enums.Languages.HungarianHungary),
                    new KeyValuePair<string, Languages>(Resources.ITALIAN_ITALY, Enums.Languages.ItalianItaly),
                    new KeyValuePair<string, Languages>(Resources.JAPANESE_JAPAN, Enums.Languages.JapaneseJapan),
                    new KeyValuePair<string, Languages>(Resources.KOREAN_KOREA, Enums.Languages.KoreanKorea),
                    new KeyValuePair<string, Languages>(Resources.PERSIAN_IRAN, Enums.Languages.PersianIran),
                    new KeyValuePair<string, Languages>(Resources.POLISH_POLAND, Enums.Languages.PolishPoland),
                    new KeyValuePair<string, Languages>(Resources.PORTUGUESE_PORTUGAL, Enums.Languages.PortuguesePortugal),
                    new KeyValuePair<string, Languages>(Resources.PORTUGUESE_BRAZILIAN, Enums.Languages.PortugueseBrazil),
                    new KeyValuePair<string, Languages>(Resources.RUSSIAN_RUSSIA, Enums.Languages.RussianRussia),
                    new KeyValuePair<string, Languages>(Resources.SERBIAN_SERBIA, Enums.Languages.SerbianSerbia),
                    new KeyValuePair<string, Languages>(Resources.SLOVAK_SLOVAKIA, Enums.Languages.SlovakSlovakia),
                    new KeyValuePair<string, Languages>(Resources.SLOVENIAN_SLOVENIA, Enums.Languages.SlovenianSlovenia),
                    new KeyValuePair<string, Languages>(Resources.SPANISH_SPAIN, Enums.Languages.SpanishSpain),
                    new KeyValuePair<string, Languages>(Resources.TURKISH_TURKEY, Enums.Languages.TurkishTurkey),
                    new KeyValuePair<string, Languages>(Resources.UKRAINIAN_UKRAINE, Enums.Languages.UkrainianUkraine),
                    new KeyValuePair<string, Languages>(Resources.URDU_PAKISTAN, Enums.Languages.UrduPakistan),
                };
            }
        }

        public List<KeyValuePair<string, KeyboardLayouts>> KeyboardLayouts
        {
            get
            {
                var keyboardLayouts = new List<KeyValuePair<string, KeyboardLayouts>>
                {
                    new KeyValuePair<string, KeyboardLayouts>(Resources.USE_DEFAULT_KEYBOARD_LAYOUT, Enums.KeyboardLayouts.Default)
                };

                if (UseAlphabeticalKeyboardLayoutIsVisible)
                {
                    keyboardLayouts.Add(new KeyValuePair<string, KeyboardLayouts>(Resources.USE_ALPHABETICAL_KEYBOARD_LAYOUT, Enums.KeyboardLayouts.Alphabetic));
                }
                if (UseSimplifiedKeyboardLayoutIsVisible)
                {
                    keyboardLayouts.Add(new KeyValuePair<string, KeyboardLayouts>(Resources.USE_SIMPLIFIED_KEYBOARD_LAYOUT, Enums.KeyboardLayouts.Simplified));
                }
                
                // TODO: should this ever be unavailable?
                keyboardLayouts.Add(new KeyValuePair<string, KeyboardLayouts>(Resources.USE_COMMUNIKATE_KEYBOARD_LAYOUT, Enums.KeyboardLayouts.Communikate));
                

                return keyboardLayouts;
            }
        }

        public List<KeyValuePair<string, SuggestionMethods>> SuggestionMethods
        {
            get
            {
                return new List<KeyValuePair<string, SuggestionMethods>> {
                        new KeyValuePair<string, SuggestionMethods>(Resources.BASIC_SUGGESTION, Enums.SuggestionMethods.Basic),
                        new KeyValuePair<string, SuggestionMethods>(Resources.NGRAM_SUGGESTION, Enums.SuggestionMethods.NGram),
                        new KeyValuePair<string, SuggestionMethods>(Resources.PRESAGE_SUGGESTION, Enums.SuggestionMethods.Presage)
                    };
            }
        }

        private Languages keyboardAndDictionaryLanguage;
        public Languages KeyboardAndDictionaryLanguage
        {
            get { return keyboardAndDictionaryLanguage; }
            set
            {
                SetProperty(ref this.keyboardAndDictionaryLanguage, value);
                OnPropertyChanged(() => UseAlphabeticalKeyboardLayoutIsVisible);
                OnPropertyChanged(() => UseSimplifiedKeyboardLayoutIsVisible);
                OnPropertyChanged(() => KeyboardLayouts);

                if (KeyboardLayouts.Count == 1)
                {
                    KeyboardLayout = KeyboardLayouts.First().Value;
                }
            }
        }

        private bool displayVoicesWhenChangingKeyboardLanguage;
        public bool DisplayVoicesWhenChangingKeyboardLanguage
        {
            get { return displayVoicesWhenChangingKeyboardLanguage; }
            set { SetProperty(ref displayVoicesWhenChangingKeyboardLanguage, value); }
        }

        private KeyboardLayouts keyboardLayout;
        public KeyboardLayouts KeyboardLayout
        {
            get { return keyboardLayout; }
            set { SetProperty(ref keyboardLayout, value); }
        }

        private Languages uiLanguage;
        public Languages UiLanguage
        {
            get { return uiLanguage; }
            set { SetProperty(ref this.uiLanguage, value); }
        }

        private bool useAlphabeticalKeyboardLayout;
        public bool UseAlphabeticalKeyboardLayout
        {
            get { return KeyboardLayout == Enums.KeyboardLayouts.Alphabetic; }
            set
            {
                SetProperty(ref useAlphabeticalKeyboardLayout,
                      KeyboardLayout == Enums.KeyboardLayouts.Alphabetic);
            }
        }

        public bool UseAlphabeticalKeyboardLayoutIsVisible
        {
            get
            {
                return KeyboardAndDictionaryLanguage == Enums.Languages.EnglishCanada
                       || KeyboardAndDictionaryLanguage == Enums.Languages.EnglishUK
                       || KeyboardAndDictionaryLanguage == Enums.Languages.EnglishUS
                       || KeyboardAndDictionaryLanguage == Enums.Languages.GermanGermany
                       || KeyboardAndDictionaryLanguage == Enums.Languages.ItalianItaly;
            }
        }

        private bool useSimplifiedKeyboardLayout;
        public bool UseSimplifiedKeyboardLayout
        {
            get { return KeyboardLayout == Enums.KeyboardLayouts.Simplified; }
            set
            {
                SetProperty(ref useSimplifiedKeyboardLayout,
                      KeyboardLayout == Enums.KeyboardLayouts.Simplified);
            }
        }

        public bool UseSimplifiedKeyboardLayoutIsVisible
        {
            get
            {
                return KeyboardAndDictionaryLanguage == Enums.Languages.EnglishCanada
                       || KeyboardAndDictionaryLanguage == Enums.Languages.EnglishUK
                       || KeyboardAndDictionaryLanguage == Enums.Languages.EnglishUS
                       || KeyboardAndDictionaryLanguage == Enums.Languages.HebrewIsrael
                       || KeyboardAndDictionaryLanguage == Enums.Languages.JapaneseJapan
                       || KeyboardAndDictionaryLanguage == Enums.Languages.TurkishTurkey
                       || KeyboardAndDictionaryLanguage == Enums.Languages.GeorgianGeorgia
                       || KeyboardAndDictionaryLanguage == Enums.Languages.GermanGermany;
            }
        }        
        
        public bool PresageSettingsAreVisible
        {
            get { return SuggestionMethod == Enums.SuggestionMethods.Presage; }
        }

        private string presageDatabaseLocation;
        public string PresageDatabaseLocation
        {
            get { return presageDatabaseLocation; }
            set { SetProperty(ref presageDatabaseLocation, value); }
        }

        private int presageNumberOfSuggestions;
        public int PresageNumberOfSuggestions
        {
            get { return presageNumberOfSuggestions; }
            set { SetProperty(ref presageNumberOfSuggestions, value); }
        }

        private bool forceCapsLock;
        public bool ForceCapsLock
        {
            get { return forceCapsLock; }
            set { SetProperty(ref forceCapsLock, value); }
        }
        
        private bool typeDiacriticsAfterLetters;
        public bool TypeDiacriticsAfterLetters
        {
            get { return typeDiacriticsAfterLetters; }
            set { SetProperty(ref typeDiacriticsAfterLetters, value); }
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

        private bool limitBackOne;
        public bool LimitBackOne
        {
            get { return limitBackOne; }
            set { SetProperty(ref limitBackOne, value); }
        }

        private bool suppressAutoCapitaliseIntelligently;
        public bool SuppressAutoCapitaliseIntelligently
        {
            get { return suppressAutoCapitaliseIntelligently; }
            set { SetProperty(ref suppressAutoCapitaliseIntelligently, value); }
        }

        private SuggestionMethods suggestionMethod;
        public SuggestionMethods SuggestionMethod
        {
            get { return suggestionMethod; }
            set
            {
                SetProperty(ref suggestionMethod, value);
                OnPropertyChanged(() => PresageSettingsAreVisible);
            }
        }

        private bool suggestWords;
        public bool SuggestWords
        {
            get { return suggestWords; }
            set { SetProperty(ref suggestWords, value); }
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
            get { return Settings.Default.UiLanguage != UiLanguage
                    || ForceCapsLock != Settings.Default.ForceCapsLock
                    || Settings.Default.SuggestionMethod != SuggestionMethod
                    || Settings.Default.PresageDatabaseLocation != PresageDatabaseLocation
                    || Settings.Default.PresageNumberOfSuggestions != PresageNumberOfSuggestions; }
        }

        #endregion

        #region Methods

        private void Load()
        {
            KeyboardAndDictionaryLanguage = Settings.Default.KeyboardAndDictionaryLanguage;
            DisplayVoicesWhenChangingKeyboardLanguage = Settings.Default.DisplayVoicesWhenChangingKeyboardLanguage;
            UiLanguage = Settings.Default.UiLanguage;
            KeyboardLayout = Settings.Default.KeyboardLayout;
            UseAlphabeticalKeyboardLayout = Settings.Default.UseAlphabeticalKeyboardLayout;
            UseSimplifiedKeyboardLayout = Settings.Default.UseSimplifiedKeyboardLayout;
            ForceCapsLock = Settings.Default.ForceCapsLock;
            TypeDiacriticsAfterLetters = Settings.Default.TypeDiacriticsAfterLetters;
            AutoAddSpace = Settings.Default.AutoAddSpace;
            AutoCapitalise = Settings.Default.AutoCapitalise;
            SuppressAutoCapitaliseIntelligently = Settings.Default.SuppressAutoCapitaliseIntelligently;
            SuggestionMethod = Settings.Default.SuggestionMethod;
            PresageDatabaseLocation = Settings.Default.PresageDatabaseLocation;
            PresageNumberOfSuggestions = Settings.Default.PresageNumberOfSuggestions;
            SuggestWords = Settings.Default.SuggestWords;
            MultiKeySelectionEnabled = Settings.Default.MultiKeySelectionEnabled;
            MultiKeySelectionMaxDictionaryMatches = Settings.Default.MaxDictionaryMatchesOrSuggestions;
            LimitBackOne = Settings.Default.LimitBackOne;
        }

        public void ApplyChanges()
        {
            var reloadDictionary = (Settings.Default.KeyboardAndDictionaryLanguage != KeyboardAndDictionaryLanguage)
                                   || (Settings.Default.SuggestionMethod != SuggestionMethod);

            Settings.Default.KeyboardAndDictionaryLanguage = KeyboardAndDictionaryLanguage;
            Settings.Default.DisplayVoicesWhenChangingKeyboardLanguage = DisplayVoicesWhenChangingKeyboardLanguage;
            Settings.Default.UiLanguage = UiLanguage;
            Settings.Default.KeyboardLayout = KeyboardLayout;
            Settings.Default.UseAlphabeticalKeyboardLayout = UseAlphabeticalKeyboardLayout;          
            // TODO: Remove these bools, the state is tangled.
            Settings.Default.UseCommuniKateKeyboardLayoutByDefault = (KeyboardLayout == Enums.KeyboardLayouts.Communikate);
            Settings.Default.UsingCommuniKateKeyboardLayout = Settings.Default.UseCommuniKateKeyboardLayoutByDefault;            
            Settings.Default.UseSimplifiedKeyboardLayout = UseSimplifiedKeyboardLayout;
            Settings.Default.ForceCapsLock = ForceCapsLock;
            Settings.Default.TypeDiacriticsAfterLetters = TypeDiacriticsAfterLetters;
            Settings.Default.AutoAddSpace = AutoAddSpace;
            Settings.Default.AutoCapitalise = AutoCapitalise;
            Settings.Default.SuppressAutoCapitaliseIntelligently = SuppressAutoCapitaliseIntelligently;
            Settings.Default.SuggestionMethod = SuggestionMethod;
            Settings.Default.PresageDatabaseLocation = PresageDatabaseLocation;
            Settings.Default.PresageNumberOfSuggestions = PresageNumberOfSuggestions;
            Settings.Default.SuggestWords = SuggestWords;
            Settings.Default.MultiKeySelectionEnabled = MultiKeySelectionEnabled;
            Settings.Default.MaxDictionaryMatchesOrSuggestions = MultiKeySelectionMaxDictionaryMatches;
            Settings.Default.LimitBackOne = LimitBackOne;

            if (reloadDictionary)
            {
                dictionaryService.LoadDictionary();
            }
        }

        #endregion
    }
}
