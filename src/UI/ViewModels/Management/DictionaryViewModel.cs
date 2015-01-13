using JuliusSweetland.ETTA.Services;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Management
{
    public class DictionaryViewModel : BindableBase
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IDictionaryService dictionaryService;
        
        #endregion
        
        #region Ctor

        public DictionaryViewModel(IDictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;
        
            AddCommand = new DelegateCommand<string>(Add, s => !string.IsNullOrEmpty(s));
            ToggleDeleteCommand = new DelegateCommand(ToggleDelete, () => !string.IsNullOrEmpty(NewEntry));
        
            Load();
        }
        
        #endregion
        
        #region Properties

        //Tuple signature is <entry, added, deleted>
        private ObservableCollection<Tuple<string, bool, bool>> entries;
        public ObservableCollection<Tuple<string, bool, bool>> Entries
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
                ToggleDeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ChangesRequireRestart
        {
            get { return false; }
        }
        
        public DelegateCommand<string> AddCommand { get; private set; }
        public DelegateCommand<string> ToggleDeleteCommand { get; private set; }        
        
        #endregion
        
        #region Methods

        private void Load()
        {
            var allDictionaryEntries = dictionaryService.GetAllEntriesWithUsageCounts();
            Entries = allDictionaryEntries != null
                ? new ObservableCollection<Tuple<string, bool, bool>(
                    allDictionaryEntries
                        .Select(e => new Tuple<string, bool, bool> { Item1 = e.Entry })
                        .OrderBy(tuple => tuple.Item1)
                        .ToList())
                : null;
        }
        
        private void Add()
        {
            if(Entries != null
               && !Entries.Any(e => e == NewEntry))
            {
                Entries.Add(new Tuple<string, bool, bool> { Item1 = NewEntry, Item2 = true });
            }
        }
        
        private void ToggleDelete(string entry)
        {
            if(Entries != null)
            {
                var match = Entries.FirstOrDefault(e => e == entry))
                if(match != null)
                {
                    match.Item3 = !match.Item3;
                }
            }
        }

        public void ApplyChanges()
        {
            if(Entries != null)
            {
                //Add new entries
                foreach(var newEntry in Entries.Where(e => e.Item2))
                {
                    dictionaryService.AddNewEntryToDictionary(newEntry);
                }
                
                //Remove deleted entries
                foreach(var deletedEntry in Entries.Where(e => e.Item3))
                {
                    dictionaryService.RemoveEntryFromDictionary(newEntry);
                }
            }
        }

        #endregion
    }
}
