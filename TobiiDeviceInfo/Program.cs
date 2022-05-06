// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using Tobii.StreamEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using TobiiDeviceInfo;

namespace TobiiConsole
{
    class Program
    {

        private static TobiiDeviceInfo.TobiiDeviceInfo tobii;

        static void Main(string[] args)
        {

            tobii = new TobiiDeviceInfo.TobiiDeviceInfo();            

            tobii.ConnectAPI();
            tobii.ConnectTracker();

            Console.ReadLine();

        }


    }
}
