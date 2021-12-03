// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System;
using System.Runtime.InteropServices;
using JuliusSweetland.OptiKey.Native.Irisbond.Duo.Enums;

namespace JuliusSweetland.OptiKey.Native.Irisbond.Duo
{
    public static class PInvoke
    {
        #region Delegates

        // Callback received when a new sample is acquired.
        // Contains the timestamp, filtered and unfiltered POG, detections, eyes position and image resolution
        // 
        // distanceFactor: 0 means the user is at the best distance from the tracker
        //                 1 means the user is too close, the image is distorted beyond this point
        //                 -1 means the user is too far, the image is distorted beyond this point
        //                 the distance should be between -0.5 and 0.5. although it will work for other values
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DATA_CALLBACK(long timestamp,
                                            float pogX, float pogY,
                                            float pogRawX, float pogRawY,
                                            int screenWidth, int screenHeight,
                                            [MarshalAs(UnmanagedType.U1)] bool detectedL, [MarshalAs(UnmanagedType.U1)] bool detectedR,
                                            int resWidth, int resHeight,
                                            float leftEyeX, float leftEyeY, float leftEyeSize,
                                            float rightEyeX, float rightEyeY, float rightEyeSize,
                                            float distanceFactor);

        // Callback received after calibration.
        // Contains the precision and accuracy for each eye and combined.
        // Tells if the calibration was cancelled or not
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CALIBRATION_RESULTS_CALLBACK(double leftPrecisionError, double leftAccuracyError,
                                                            double rightPrecisionError, double rightAccuracyError,
                                                            double combinedPrecisionError, double combinedAccuracyError,
                                                            [MarshalAs(UnmanagedType.U1)] bool cancelled);

        // Callback received after calibration.
        // Contains the normalized error of each calibration point, as well as its location on the screen (normalized)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CALIBRATION_RESULTS_POINTS_CALLBACK(int nPoints, IntPtr xCoords, IntPtr yCoords,
                                                                 IntPtr leftErrorsPx, IntPtr rightErrorsPx, IntPtr combinedErrorsPx,
                                                                 IntPtr leftErrorsNorm, IntPtr rightErrorsNorm, IntPtr combinedErrorsNorm);

        // Callback received when calibration is cancelled.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CALIBRATION_CANCELLED_CALLBACK();

        // Callback received with the position of the calibration target on screen
        // Contains the position of the target final position and indicates if the target is fixated or travelling
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CALIBRATION_TARGET_CALLBACK(double pointX, double pointY, int screenWidth, int screenHeight, [MarshalAs(UnmanagedType.U1)] bool fixated);

        // Callback received during calibration when user is not found.
        // The callback must return true to retry, and false to cancel.
        // If the callback is not set, it will retry automatically
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool CALIBRATION_POINT_ERROR_CALLBACK();

        // Callback received with image from the camera
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IMAGE_CALLBACK(IntPtr data, int rows, int cols, int channels, long timestamp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BLINK_CALLBACK(int x, int y, int screenWidth, int screenHeight);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DWELL_CALLBACK(int x, int y, int screenWidth, int screenHeight);

        #endregion
        
        #region Functions 

        // Check if tracker is present
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool trackerIsPresent();

        // Get hardware number
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr getHardwareNumber();
        public static string getHWNumber()
        {
            string s = Marshal.PtrToStringAnsi(getHardwareNumber());
            return s;
        }

        // Set the license
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setLicense(string license);

        // Start the library: Launch the processing frame and initialize the camera
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern START_STATUS start();

        // Stop the library
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void stop();

        // Check if application is ended
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool isApplicationEnded();

        // Switch on the led lights, when the API is started
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool switchOnLedLights();

        // Switch off the led lights, when the API is started
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool switchOffLedLights();

        // Check if led lights are on, when the API is started
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool areLedLightsOn();
        
        // Shows positioning windows.
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void showPositioningWindow();

        // Hides positioning windows.
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hidePositioningWindow();

        // Get if user is calibrated.
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool isUserCalibrated(int userID);

        // Start the calibration process
        // param numCalibPoints [in] Number of calibration points (5,9,16)
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void startCalibration(int numCalibPoints);

        // Cancels the calibration
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cancelCalibration();

        // Starts the calibration improve
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void startImproveCalibration();

        // Starts the calibration rectification (1 point)
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void startCalibrationRectification();

        // Wait for the calibration to end
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern CALIBRATION_STATUS waitForCalibrationToEnd(int timeoutInMinutes);

        // Sets the calibration target parameters: travel time and fixation time
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCalibrationParameters(double travelTime, double fixationTime);

        // Enables or disables the default calibration GUI
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void showCalibrationGUI(bool show);

        // Enables or disables the default point error GUI
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void showCalibrationPointErrorGUI(bool show);

        // Enables or disables the default calibration results GUI
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void showCalibrationResultsGUI(bool show);
        
        // Sets the controlling eye
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool setUserEyeControlMode(CONTROLLING_EYE controlMode);

        // Sets the smoothing value of the POG filter
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setSmoothValue(int smooth);

        // Sets the blink configuration
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setBlinkConfiguration(double singleTime, double cancelTime, bool bothEyesRequired);

        // Sets the dwell configuration
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setDwellConfiguration(int areaPixels, double time, bool bothEyesRequired);

        // Sets the data callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setDataCallback(DATA_CALLBACK theCallback);

        // Sets the calibration results callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCalibrationResultsCallback(CALIBRATION_RESULTS_CALLBACK theCallback);

        // Sets the detailed calibration results callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCalibrationResultsPointsCallback(CALIBRATION_RESULTS_POINTS_CALLBACK theCallback);

        // Sets the calibration cancelled callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCalibrationCancelledCallback(CALIBRATION_CANCELLED_CALLBACK theCallback);

        // Sets the calibration target callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCalibrationTargetCallback(CALIBRATION_TARGET_CALLBACK theCallback);

        // Sets the calibration point error callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCalibrationPointErrorCallback(CALIBRATION_POINT_ERROR_CALLBACK theCallback);

        // Sets the camera image callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setImageCallback(IMAGE_CALLBACK theCallback);

        // Sets the blink callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setBlinkCallback(BLINK_CALLBACK theCallback);

        // Sets the dwell callback
        [DllImport("IrisbondAPI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setDwellCallback(DWELL_CALLBACK theCallback);

        #endregion
    }
}
