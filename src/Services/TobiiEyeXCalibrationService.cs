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
        
        public async Task<string> Calibrate(Window parentWindow)
        {
            var taskCompletionSource = new TaskCompletionSource<string>(); //Used to make this method awaitable on the InteractionRequest callback

            using (var eyeXHost = new EyeXHost())
            {
                eyeXHost.Start();
                eyeXHost.LaunchRecalibration();
                eyeXHost.EyeTrackingDeviceStatusChanged += (s, e) =>
                {
                    if (e.IsValid && e.Value == EyeTrackingDeviceStatus.Tracking)
                    {
                        taskCompletionSource.SetResult("Calibration success!");
                    }
                };

                return await taskCompletionSource.Task;
            }
        }
    }
}
