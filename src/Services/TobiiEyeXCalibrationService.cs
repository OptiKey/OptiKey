using System;
using System.Threading.Tasks;
using System.Windows;
using EyeXFramework;
using log4net;
using Tobii.EyeX.Framework;

namespace JuliusSweetland.OptiKey.Services
{
    public class TobiiEyeXCalibrationService : ICalibrationService
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public EyeXHost EyeXHost { get; set; }

        public async Task<string> Calibrate(Window parentWindow)
        {
            Log.Debug("Attempting to calibrate using the Tobii EyeX engine.");

            var taskCompletionSource = new TaskCompletionSource<string>(); //Used to make this method awaitable on the InteractionRequest callback

            if(EyeXHost != null)
            {
                EyeXHost.LaunchRecalibration();
                EyeXHost.EyeTrackingDeviceStatusChanged += (s, e) =>
                {
                    if (e.IsValid && e.Value == EyeTrackingDeviceStatus.Tracking)
                    {
                        taskCompletionSource.SetResult("Calibration success!");
                    }
                };
            }
            else
            {
                throw new ApplicationException("Unable to attempt a calibration as no EyeX engine is available.");
            }

            return await taskCompletionSource.Task;
        }

        public bool CanBeCompletedWithoutManualIntervention { get { return false; } }
    }
}
