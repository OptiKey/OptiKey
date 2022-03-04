// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Threading.Tasks;
using System.Windows;
using EyeXFramework;
using log4net;
using Tobii.EyeX.Framework;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Services
{
    public class TobiiEyeXCalibrationService : ICalibrationService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public EyeXHost EyeXHost { get; set; }

        public async Task<string> Calibrate(Window parentWindow)
        {
            Log.Info("Attempting to calibrate using the Tobii EyeX engine.");

            var taskCompletionSource = new TaskCompletionSource<string>(); //Used to make this method awaitable on the InteractionRequest callback

            if(EyeXHost != null)
            {
                EyeXHost.LaunchRecalibration();

                EventHandler<EngineStateValue<EyeTrackingDeviceStatus>> handler = null;
                handler = (s, e) =>
                {
                    if (e.Value == EyeTrackingDeviceStatus.Tracking)
                    {
                        Log.Info("Calibration ended.");
                        EyeXHost.EyeTrackingDeviceStatusChanged -= handler;
                        taskCompletionSource.SetResult(Resources.TOBII_EYEX_CALIBRATION_SUCCESS);
                    }
                };
                EyeXHost.EyeTrackingDeviceStatusChanged += handler;
            }
            else
            {
                throw new ApplicationException(Resources.THE_EYE_TRIBE_UNABLE_TO_CALIBRATE_NO_ENGINE);
            }

            return await taskCompletionSource.Task;
        }

        public bool CanBeCompletedWithoutManualIntervention { get { return true; } }
    }
}
