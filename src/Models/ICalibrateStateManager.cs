using System.ComponentModel;
using JuliusSweetland.ETTA.Services;

namespace JuliusSweetland.ETTA.Models
{
    public interface ICalibrateStateManager : INotifyPropertyChanged
    {
        ICalibrateService CalibrateService { get; }
    }
}
