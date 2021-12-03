// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
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
