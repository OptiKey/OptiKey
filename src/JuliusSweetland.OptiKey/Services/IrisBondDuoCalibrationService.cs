using System;
using System.Threading.Tasks;
using System.Windows;
using JuliusSweetland.OptiKey.Native.IrisBond;
using log4net;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Services
{
    public class IrisBondDuoCalibrationService : ICalibrationService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private PInvoke.CALIBRATION_RESULTS_CALLBACK onCalibrationResultsCallback;
        private TaskCompletionSource<string> taskCompletionSource;

        public async Task<string> Calibrate(Window parentWindow)
        {
            Log.Info("Attempting to calibrate using the IrisBond engine.");

            taskCompletionSource = new TaskCompletionSource<string>(); //Used to make this method awaitable on the InteractionRequest callback

            onCalibrationResultsCallback = OnCalibrationResults;

            PInvoke.setCalibrationResultsCallback(onCalibrationResultsCallback);

            PInvoke.startCalibration(9);
            PInvoke.waitForCalibrationToEnd(1);

            return await taskCompletionSource.Task;
        }

        public bool CanBeCompletedWithoutManualIntervention { get { return true; } }

        #region Callbacks

        public void OnCalibrationResults(double leftPrecisionError, double leftAccuracyError,
            double rightPrecisionError, double rightAccuracyError,
            double combinedPrecisionError, double combinedAccuracyError,
            bool cancelled)
        {
            Log.Info("Calibration ended.");
            taskCompletionSource.SetResult(Resources.IRISBOND_CALIBRATION_COMPLETE);
        }

        #endregion
    }
}
