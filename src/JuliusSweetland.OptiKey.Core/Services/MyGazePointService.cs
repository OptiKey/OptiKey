// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive;
using System.Windows;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using MyGazeNative = JuliusSweetland.OptiKey.Native.MyGaze;

namespace JuliusSweetland.OptiKey.Services
{
    public class MyGazePointService : IPointService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private event EventHandler<Timestamped<Point>> pointEvent;

        private readonly SampleCallbackHandler sampleCallbackHandler; //Must be a member variable to prevent garbage collection
        private delegate void SampleCallbackHandler(MyGazeNative.Structs.SampleStruct sampleData);
        private readonly EventCallbackHandler eventCallbackHandler; //Must be a member variable to prevent garbage collection
        private delegate void EventCallbackHandler(MyGazeNative.Structs.EventStruct eventData);

        #endregion

        #region Ctor

        public MyGazePointService()
        {
            KalmanFilterSupported = true;

            sampleCallbackHandler = SampleCallback;
            eventCallbackHandler = EventCallback;

            //Disconnect from the myGaze server on shutdown
            Application.Current.Exit += (sender, args) =>
            {
                Log.Info("Disconnecting from the myGaze server.");
                MyGazeNative.PInvoke.Disconnect();
            };
        }

        #endregion

        #region Properties

        public bool KalmanFilterSupported { get; private set; }

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        public event EventHandler<Timestamped<Point>> Point
        {
            add
            {
                if (pointEvent == null)
                {
                    Log.Info("Point event has first subscriber.");

                    //Connect to the myGaze server
                    Log.Info("Attempting to connect to the myGaze server...");
                    if (MyGazeNative.PInvoke.Connect() == 1)
                    {
                        Log.Info("...connection was successful.");
                        Log.Info("Attempting to get info about the myGaze API and server...");
                        MyGazeNative.Structs.SystemInfoStruct systemInfo = new MyGazeNative.Structs.SystemInfoStruct();
                        if (MyGazeNative.PInvoke.GetSystemInfo(ref systemInfo) == 1)
                        {
                            Log.Info("...attempt to get info was successful.");
                            Log.InfoFormat("myGaze API version:{0}.{1}.{2}", systemInfo.API_MajorVersion, systemInfo.API_MinorVersion, systemInfo.API_Buildnumber);
                            Log.InfoFormat("myGaze Server version:{0}.{1}.{2}", systemInfo.iV_MajorVersion, systemInfo.iV_MinorVersion, systemInfo.iV_Buildnumber);
                        }
                        else
                        {
                            Log.Warn("...attempt to get info was unsuccessful.");
                        }

                        Log.Info("Attempting to add a sample callback listener to the myGaze server.");
                        MyGazeNative.PInvoke.SetSampleCallback(sampleCallbackHandler);
                        Log.Info("...complete.");

                        //Log.Info("Attempting to add an event callback listener to the myGaze server.");
                        //MyGazeNative.PInvoke.SetEventCallback(eventCallbackHandler);
                        //Log.Info("...complete.");
                    }
                    else
                    {
                        PublishError(this, new ApplicationException(Resources.MY_GAZE_CONNECT_ERROR));
                    }
                }

                pointEvent += value;
            }
            remove
            {
                pointEvent -= value;

                if (pointEvent == null)
                {
                    Log.Info("Last listener of Point event has unsubscribed. Disconnecting from server...");
                    var success = MyGazeNative.PInvoke.Disconnect() == 1;
                    Log.InfoFormat("...disconnect {0}", success ? "was successful." : "failed!");
                }
            }
        }

        #endregion

        #region Callback Methods

        private void SampleCallback(MyGazeNative.Structs.SampleStruct sampleData)
        {
            if (pointEvent != null)
            {
                //Take left OR right eye screen co-ordinates (left takes precedence if present)
                var leftX = sampleData.leftEye.gazeX;
                var leftY = sampleData.leftEye.gazeY;
                var rightX = sampleData.rightEye.gazeX;
                var rightY = sampleData.rightEye.gazeY;

                double? x = null;
                double? y = null;

                if (!double.IsNaN(leftX) && leftX > 0
                    && !double.IsNaN(leftY) && leftY > 0)
                {
                    x = leftX;
                    y = leftY;
                }
                else if (!double.IsNaN(rightX) && rightX > 0
                    && !double.IsNaN(rightY) && rightY > 0)
                {
                    x = rightX;
                    y = rightY;
                }

                if (x != null && y != null)
                {
                    pointEvent(this, new Timestamped<Point>(new Point(x.Value, y.Value),
                        new DateTimeOffset(DateTime.UtcNow).ToUniversalTime())); //Sample timestamp is not useful
                        //new DateTimeOffset(new DateTime(sampleData.timestamp)).ToUniversalTime()));
                }
            }
        }

        private void EventCallback(MyGazeNative.Structs.EventStruct eventData)
        {
            if (pointEvent != null
                && !double.IsNaN(eventData.positionX)
                && eventData.positionX > 0
                && !double.IsNaN(eventData.positionY)
                && eventData.positionY > 0)
            {
                pointEvent(this, new Timestamped<Point>(
                    new Point(eventData.positionX, eventData.positionY),
                    new DateTimeOffset(DateTime.UtcNow).ToUniversalTime())); //Event data does not include a useable timestamp as it would be meaningless
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