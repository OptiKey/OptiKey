// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive;
using System.Windows;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

using JuliusSweetland.OptiKey.Static;
using JuliusSweetland.OptiKey.Properties;

using log4net;
using System.IO;

namespace JuliusSweetland.OptiKey.Services
{
    public class TobiiPointService : IPointService, IDisposable
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private IntPtr apiContext;
        private IntPtr deviceContext;            

        private event EventHandler<Timestamped<Point>> pointEvent;

        // Separate thread for polling the tobii
        private BackgroundWorker pollWorker;

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
                pointEvent += value;
                ConnectTracker();
            }
            remove
            {
                pointEvent -= value;                
            }
        }

        #endregion

        #region Private methods
        
        private static Mutex tobiiMutex = new Mutex();

        private void pollTobii(object sender, DoWorkEventArgs e)
        {
            
        }

        private void ConnectAPI()
        {
        }
            
        private void CleanupAPI()
        {
        }

        private void ConnectTracker()
        {            
            Log.Error($"Tobii eyetracker is currently not supported by Optikey 4.0+");
            PublishError(this, new ApplicationException(Resources.TOBII_NOT_SUPPORTED));
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
