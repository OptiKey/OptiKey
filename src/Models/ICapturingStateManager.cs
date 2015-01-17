using System.ComponentModel;

namespace JuliusSweetland.OptiKey.Models
{
    public interface ICapturingStateManager : INotifyPropertyChanged
    {
        bool CapturingMultiKeySelection { get; set; }
    }
}
