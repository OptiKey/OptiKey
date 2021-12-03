// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using log4net;
using JuliusSweetland.OptiKey.Native.Irisbond;
using JuliusSweetland.OptiKey.Native.Irisbond.Duo;
using JuliusSweetland.OptiKey.Native.Irisbond.Duo.Enums;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Services
{
    public class IrisbondDuoPointService : IPointService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private PInvoke.DATA_CALLBACK onDataCallback;

        private event EventHandler<Timestamped<Point>> pointEvent;

        #endregion

        #region Ctor

        public IrisbondDuoPointService()
        {
            KalmanFilterSupported = true;
        }

        #endregion

        #region Properties

        public bool KalmanFilterSupported {get; private set; }

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        public event EventHandler<Timestamped<Point>> Point
        {
            add
            {
                if (pointEvent == null)
                {
                    Log.Info("Checking the Irisbond Duo tracker is connected...");

                    bool trackerConnected = PInvoke.trackerIsPresent();
                    if (!trackerConnected)
                    {
                        PublishError(this, new ApplicationException(Resources.IRISBOND_TRACKER_NOT_FOUND));
                        return;
                    }
                    Log.Info("Tracker is connected. Attempting to start the tracker/engine.");
                    START_STATUS status = PInvoke.start();
                    Log.InfoFormat("Status is '{0}'", status.ToString());

                    switch (status)
                    {
                        case START_STATUS.START_OK:
                            Log.Info("Tracker started.");
                            break;
                            
                        default:
                            Log.Info("Unable to start the tracker. Ensuring the tracker is stopped...");
                            PInvoke.stop();
                            Log.Info("Tracker stopped.");
                            PublishError(this, new ApplicationException(Resources.IRISBOND_TRACKER_COULD_NOT_BE_STARTED));
                            return;
                    }

                    onDataCallback = OnData;
                    PInvoke.setDataCallback(onDataCallback);
                }

                pointEvent += value;
            }
            remove
            {
                pointEvent -= value;

                if (pointEvent == null)
                {
                    Log.Info("Last listener of Point event has unsubscribed. Freeing callbacks.");

                    onDataCallback = null;
                }
            }
        }

        #endregion

        #region Callbacks

        public void OnData(long timestamp,
                                     float pogX, float pogY,
                                     float pogRawX, float pogRawY,
                                     int screenWidth, int screenHeight,
                                     bool detectedL, bool detectedR,
                                     int resWidth, int resHeight,
                                     float leftEyeX, float leftEyeY, float leftEyeSize,
                                     float rightEyeX, float rightEyeY, float rightEyeSize,
                                     float distanceFactor)
        {
            if (pointEvent != null)
            {
                float x = Settings.Default.IrisbondProcessingLevel > DataStreamProcessingLevels.None ? pogX : pogRawX;
                float y = Settings.Default.IrisbondProcessingLevel > DataStreamProcessingLevels.None ? pogY : pogRawY;

                if(!double.IsNaN(x)
                   && !double.IsNaN(y))
                {
                    pointEvent(this, new Timestamped<Point>(new Point(pogX, pogY),
                        new DateTimeOffset(DateTime.UtcNow).ToUniversalTime())); //IrisbondDuo does not publish a useable timestamp
                }
            }
        }
        
        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion
    }
}