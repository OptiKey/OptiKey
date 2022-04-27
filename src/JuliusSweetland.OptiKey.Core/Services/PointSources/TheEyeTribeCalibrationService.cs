// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using TETControls.Calibration;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Services
{
    public class TheEyeTribeCalibrationService : ICalibrationService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public async Task<string> Calibrate(Window parentWindow)
        {
            Log.Info("Attempting to calibrate using the TheEyeTribe calibration runner.");
            
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
                    var message = string.Format(Resources.CALIBRATION_SUCCESS_WITH_ACCURACY,
                        calibrateArgs.CalibrationResult.AverageErrorDegree);
                    Log.Info(message);
                    return message;

                case CalibrationRunnerResult.Abort:
                    throw new ApplicationException(string.Format(Resources.CALIBRATION_ABORT_MESSAGE, calibrateArgs.Message));

                case CalibrationRunnerResult.Error:
                    throw new ApplicationException(string.Format(Resources.CALIBRATION_ERROR_MESSAGE, calibrateArgs.Message));

                case CalibrationRunnerResult.Failure:
                    throw new ApplicationException(string.Format(Resources.CALIBRATION_FAIL_MESSAGE, calibrateArgs.Message));

                case CalibrationRunnerResult.Unknown:
                    throw new ApplicationException(string.Format(Resources.CALIBRATION_STOPPED_MESSAGE, calibrateArgs.Message));
            }

            return null;
        }

        public bool CanBeCompletedWithoutManualIntervention { get { return true; } }
    }
}
