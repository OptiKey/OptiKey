using System.ComponentModel;
using System.Windows;

namespace JuliusSweetland.OptiKey.UI.ViewModels
{
    public interface ILookToScrollOverlayViewModel : INotifyPropertyChanged
    {
        bool IsLookToScrollActive { get; }
        Rect ActiveLookToScrollBounds { get; }
        Rect ActiveLookToScrollDeadzone { get; }
        Thickness ActiveLookToScrollMargins { get; } // between deadzone and border
    }
}
