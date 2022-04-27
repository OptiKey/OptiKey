// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using JuliusSweetland.OptiKey.Properties;
using MyGazeNative = JuliusSweetland.OptiKey.Native.MyGaze;

namespace JuliusSweetland.OptiKey.Services
{
    public class MyGazeCalibrationService : ICalibrationService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public async Task<string> Calibrate(Window parentWindow)
        {
            Log.Info("Attempting to calibrate using MyGaze native calls.");

            var result = await Task.Run(() => MyGazeNative.PInvoke.Calibrate());

            if (result == 1)
            {
                var message = Resources.CALIBRATION_SUCCESS;
                Log.Info(message);
                return message;
            }

            throw new ApplicationException(Resources.CALIBRATION_FAIL_NO_MESSAGE);
        }

        public bool CanBeCompletedWithoutManualIntervention { get { return true; } }
    }
}
