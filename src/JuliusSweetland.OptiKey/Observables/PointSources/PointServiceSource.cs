using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.DataFilters;

namespace JuliusSweetland.OptiKey.Observables.PointSources
{
    public class PointServiceSource : IPointSource
    {
        #region Fields
        
        private readonly TimeSpan pointTtl;
        private readonly IPointService pointGeneratingService;
        private readonly KalmanFilter kalmanFilterX;
        private readonly KalmanFilter kalmanFilterY;

        private IObservable<Timestamped<PointAndKeyValue?>> sequence;

        #endregion

        #region Ctor

        public PointServiceSource(
            TimeSpan pointTtl,
            IPointService pointGeneratingService)
        {
            this.pointTtl = pointTtl;
            this.pointGeneratingService = pointGeneratingService;
            this.kalmanFilterX = new KalmanFilter(0, 0, 5, 1000);
            this.kalmanFilterY = new KalmanFilter(0, 0, 5, 1000);
        }

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
                    sequence = Observable.FromEventPattern<Timestamped<Point>>(
                            eh => pointGeneratingService.Point += eh,
                            eh => pointGeneratingService.Point -= eh)
                        .Where(_ => State == RunningStates.Running)
                        .Select(ep => 
                            !pointGeneratingService.CanHaveSmoothingApplied 
                            ? ep.EventArgs
                            : new Timestamped<Point>(
                                value: new Point(
                                    x: (int)kalmanFilterX.Update(ep.EventArgs.Value.X),
                                    y: (int)kalmanFilterY.Update(ep.EventArgs.Value.Y)
                                ),
                                timestamp: ep.EventArgs.Timestamp)
                        )
                        .PublishLivePointsOnly(pointTtl)
                        .Select(tp => new Timestamped<PointAndKeyValue?>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                        .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                        .RefCount();
                }

                return sequence;
            }
        }
        
        #endregion
    }
}
