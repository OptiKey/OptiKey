// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using log4net;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class ObservableExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Prefix the sequence with an initial value of NULL, then publish points as long as the current point is within
        /// the PointTtl period (time to live). If no point is currently 'live' then the sequence reverts to NULL.
        /// </summary>
        public static IObservable<Timestamped<Point?>> PublishLivePointsOnly(
            this IObservable<Timestamped<Point>> source, TimeSpan pointTtl)
        {
            //Sequence of live points only
            var usefulSource = source
                .FilterOutStaleOnArrival(pointTtl)
                .Select(x => new Timestamped<Point?>(x.Value, x.Timestamp));

            //Sequence of nulls, throttled to only publish when there is no live point
            var staleSource = usefulSource
                .Throttle(tp =>
                {
                    var ageOfPoint = DateTimeOffset.Now.ToUniversalTime().Subtract(tp.Timestamp.ToUniversalTime());
                    var timeUntilStale = pointTtl.Subtract(ageOfPoint);
                    return Observable.Timer(timeUntilStale).Select(_ => tp);
                })
                .Select(tp =>
                {
                    Log.Debug("There is no currently 'live' point - pushing NULL to points sequence.");
                    return new Timestamped<Point?>(null, DateTimeOffset.Now.ToUniversalTime());
                });

            //Return the live and stale sequence merged together
            return usefulSource
                .Merge(staleSource)
                .StartWith(new Timestamped<Point?>(null, DateTimeOffset.Now.ToUniversalTime())); //Prefix the sequence with an initial value of NULL
        }

        public static IObservable<Timestamped<Point>> FilterOutStaleOnArrival(
            this IObservable<Timestamped<Point>> source, TimeSpan pointTtl)
        {
            return source.Where(tp =>
            {
                //Log and filter out points which are stale on arrival
                var ageOfPoint = DateTimeOffset.Now.ToUniversalTime().Subtract(tp.Timestamp.ToUniversalTime());
                var timeUntilStale = pointTtl.Subtract(ageOfPoint);

                if (timeUntilStale.Ticks <= 0)
                {
                    Log.Warn(string.Format(
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

        public static IObservable<T> OnPropertyChanges<T>(this DependencyObject source, DependencyProperty property)
        {
            return Observable.Create<T>(o =>
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(property, property.OwnerType);
                if (dpd == null)
                {
                    o.OnError(new InvalidOperationException("Can not register change handler for this dependency property."));
                }

                EventHandler handler = delegate { o.OnNext((T)source.GetValue(property)); };
                dpd.AddValueChanged(source, handler);

                return Disposable.Create(() => dpd.RemoveValueChanged(source, handler));
            });
        }

        /// <summary>
        /// Returns an observable sequence of the value of a property when <paramref name="source"/> raises <seealso cref="INotifyPropertyChanged.PropertyChanged"/> for the given property.
        /// </summary>
        /// <typeparam name="T">The type of the source object. Type must implement <seealso cref="INotifyPropertyChanged"/>.</typeparam>
        /// <typeparam name="TProperty">The type of the property that is being observed.</typeparam>
        /// <param name="source">The object to observe property changes on.</param>
        /// <param name="property">An expression that describes which property to observe.</param>
        /// <returns>Returns an observable sequence of the property values as they change.</returns>
        public static IObservable<TProperty> OnPropertyChanges<T, TProperty>(this T source, Expression<Func<T, TProperty>> property)
            where T : INotifyPropertyChanged
        {
            return Observable.Create<TProperty>(o =>
            {
                var propertyName = property.GetPropertyInfo().Name;
                var propertySelector = property.Compile();

                return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                handler => handler.Invoke,
                                h => source.PropertyChanged += h,
                                h => source.PropertyChanged -= h)
                            .Where(e => e.EventArgs.PropertyName == propertyName)
                            .Select(e => propertySelector(source))
                            .Subscribe(o);
            });
        }

        /// <summary>
        /// Returns an observable sequence of the source any time the <c>PropertyChanged</c> event is raised.
        /// </summary>
        /// <typeparam name="T">The type of the source object. Type must implement <seealso cref="INotifyPropertyChanged"/>.</typeparam>
        /// <param name="source">The object to observe property changes on.</param>
        /// <returns>Returns an observable sequence of the value of the source when ever the <c>PropertyChanged</c> event is raised.</returns>
        public static IObservable<T> OnAnyPropertyChanges<T>(this T source)
            where T : INotifyPropertyChanged
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => handler.Invoke,
                h => source.PropertyChanged += h,
                h => source.PropertyChanged -= h)
                .Select(_ => source);
        }
    }
}