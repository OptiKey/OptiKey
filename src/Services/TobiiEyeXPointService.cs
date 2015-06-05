using System;
using System.Reactive;
using System.Windows;
using EyeXFramework;
using log4net;
using Tobii.EyeX.Client;
using Tobii.EyeX.Framework;

namespace JuliusSweetland.OptiKey.Services
{
    public class TobiiEyeXPointService : IPointService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EyeXHost eyeXHost;

        private GazePointDataStream gazeDataStream;

        private event EventHandler<Timestamped<Point>> pointEvent;

        #endregion

        #region Ctor

        public TobiiEyeXPointService()
        {
            eyeXHost = new EyeXHost();

            //Disconnect (deactivate) from the TET server on shutdown - otherwise the process can hang
            Application.Current.Exit += (sender, args) =>
            {
                if (eyeXHost != null)
                {
                    Log.Debug("Disposing of the EyeXHost.");
                    eyeXHost.Dispose();
                }
            };
        }

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        public event EventHandler<Timestamped<Point>> Point
        {
            add
            {
                if (pointEvent == null)
                {
                    Log.Debug("Checking the state of the Tobii service...");

                    switch (EyeXHost.EyeXAvailability)
                    {
                        case EyeXAvailability.NotAvailable:
                            PublishError(this, new ApplicationException("Tobii EyeX Engine cannot be found. Please install the EyeX Engine and try again."));
                            return;

                        case EyeXAvailability.NotRunning:
                            PublishError(this, new ApplicationException("Tobii EyeX Engine is not running. Please start the EyeX Engine and try again."));
                            return;
                    }
                    
                    Log.Debug("Attaching eye tracking device status changed listener to the Tobii service.");
                    eyeXHost.EyeTrackingDeviceStatusChanged += (s, e) => Log.DebugFormat("Tobii EyeX tracking device status changed to {0}", e);

                    gazeDataStream = eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);

                    eyeXHost.Start(); // Start the EyeX host

                    gazeDataStream.Next += (s, data) =>
                    {
                        if (pointEvent != null)
                        {
                            pointEvent(this, new Timestamped<Point>(new Point(data.X, data.Y), 
                                new DateTimeOffset(DateTime.UtcNow).ToUniversalTime())); //EyeX does not publish a useable timestamp
                        }
                    };
                }

                pointEvent += value;
            }
            remove
            {
                pointEvent -= value;

                if (pointEvent == null)
                {
                    Log.Info("Last listener of Point event has unsubscribed. Disposing gaze data stream.");
                    gazeDataStream.Dispose();
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