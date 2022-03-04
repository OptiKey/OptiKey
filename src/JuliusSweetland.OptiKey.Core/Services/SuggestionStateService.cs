// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class SuggestionStateService : BindableBase, ISuggestionStateService
    {
        private List<string> suggestions;
        public List<string> Suggestions
        {
            get { return suggestions; }
            set
            {
                SetProperty(ref suggestions, value);
                SuggestionsPage = 0; //Reset page
            }
        }

        private int suggestionsPage;
        public int SuggestionsPage
        {
            get { return suggestionsPage; }
            set { SetProperty(ref suggestionsPage, value); }
        }

        private int suggestionsPerPage;
        public int SuggestionsPerPage
        {
            get { return suggestionsPerPage; }
            set { SetProperty(ref suggestionsPerPage, value); }
        }
    }
}
