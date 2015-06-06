using System.Threading.Tasks;
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface ICalibrationService
    {
        Task<string> Calibrate(Window parentWindow);
        bool CanBeCompletedWithoutManualIntervention { get; }
    }
}
