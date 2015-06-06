using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using TETControls.Calibration;

namespace JuliusSweetland.OptiKey.Services
{
    public class TheEyeTribeCalibrationService : ICalibrationService
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public async Task<string> Calibrate(Window parentWindow)
        {
            Log.Debug("Attempting to calibrate using the TheEyeTribe calibration runner.");
            
            var calRunner = new CalibrationRunner();

            if (parentWindow != null)
            {
                calRunner.Owner = parentWindow; //Setting the owner preserves the z-order of the parent and child windows if the focus is shifted away from the child window (otherwise the child popup will be hidden)
            }

            var resultTask = Observable
                .FromEventPattern<CalibrationRunnerEventArgs>(
                    eh => calRunner.OnResult += eh,
                    eh => calRunner.OnResult -= eh)
                .FirstAsync()
                .Select(i => i.EventArgs)
                .ToTask();

            calRunner.Width = 100;
            calRunner.Height = 100;

            calRunner.Start();

            var calibrateArgs = await resultTask;

            calRunner.Owner = null;
            calRunner.Close();

            switch (calibrateArgs.Result)
            {
                case CalibrationRunnerResult.Success:
                    var message = string.Format("Calibration success! Accuracy (Avg Error Degree = {0})",
                        calibrateArgs.CalibrationResult.AverageErrorDegree);
                    Log.Debug(message);
                    return message;

                case CalibrationRunnerResult.Abort:
                    throw new ApplicationException(string.Format("Calibration aborted with message: '{0}'", calibrateArgs.Message));

                case CalibrationRunnerResult.Error:
                    throw new ApplicationException(string.Format("An error occurred during calibration. Message: '{0}'", calibrateArgs.Message));

                case CalibrationRunnerResult.Failure:
                    throw new ApplicationException(string.Format("Calibration failed with message: '{0}'", calibrateArgs.Message));

                case CalibrationRunnerResult.Unknown:
                    throw new ApplicationException(string.Format("Calibration stopped for an unknown reason. Message: '{0}'", calibrateArgs.Message));
            }

            return null;
        }

        public bool CanBeCompletedWithoutManualIntervention { get { return true; } }
    }
}
