using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using log4net;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class ObservableExtensions
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Prefix the sequence with an initial value of NULL, then publish points as long as the current point is within
        /// the PointTtl period (time to live). If no point is currently 'live' then the sequence reverts to NULL.
        /// </summary>
        public static IObservable<Timestamped<Point?>> PublishLivePointsOnly(
            this IObservable<Timestamped<Point>> source, TimeSpan pointTtl)
        {
            var usefulSource = source
                .FilterOutStaleOnArrival(pointTtl)
                .Select(x => new Timestamped<Point?>(x.Value, x.Timestamp));

            var staleSource = usefulSource
                .Throttle(tp =>
                {
                    var ageOfPoint = DateTimeOffset.Now.ToUniversalTime().Subtract(tp.Timestamp.ToUniversalTime());
                    var remainingUntilStale = pointTtl.Subtract(ageOfPoint);
                    return Observable.Timer(remainingUntilStale).Select(_ => tp);
                })
                .Select(tp =>
                {
                    Log.Debug("There is no currently 'live' point - pushing NULL to points sequence.");
                    return new Timestamped<Point?>(null, DateTimeOffset.Now.ToUniversalTime());
                });

            return usefulSource
                .Merge(staleSource)
                .StartWith(new Timestamped<Point?>(null, DateTimeOffset.Now.ToUniversalTime()));
        }

        public static IObservable<Timestamped<Point>> FilterOutStaleOnArrival(
            this IObservable<Timestamped<Point>> source, TimeSpan pointTtl)
        {
            return source.Where(tp =>
            {
                //Log and filter out points which are stale on arrival
                var ageOfPoint = DateTimeOffset.Now.ToUniversalTime().Subtract(tp.Timestamp.ToUniversalTime());
                var remainingUntilStale = pointTtl.Subtract(ageOfPoint);

                if (remainingUntilStale.Ticks <= 0)
                {
                    Log.Debug(string.Format(
                        "Point received which was stale on arrival. Timestamp of point:{0}ms old (TTL is {1}ms). Discarding it.",
                        ageOfPoint.TotalMilliseconds, pointTtl.TotalMilliseconds));

                    return false;
                }

                return true;
            });
        }

        public static void Dump<T>(this IObservable<T> source, string name)
        {
            source.Subscribe(
            i => Console.WriteLine("{0}-->{1}", name, i),
            ex => Console.WriteLine("{0} failed-->{1}", name, ex.Message),
            () => Console.WriteLine("{0} completed", name));
        }
    }
}