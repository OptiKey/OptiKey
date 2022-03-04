// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;

namespace JuliusSweetland.OptiKey.Models
{
    public interface ILastMouseActionStateManager : INotifyPropertyChanged
    {
        Action LastMouseAction { get; set; }
        bool LastMouseActionExists { get; }
    }
}
