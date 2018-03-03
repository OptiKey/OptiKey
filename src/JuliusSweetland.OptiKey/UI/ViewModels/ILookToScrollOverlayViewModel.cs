using System.ComponentModel;
using System.Windows;

namespace JuliusSweetland.OptiKey.UI.ViewModels
{
    public interface ILookToScrollOverlayViewModel : INotifyPropertyChanged
    {
        Rect ActiveLookToScrollBounds { get; }
    }
}
