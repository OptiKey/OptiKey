// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.ComponentModel;

namespace JuliusSweetland.OptiKey.Models
{
    public interface ICapturingStateManager : INotifyPropertyChanged
    {
        bool CapturingMultiKeySelection { get; set; }
    }
}
