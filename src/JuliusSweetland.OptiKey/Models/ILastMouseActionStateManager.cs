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
