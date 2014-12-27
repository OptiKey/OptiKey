using System.Threading.Tasks;
using System.Windows;

namespace JuliusSweetland.ETTA.Services
{
    public interface ICalibrationService
    {
        Task<string> Calibrate(Window parentWindow);
    }
}
