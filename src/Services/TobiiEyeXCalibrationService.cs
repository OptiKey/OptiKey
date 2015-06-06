using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using EyeXFramework;
using JuliusSweetland.OptiKey.Properties;
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
                var timeoutSubscription = Observable.Interval(Settings.Default.CalibrationMaxDuration)
                    .Subscribe(_ => { throw new TimeoutException("Calibration attempt has exceeded the maximum duration"); });

                EyeXHost.LaunchRecalibration();
                EyeXHost.EyeTrackingDeviceStatusChanged += (s, e) =>
                {
                    if (e.IsValid && e.Value == EyeTrackingDeviceStatus.Tracking)
                    {
                        timeoutSubscription.Dispose();
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
    }
}
