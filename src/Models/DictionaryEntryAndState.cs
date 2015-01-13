using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Models
{
    public class DictionaryEntryAndState : BindableBase
    {
        private string entry;
        public string Entry
        {
            get { return entry; }
            set { SetProperty(ref entry, value); }
        }
        
        private bool added;
        public bool Added
        {
            get { return added; }
            set { SetProperty(ref added, value); }
        }
        
        private bool deleted;
        public bool Deleted
        {
            get { return deleted; }
            set { SetProperty(ref deleted, value); }
        }
    }
}
