using System.Collections.Generic;
using System.ComponentModel;

namespace JuliusSweetland.ETTA.Services
{
    public interface ISuggestionService : INotifyPropertyChanged
    {
        List<string> Suggestions { get; set; }
        int SuggestionsPage { get; set; }
        int SuggestionsPerPage { get; set; }
    }
}
