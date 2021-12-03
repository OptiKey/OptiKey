// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.ComponentModel;

namespace JuliusSweetland.OptiKey.Services
{
    public interface ISuggestionStateService : INotifyPropertyChanged
    {
        List<string> Suggestions { get; set; }
        int SuggestionsPage { get; set; }
        int SuggestionsPerPage { get; set; }
    }
}
