// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using Tobii.StreamEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace TobiiConsole
{
    class Program
    {

        private static void OnGazePoint(ref tobii_gaze_point_t gazePoint, IntPtr userData)
        {
            if (
                gazePoint.validity == tobii_validity_t.TOBII_VALIDITY_VALID)
            {
                Console.WriteLine($"Gaze point: {gazePoint.position.x}, {gazePoint.position.y}\t time: {gazePoint.timestamp_us}");
            }
        }

        private static TobiiPointService tobii;

        private static BackgroundWorker worker;

        public static void pollTobii(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                tobii.GetPoints(1);
            }
        }

        static void Main(string[] args)
        {

            tobii = new TobiiPointService();            

            tobii.ConnectAPI();
            tobii.ConnectTracker();
            tobii.Subscribe();
            /*
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            tobii.GetPoints(10);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            Thread.Sleep(1000);

            tobii_timesync_data_t timesync;
            Interop.tobii_timesync(tobii.deviceContext, out timesync);
            Console.WriteLine($"timesync: {timesync.system_start_us}, {timesync.tracker_us}, {timesync.system_end_us}");

            Interop.tobii_update_timesync(tobii.deviceContext);
            Interop.tobii_timesync(tobii.deviceContext, out timesync);

            Console.WriteLine($"timesync: {timesync.system_start_us}, {timesync.tracker_us}, {timesync.system_end_us}");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            */
            //tobii.GetPoints(10);

            //long startTime = new DateTimeOffset(DateTime.UtcNow).ToUniversalTime();
            //Interop.tobii_update_timesync(deviceCon)
            //tobii.GetPoints(1000);

            worker = new BackgroundWorker();
            worker.DoWork += pollTobii;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync();

            Console.ReadLine();

        }


    }
}
