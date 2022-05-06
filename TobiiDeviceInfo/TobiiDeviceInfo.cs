using System;
using System.Windows;
using Tobii.StreamEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace TobiiDeviceInfo
{
    public class TobiiDeviceInfo
    {
        
        private IntPtr apiContext;
        public IntPtr deviceContext;

        public TobiiDeviceInfo()
        {
        }

        public void ConnectAPI()
        {
            // Create API context            
            tobii_error_t result = Interop.tobii_api_create(out apiContext, null);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);
        }


        public void ConnectTracker()
        {
            // Enumerate devices to find connected eye trackers
            List<string> urls;
            tobii_error_t result = Interop.tobii_enumerate_local_device_urls(apiContext, out urls);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

            if (urls.Count == 0)
            {
                Console.WriteLine("Error: No device found");
                return;
            }

            // Connect to first tracker
            result = Interop.tobii_device_create(apiContext, urls[0], Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, out deviceContext);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

            tobii_device_info_t info;
            Interop.tobii_get_device_info(deviceContext, out info);
            Console.WriteLine($"model: {info.model}");
            Console.WriteLine($"generation: {info.generation}");
            Console.WriteLine($"serial_number: {info.serial_number}");
            Console.WriteLine($"firmware_version: {info.firmware_version}");
            Console.WriteLine($"runtime_build_version: {info.runtime_build_version}");
            Console.WriteLine("");
            Console.WriteLine($"integration_type: {info.integration_type}");
            Console.WriteLine($"integration_id: {info.integration_id}");
            Console.WriteLine($"hw_calibration_date: {info.hw_calibration_date}");
            Console.WriteLine($"hw_calibration_version: {info.hw_calibration_version}");
            Console.WriteLine($"lot_id: {info.lot_id}");
            
        }

        public void Dispose()
        {
            CleanupAPI();
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

                     
    }
}
