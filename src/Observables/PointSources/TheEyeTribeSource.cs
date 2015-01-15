using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Services;

namespace JuliusSweetland.ETTA.Observables.PointSources
{
    public class TheEyeTribeSource : IPointSource
    {
        #region Fields
        
        private readonly TimeSpan pointTtl;
        private readonly ITheEyeTribePointService theEyeTribePointService;

        private IObservable<Timestamped<PointAndKeyValue?>> sequence;

        #endregion

        #region Ctor

        public TheEyeTribeSource(
            TimeSpan pointTtl,
            ITheEyeTribePointService theEyeTribePointService)
        {
            this.pointTtl = pointTtl;
            this.theEyeTribePointService = theEyeTribePointService;
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
                            eh => theEyeTribePointService.Point += eh,
                            eh => theEyeTribePointService.Point -= eh)
                        .Where(_ => State == RunningStates.Running)
                        .Select(ep => ep.EventArgs)
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
