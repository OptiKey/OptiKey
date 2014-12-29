using System.ComponentModel;

namespace JuliusSweetland.ETTA.Models
{
    public interface ICapturingStateManager : INotifyPropertyChanged
    {
        bool CapturingMultiKeySelection { get; set; }
    }
}
