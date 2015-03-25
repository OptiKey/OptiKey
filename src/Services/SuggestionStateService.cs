using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class SuggestionStateService : BindableBase, ISuggestionStateService
    {
        private List<string> suggestions;
        public List<string> Suggestions
        {
            get { return suggestions; }
            set { SetProperty(ref suggestions, value); }
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
