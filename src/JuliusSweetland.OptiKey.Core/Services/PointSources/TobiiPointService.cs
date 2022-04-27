// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive;
using System.Windows;

using JuliusSweetland.OptiKey.Enums;
using log4net;
using Tobii.StreamEngine;
using JuliusSweetland.OptiKey.Static;

using JuliusSweetland.OptiKey.Properties;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace JuliusSweetland.OptiKey.Services
{
    public class TobiiPointService : IPointService, IDisposable
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private IntPtr apiContext;
        private IntPtr deviceContext;
        private tobii_error_t lastError;        

        private event EventHandler<Timestamped<Point>> pointEvent;

        // Separate thread for polling the tobii
        private BackgroundWorker pollWorker;

        // Callback for new points - we need to keep handle to this rather than having a member function 
        // which would be cast to a tobii_gaze_point_callback_t via a temporary variable that may be disposed
        private tobii_gaze_point_callback_t GazeCallback;

        #endregion

        #region Ctor

        public TobiiPointService()
        {
            KalmanFilterSupported = true;

            ConnectAPI();
            ConnectTracker();

            pollWorker = new BackgroundWorker();
            pollWorker.DoWork += pollTobii;
            pollWorker.WorkerSupportsCancellation = true;

            //Disconnect on shutdown 
            Application.Current.Exit += (sender, args) => CleanupAPI();
        }

        public void Dispose()
        {
            CleanupAPI();
            pollWorker.CancelAsync();
            pollWorker.Dispose();
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
                    Log.Info("Checking the state of the Tobii service...");

                    // FIXME: how do we detect whether running or not
                    // not sure we can - nothing fails if Tobii Experience is closed
                    // (except eye tracker not found)
                    if (apiContext == IntPtr.Zero)
                    {
                        PublishError(this, new ApplicationException(Resources.TOBII_EYEX_ENGINE_NOT_FOUND));
                        return;
                    }

                    if (deviceContext == IntPtr.Zero)
                    {
                        // FIXME: does this get polled often? If so have cooldown
                        ConnectTracker();
                        if (deviceContext == IntPtr.Zero)
                        {
                            // FIXME: "Tobii device not found. Check that Tobii Experience is running and tracker is plugged in"
                            PublishError(this, new ApplicationException(Resources.TOBII_EYEX_ENGINE_NOT_RUNNING));
                            return;
                        }
                    }

                    // Subscribe to gaze data
                    GazeCallback = (ref tobii_gaze_point_t gazePoint, IntPtr userData) =>
                    {
                        if (pointEvent != null &&
                            gazePoint.validity == tobii_validity_t.TOBII_VALIDITY_VALID)
                        {
                            // Re: Timestamps -
                            // Data is timestamped with the eye tracker clock, ticks relative to some arbitrary time. 
                            // Tobii API does provides some clock sync methods, but they are returning all zeros.
                            // The Tobii clock does seem to drift (5C, drifted ~5ms / 6 minutes of logging) so we need to 
                            // avoid the point TTL logic filtering out all data after a couple of hours of use. 
                            // We either need to use system time (low resolution, doesn't help us with upstream delays)
                            // or keep track of minimum clock delays over a local window
                            // We would do this by keeping track of delta between tobii clock and host clock each time, tracking over 
                            // a window of ~5 mins and using the minimum as our reference. You would discard all points with worse
                            // delays than the incoming points from our sliding 5-minute window, so you'd only be left with monotically-increasing
                            // delays in the buffer, and the minimum would be the oldest in the buffer. 
                            // Pseudo-code:
                            // - Get incoming data point and time delta
                            // - From the end of the buffer, pop off to the left (backwards) any data with larger delay (
                            // - Stop popping as soon as you find lower delay
                            // - Peek the left hand end (start) of the buffer to get the minimum delay
                            // - Pop off left hand end if older than 5 minuts
                            // - Use this delay to adjust Tobii-time to host-time.                            
                            // We would need to ensure that the system timestamp is stable (time can't flow backwards)

                            var timeStamp = Time.HighResolutionUtcNow.ToUniversalTime();                            

                            pointEvent(this, new Timestamped<Point>(
                                new Point(Graphics.PrimaryScreenWidthInPixels * gazePoint.position.x,
                                          Graphics.PrimaryScreenHeightInPixels * gazePoint.position.y),
                                       timeStamp));
                        }
                    };
                    
                    tobii_error_t result = Interop.tobii_gaze_point_subscribe(deviceContext, GazeCallback);
                    Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

                    // We'll need to be polling it
                    pollWorker.RunWorkerAsync();

                }

                pointEvent += value;
            }
            remove
            {
                pointEvent -= value;

                if (pointEvent == null)
                {
                    pollWorker.CancelAsync();

                    Log.Info("Last listener of Point event has unsubscribed. Unsubscribing from Tobii stream.");
                    tobii_error_t result = Interop.tobii_gaze_point_unsubscribe(deviceContext);
                }
            }
        }

        #endregion

        #region Private methods
        
        private static Mutex tobiiMutex = new Mutex();

        private void pollTobii(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (pollWorker.CancellationPending) { return; }
                if (apiContext == null) { return; }
                if (deviceContext == null) { return; }

                lock (this)
                {
                    // Puts the calling thread to sleep until there are new callbacks 
                    // available to process from one or multiple device connections.                
                    tobii_error_t result = Interop.tobii_wait_for_callbacks(new[] { deviceContext });

                    bool hasError = CheckResultForError(result);
                    if (!hasError && !pollWorker.CancellationPending)
                    {
                        // Process callbacks                    
                        result = Interop.tobii_device_process_callbacks(deviceContext);
                        hasError = CheckResultForError(result);
                    }

                    if (hasError)
                    {
                        // Sleep thread to avoid hot loop
                        int delay = 500; // ms
                        Thread.Sleep(delay);

                        // Attempt reconnect
                        Interop.tobii_device_reconnect(deviceContext);
                    }
                }
            }
        }

        private void ConnectAPI()
        {
            // Create API context            
            tobii_error_t result = Interop.tobii_api_create(out apiContext, null);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
        }

        private void ConnectTracker()
        {
            // Enumerate devices to find connected eye trackers
            List<string> urls;
            tobii_error_t result = Interop.tobii_enumerate_local_device_urls(apiContext, out urls);
            CheckResultForError(result);

            if (urls.Count == 0)
            {
                Log.ErrorFormat("No Tobii devices found");
                PublishError(this, new ApplicationException(Resources.TOBII_TRACKER_NOT_FOUND));
                return;
            }

            // Connect to first tracker
            result = Interop.tobii_device_create(apiContext, urls[0], Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, out deviceContext);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

            /*IntPtr tempDeviceContext;
            foreach(string url in urls)
            {
                result = Interop.tobii_device_create(apiContext, url, Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, out tempDeviceContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

                tobii_device_info_t info;
                Interop.tobii_get_device_info(tempDeviceContext, out info);
                Console.WriteLine(info.model);

                bool supported = true;
                if (supported)
                {
                    // Connect to first supported tracker
                    deviceContext = tempDeviceContext;
                    break;
                }
            }*/
        }

        private bool CheckResultForError(tobii_error_t error_t)
        {
            bool isError = error_t != tobii_error_t.TOBII_ERROR_NO_ERROR;
            if (isError)
            {
                // Log if it's a new error
                if (error_t != lastError)
                {
                    Log.ErrorFormat($"Tobii error code: {error_t}");
                    lastError = error_t;
                }
            }
            return isError;
        }

        private void CleanupAPI()
        {
            pollWorker.CancelAsync();

            lock (this) // ensure we don't clean up while waiting on new data
            {
                tobii_error_t result;
                if (deviceContext != IntPtr.Zero)
                {
                    // Cleanup
                    result = Interop.tobii_gaze_point_unsubscribe(deviceContext);
                    CheckResultForError(result);

                    result = Interop.tobii_device_destroy(deviceContext);
                    CheckResultForError(result);
                    deviceContext = IntPtr.Zero;
                }
                if (apiContext != IntPtr.Zero)
                {
                    result = Interop.tobii_api_destroy(apiContext);
                    CheckResultForError(result);
                    apiContext = IntPtr.Zero;
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
