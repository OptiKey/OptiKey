using System;
using System.Reactive;
using System.Windows;
using log4net;
using TETCSharpClient;
using TETCSharpClient.Data;

namespace JuliusSweetland.ETTA.Services
{
    public class TheEyeTribePointService : ITheEyeTribePointService
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private event EventHandler<Timestamped<Point>> pointEvent;

        #endregion

        #region Ctor

        public TheEyeTribePointService()
        {
            GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);
        }

        #endregion

        #region Point Event

        public event EventHandler<Timestamped<Point>> Point
        {
            add
            {
                pointEvent += value;

                if (!GazeManager.Instance.IsActivated)
                {
                    Log.Error("TET server is not activated (not running)! Please start the TET server and try again! Attempting to connect anyway.");
                }

                if (!GazeManager.Instance.HasGazeListener(this))
                {
                    Log.Info("Point event has first subscriber. Connecting to server.");

                    if (GazeManager.Instance.IsActivated
                        && !GazeManager.Instance.IsCalibrated)
                    {
                        Log.Warn("TET has not been calibrated. No data will be received until calibration is completed.");
                    }

                    GazeManager.Instance.AddGazeListener(this);
                }
            }
            remove
            {
                pointEvent -= value;

                if (pointEvent == null)
                {
                    Log.Info("Last listener of Point event has unsubscribed. Disconnecting from server...");

                    var success = GazeManager.Instance.RemoveGazeListener(this);

                    Log.Info(string.Format("...disconnect {0}", success ? "was successful." : "failed!"));
                }
            }
        }

        #endregion

        #region On Gaze Update Event Handler

        public void OnGazeUpdate(GazeData data)
        {
            if (GazeManager.Instance.IsCalibrated
                && pointEvent != null)
            {
                pointEvent(this, new Timestamped<Point>(
                    new Point(data.SmoothedCoordinates.X, data.SmoothedCoordinates.Y),
                    new DateTimeOffset(DateTime.Parse(data.TimeStampString)).ToUniversalTime()));
            }
        }

        #endregion
    }
}
