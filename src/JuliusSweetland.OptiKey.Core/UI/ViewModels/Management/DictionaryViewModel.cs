// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.ObjectModel;
using System.Linq;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class DictionaryViewModel : BindableBase
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IDictionaryService dictionaryService;
        
        #endregion
        
        #region Ctor

        public DictionaryViewModel(IDictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;
        
            AddCommand = new DelegateCommand(Add, () => !string.IsNullOrEmpty(NewEntry));
            ToggleDeleteCommand = new DelegateCommand<string>(ToggleDelete, e => !string.IsNullOrEmpty(e));
            LoadCommand = new DelegateCommand(Load);
        }
        
        #endregion
        
        #region Properties

        private ObservableCollection<DictionaryEntryAndState> entries;
        public ObservableCollection<DictionaryEntryAndState> Entries
        {
            get { return entries; }
            set { SetProperty(ref entries, value); }
        }
        
        private string newEntry;
        public string NewEntry
        {
            get { return newEntry; }
            set 
            { 
                SetProperty(ref newEntry, value);
                AddCommand.RaiseCanExecuteChanged();
            }
        }

        public bool DictionaryIsNotPresage
        {
            get
            {
                return Settings.Default.SuggestionMethod != Enums.SuggestionMethods.Presage;
            }
        }

        public bool ChangesRequireRestart
        {
            get { return false; }
        }

        private bool isLoaded = false;
        public bool IsLoaded
        {
            set { SetProperty(ref isLoaded, value); }
            get { return isLoaded; }
        }

        public DelegateCommand AddCommand { get; private set; }
        public DelegateCommand<string> ToggleDeleteCommand { get; private set; }
        public DelegateCommand LoadCommand { get; private set; }

        #endregion
        
        #region Methods

        public void Load()
        {
            var allDictionaryEntries = dictionaryService.GetAllEntries();
            Entries = allDictionaryEntries != null
                ? new ObservableCollection<DictionaryEntryAndState>(
                    allDictionaryEntries
                        .Select(e => new DictionaryEntryAndState {Entry = e.Entry})
                        .OrderBy(e => e.Entry)
                        .ToList())
                : null;

            IsLoaded = true;

        }
        
        private void Add()
        {
            if(Entries != null
               && !Entries.Any(e => e.Entry == NewEntry))
            {
                Entries.Add(new DictionaryEntryAndState {Entry = NewEntry, Added = true});
            }
            
            NewEntry = null;
        }
        
        private void ToggleDelete(string entry)
        {
            if(Entries != null)
            {
                var match = Entries.FirstOrDefault(e => e.Entry == entry);
                if(match != null)
                {
                    match.Deleted = !match.Deleted;
                }
            }
        }

        public void ApplyChanges()
        {
            if(Entries != null)
            {
                //Add new entries
                foreach(var addedEntry in Entries.Where(e => e.Added).Select(e => e.Entry))
                {
                    dictionaryService.AddNewEntryToDictionary(addedEntry);
                }
                
                //Remove deleted entries
                foreach(var deletedEntry in Entries.Where(e => e.Deleted).Select(e => e.Entry))
                {
                    dictionaryService.RemoveEntryFromDictionary(deletedEntry);
                }
            }
        }

        #endregion
    }
}
