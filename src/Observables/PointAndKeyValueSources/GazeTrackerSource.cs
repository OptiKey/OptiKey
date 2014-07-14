using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Model;
using log4net;

namespace JuliusSweetland.ETTA.Observables.PointAndKeyValueSources
{
    public class GazeTrackerSource : IPointAndKeyValueSource
    {
        #region Fields

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly TimeSpan pointTtl;
        private readonly int gazeTrackerUdpPort;
        private readonly Regex gazeTrackerRegex;

        private IObservable<Timestamped<PointAndKeyValue?>> sequence;

        #endregion

        #region Ctor

        public GazeTrackerSource(
            TimeSpan pointTtl,
            int gazeTrackerUdpPort,
            Regex gazeTrackerRegex)
        {
            this.pointTtl = pointTtl;
            this.gazeTrackerUdpPort = gazeTrackerUdpPort;
            this.gazeTrackerRegex = gazeTrackerRegex;
        }

        #endregion

        #region Properties

        public Dictionary<Rect, KeyValue> PointToKeyValueMap { private get; set; }

        /// <summary>
        /// DEFER: The factory method to receive a datagram is DEFERRED until this subscription comes along, 
        /// i.e. it is a Task<UdpReceiveResult> which would usually begin immediately - defer prevents this happening too early.
        /// (http://msdn.microsoft.com/en-us/library/hh229160(v=vs.103).aspx)
        /// 
        /// REPEAT: Keep resubscribing to the observable indefinitely (http://theburningmonk.com/2010/03/rx-framework-iobservable-repeat/)
        /// </summary>
        public IObservable<Timestamped<PointAndKeyValue?>> Sequence
        {
            get
            {
                if (sequence == null)
                { 
                    sequence = Observable.Using(() =>
                        {
                            var udpClient = new UdpClient();
                            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); //Allows multiple simultaneous subscriptions
                            udpClient.Client.Bind(new IPEndPoint(IPAddress.Loopback, gazeTrackerUdpPort));
                            return udpClient;
                        }, 
                        udpClient => Observable
                            .Defer(() => udpClient.ReceiveAsync().ToObservable())
                            .Repeat() 
                            .Select(udpReceiveResult =>
                            {
                                var receivedString = Encoding.ASCII.GetString(udpReceiveResult.Buffer, 0, udpReceiveResult.Buffer.Length);
                                var match = gazeTrackerRegex.Match(receivedString);
                                if (match.Success)
                                {
                                    var x = double.Parse(match.Groups["x"].Value);
                                    var y = double.Parse(match.Groups["y"].Value);
                                    var timeStamp = new DateTimeOffset(
                                        long.Parse(match.Groups["instanceTime"].Value) * TimeSpan.TicksPerMillisecond, 
                                        new TimeSpan(DateTimeOffset.Now.Offset.Ticks)).ToUniversalTime();

                                    return new Timestamped<Point>(new Point(x, y), timeStamp);
                                }

                                Log.Warn(string.Format("Unable to parse received string '{0}' using Regex '{1}'. Ignore if GazeTracker is reporting a calibration.", receivedString, gazeTrackerRegex));
                                
                                return new Timestamped<Point>(new Point(0,0), new DateTimeOffset()); //Return useless point which will be filtered out
                            })
                            .Where(timestampedPoint => (timestampedPoint.Value.X != 0 || timestampedPoint.Value.Y != 0)) //(0,0) coordinates indicate that GT hasn't been calibrated, or regex failed to parse datagram - suppress
                            .DistinctUntilChanged(timestampedPoint => timestampedPoint.Value) //When GT loses the user's eyes it repeats the last value - suppress
                            .PublishLivePointsOnly(pointTtl)
                            .DistinctUntilChanged(timestampedPoint => timestampedPoint.Value) //Suppress repeating values, such as nulls when stale feed
                            .Select(tp => new Timestamped<PointAndKeyValue?>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp)))
                        .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                        .RefCount();
                }

                return sequence;
            }
        }

        #endregion
    }
}