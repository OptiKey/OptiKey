using System;
using System.Windows;
using Tobii.StreamEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using JuliusSweetland.OptiKey.Static;

namespace TobiiConsole
{
    public class TobiiPointService
    {
        

        private IntPtr apiContext;
        public IntPtr deviceContext;

        private BackgroundWorker pollWorker;
        private int pollDelayMs = 20;

        public void GetPoints(int n)
        {
            tobii_error_t result;
            for (int i = 0; i < n; i++)
            {
                // Optionally block this thread until data is available. Especially useful if running in a separate thread.
                result = Interop.tobii_wait_for_callbacks(new[] { deviceContext });
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR || result == tobii_error_t.TOBII_ERROR_TIMED_OUT);

                // Process callbacks on this thread if data is available
                result = Interop.tobii_device_process_callbacks(deviceContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
            }
        }

        private DateTimeOffset? startTimeCoarse;
        private DateTimeOffset? startTimeHost;
        private long? startTimeTobii;

        private tobii_gaze_point_callback_t GazeCallback;

        public void Subscribe()
        {
            // Subscribe to gaze data
            GazeCallback = (ref tobii_gaze_point_t gazePoint, IntPtr userData) =>
            {
                if (gazePoint.validity == tobii_validity_t.TOBII_VALIDITY_VALID)
                {
                    if (!startTimeCoarse.HasValue)
                    {
                        startTimeCoarse = Time.HighResolutionUtcNow.ToUniversalTime();
                    }
                    if (!startTimeHost.HasValue)
                    {
                        startTimeHost = DateTime.UtcNow.ToUniversalTime();
                    }
                    if (!startTimeTobii.HasValue)
                    {
                        startTimeTobii = gazePoint.timestamp_us;
                    }

                    // Low resolution time
                    var timeStampCoarse = new DateTimeOffset(DateTime.UtcNow.ToUniversalTime());
                    TimeSpan spanCoarse = timeStampCoarse.Subtract(startTimeCoarse.Value);
                    long usCoarse = (long)spanCoarse.TotalMilliseconds * 1000;

                    // Current system time relative to start
                    var timeStampHost = new DateTimeOffset(Time.HighResolutionUtcNow.ToUniversalTime());
                    TimeSpan spanHost = timeStampHost.Subtract(startTimeHost.Value);
                    long usHost = (long)spanHost.TotalMilliseconds * 1000;

                    // Tobii time relative to start
                    var timeStampTobii = gazePoint.timestamp_us;
                    long usTobii = timeStampTobii - startTimeTobii.Value;
                    long delta = usCoarse - usTobii ;

                    Console.WriteLine($"{usTobii} {usHost} {usCoarse} {usCoarse / 1000000} {delta}");

                    //Console.WriteLine($"Gaze point: {gazePoint.position.x}, {gazePoint.position.y}\t time: {gazePoint.timestamp_us}");
                }
            };
            tobii_error_t result = Interop.tobii_gaze_point_subscribe(deviceContext, GazeCallback);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
        }

        public void ConnectAPI()
        {
            // Create API context            
            tobii_error_t result = Interop.tobii_api_create(out apiContext, null);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
        }

        public void Dispose()
        {
            pollWorker.CancelAsync();
            pollWorker.Dispose();
        }

        public void pollTobii(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (pollWorker.CancellationPending) { return; }
                if (apiContext == IntPtr.Zero) { return; }
                if (deviceContext == IntPtr.Zero) { return; }
                
                // FIXME: can we just always call reconnect here? is it a no-op if connected?

                // Puts the calling thread to sleep until there are new callbacks available to process from one or multiple device connections.
                // Can return "timed out" so should be okay
                tobii_error_t result = Interop.tobii_wait_for_callbacks(new[] { deviceContext });
                if (result == tobii_error_t.TOBII_ERROR_NO_ERROR)
                {
                    Interop.tobii_device_process_callbacks(deviceContext);
                }
                Thread.Sleep(pollDelayMs);
            }
        }

        public void CleanupAPI()
        {
            tobii_error_t result;
            if (deviceContext != IntPtr.Zero)
            {
                // Cleanup
                result = Interop.tobii_gaze_point_unsubscribe(deviceContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
                result = Interop.tobii_device_destroy(deviceContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
                deviceContext = IntPtr.Zero;
            }
            if (apiContext != IntPtr.Zero)
            {
                result = Interop.tobii_api_destroy(apiContext);
                Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
                apiContext = IntPtr.Zero;
            }
        }


        public void ConnectTracker()
        {
            // Enumerate devices to find connected eye trackers
            List<string> urls;
            tobii_error_t result = Interop.tobii_enumerate_local_device_urls(apiContext, out urls);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

            if (urls.Count == 0)
            {
                // FIXME: toast?
                Console.WriteLine("Error: No device found");
                return;
            }

            // Connect to first tracker
            result = Interop.tobii_device_create(apiContext, urls[0], Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, out deviceContext);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);


            /*IntPtr tempDeviceContext;
            foreach (string url in urls)
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



        public TobiiPointService()
        {
            //ConnectAPI();
            //ConnectTracker();


            //pollWorker = new BackgroundWorker();
            //pollWorker.DoWork += pollTobii;
            //pollWorker.WorkerSupportsCancellation = true;

            //// This sample will collect 1000 gaze points
            //for (int i = 0; i < 1000; i++)
            //{
            //    // Optionally block this thread until data is available. Especially useful if running in a separate thread.
            //    Interop.tobii_wait_for_callbacks(new[] { deviceContext });
            //    Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR || result == tobii_error_t.TOBII_ERROR_TIMED_OUT);

            //    // Process callbacks on this thread if data is available
            //    Interop.tobii_device_process_callbacks(deviceContext);
            //    Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
            //}

            //Disconnect on shutdown 
            //Application.Current.Exit += (sender, args) => CleanupAPI();
        }
    }
}
