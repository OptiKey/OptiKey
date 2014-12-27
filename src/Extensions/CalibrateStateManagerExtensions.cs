using System.Globalization;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.ETTA.Enums;
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
