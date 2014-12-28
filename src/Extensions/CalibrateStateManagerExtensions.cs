using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class CalibrateStateManagerExtensions
    {
        public static bool CanCalibrate(this ICalibrateStateManager csm)
        {
            return csm.CalibrationService != null;
        }
    }
}
