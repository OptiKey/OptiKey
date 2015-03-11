using System.ComponentModel;
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IWindowStateService : INotifyPropertyChanged
    {
        Window Window { set; }
        WindowState? WindowState { get; }
    }
}
