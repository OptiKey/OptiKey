using System;
using JuliusSweetland.ETTA.Properties;
using log4net;
using TETControls.Calibration;

namespace JuliusSweetland.ETTA.Services
{
    public class TheEyeTribeCalibrationService : ICalibrateService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Events

        public event EventHandler<string> Info;
        public event EventHandler<Exception> Error;
        
        #endregion

        #region Calibrate

        public void Calibrate(int retryNumber, Action onResultAction)
        {
            bool executeOnResultAction = true;

            var calRunner = new CalibrationRunner();
            
            calRunner.OnResult += (sender, args) =>
            {
                ApplicationException calibrationException = null;
                var retry = retryNumber + 1 < Settings.Default.CalibrationAutoRetryCount;

                switch (args.Result)
                {
                    case CalibrationRunnerResult.Success:
                        PublishInfo(this, 
                            string.Format("Calibration success! Accuracy (Avg Error Degree = {0})", 
                                args.CalibrationResult.AverageErrorDegree));
                        break;

                    case CalibrationRunnerResult.Abort:
                        calibrationException = new ApplicationException(
                            string.Format("Calibration aborted with message: '{0}'{1}", 
                                args.Message, retry ? " - retrying..." : null));
                        break;

                    case CalibrationRunnerResult.Error:
                        calibrationException = new ApplicationException(
                            string.Format("An error occurred during calibration. Message: '{0}'{1}", 
                                args.Message, retry ? " - retrying..." : null));
                        break;

                    case CalibrationRunnerResult.Failure:
                        calibrationException = new ApplicationException(
                            string.Format("Calibration failed with message: '{0}'{1}", 
                                args.Message, retry ? " - retrying..." : null));
                        break;

                    case CalibrationRunnerResult.Unknown:
                        calibrationException = new ApplicationException(
                            string.Format("Calibration stopped for an unknown reason. Message: '{0}'{1}", 
                                args.Message, retry ? " - retrying..." : null));
                        break;
                }

                if (calibrationException != null)
                {
                    PublishError(this, calibrationException);

                    if (retry)
                    {
                        executeOnResultAction = false;
                        Calibrate(retryNumber + 1, onResultAction);
                    }
                }

                if (executeOnResultAction)
                {
                    onResultAction();
                }
            };

            calRunner.Start();
        }

        #endregion

        #region Publish Info

        private void PublishInfo(object sender, string info)
        {
            if (Info != null)
            {
                Log.Debug(string.Format("Publishing Info event: '{0}'", info));

                Info(sender, info);
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            if (Error != null)
            {
                Log.Error("Publishing Error event", ex);

                Error(sender, ex);
            }
        }

        #endregion
    }
}
