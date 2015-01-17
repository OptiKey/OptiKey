using System.Collections.Generic;
using System.ComponentModel;

namespace JuliusSweetland.OptiKey.Services
{
    public interface ISuggestionService : INotifyPropertyChanged
    {
        List<string> Suggestions { get; set; }
        int SuggestionsPage { get; set; }
        int SuggestionsPerPage { get; set; }
    }
}
