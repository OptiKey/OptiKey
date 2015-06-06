using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using log4net;
using TETCSharpClient;
using TETCSharpClient.Data;

namespace JuliusSweetland.OptiKey.Observables.PointSources
{
    public class TheEyeTribePointSource : IPointSource, IGazeListener, INotifyErrors
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly TimeSpan pointTtl;

        private IObservable<Timestamped<PointAndKeyValue?>> sequence;

        private event EventHandler<Timestamped<Point>> point;

        #endregion

        #region Ctor

        public TheEyeTribePointSource(TimeSpan pointTtl)
        {
            this.pointTtl = pointTtl;

            //Disconnect (deactivate) from the TET server on shutdown - otherwise the process can hang
            Application.Current.Exit += (sender, args) =>
            {
                if (GazeManager.Instance.IsActivated)
                {
                    Log.Debug("Deactivating TheEyeTribe's GazeManager.");
                    GazeManager.Instance.Deactivate();
                }
            };
        }

        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Properties

        public RunningStates State { get; set; }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap { private get; set; }

        public IObservable<Timestamped<PointAndKeyValue?>> Sequence
        {
            get
            {
                if (sequence == null)
                {
                    sequence = Observable.Create<Timestamped<PointAndKeyValue?>>(observer =>
                    {
                        bool disposed = false;

                        ConnectToEyeTrackerService();

                        Action disposeAllSubscriptions = null;

                        var pointSubscription = Observable.FromEventPattern<Timestamped<Point>>(
                                eh => point += eh,
                                eh => point -= eh)
                            .Where(_ => disposed == false)
                            .Where(_ => State == RunningStates.Running)
                            .Select(ep => ep.EventArgs)
                            .PublishLivePointsOnly(pointTtl)
                            .Select(tp => new Timestamped<PointAndKeyValue?>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                            .Subscribe(observer.OnNext, ex =>
                            {
                                observer.OnError(ex);
                                disposeAllSubscriptions();
                            });

                        disposeAllSubscriptions = () =>
                        {
                            disposed = true;

                            if (pointSubscription != null)
                            {
                                pointSubscription.Dispose();
                                pointSubscription = null;
                            }

                            DisconnectFromEyeTrackerService();
                        };

                        return disposeAllSubscriptions;
                    })
                    .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                    .RefCount();
                }

                return sequence;
            }
        }

        #endregion

        #region On Gaze Update Event Handler

        public void OnGazeUpdate(GazeData data)
        {
            if (GazeManager.Instance.IsCalibrated
                && point != null)
            {
                point(this, new Timestamped<Point>(
                    new Point(data.SmoothedCoordinates.X, data.SmoothedCoordinates.Y),
                    new DateTimeOffset(DateTime.Parse(data.TimeStampString)).ToUniversalTime()));
            }
        }

        #endregion

        #region Connect/Disconnect To/From Eye Tracker Service

        private void ConnectToEyeTrackerService()
        {
            //Activate TET if required
            if (!GazeManager.Instance.IsActivated)
            {
                Log.Info("Attempting to connect to TheEyeTribe service...");
                var success = GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push); //Connect client
                if (success)
                {
                    Log.Info("...connection established.");
                }
                else
                {
                    PublishError(this, new ApplicationException("TheEyeTribe service cannot be reached. Please check that the service is running."));
                    return;
                }
            }
            else
            {
                Log.Info("Connection already established to TheEyeTribe service.");
            }

            //Add this class as a gaze listener for TET updates
            if (GazeManager.Instance.IsActivated
                && !GazeManager.Instance.HasGazeListener(this))
            {
                Log.Info("Subscribing to TheEyeTribe service gaze data.");
                GazeManager.Instance.AddGazeListener(this); //Register this class for updates
            }

            //Publish error if TET not calibrated
            if (GazeManager.Instance.IsActivated
                && !GazeManager.Instance.IsCalibrated)
            {
                Log.InfoFormat("TheEyeTribe server is reporting that it is not calibrated - automatically retrying for up to {0}ms",
                    Settings.Default.TETCalibrationCheckTimeSpan.TotalMilliseconds);

                var calibrated = false;
                var retryStart = DateTimeOffset.Now.ToUniversalTime();
                while (DateTimeOffset.Now.ToUniversalTime().Subtract(retryStart.ToUniversalTime()) < Settings.Default.TETCalibrationCheckTimeSpan)
                {
                    if (GazeManager.Instance.IsCalibrated)
                    {
                        Log.Info("TheEyeTribe server is now reporting that it is calibrated - moving on");
                        calibrated = true;
                        break;
                    }
                }

                if (!calibrated)
                {
                    PublishError(this, new ApplicationException("TheEyeTribe has not been calibrated. No data will be received until calibration is completed."));
                }
            }
        }

        private void DisconnectFromEyeTrackerService()
        {
            Log.Info("Attempting to unsubscribe from TheEyeTribe service gaze data...");
            var success = GazeManager.Instance.RemoveGazeListener(this); //Unregister this class for updates
            Log.InfoFormat("...unsubscribe {0}", success ? "was successful." : "failed!");
            Log.Info("Disconnecting from TheEyeTribe service...");
            GazeManager.Instance.Deactivate(); //Disconnect client
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
