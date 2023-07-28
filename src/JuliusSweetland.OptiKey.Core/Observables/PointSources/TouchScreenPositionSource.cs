// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using System.Windows.Input;
using JuliusSweetland.OptiKey.Observables.TriggerSources;
using log4net;
using JuliusSweetland.OptiKey.Static;

namespace JuliusSweetland.OptiKey.Observables.PointSources
{
    public class TouchScreenPositionSource : IPointSource
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly TimeSpan pointTtl;

        private IObservable<Timestamped<PointAndKeyValue>> pointSequence;
        private IObservable<TriggerSignal> triggerSequence;

        public TouchScreenPositionSource(TimeSpan pointTtl)
        {
            this.pointTtl = pointTtl;
        }

        public RunningStates State { get; set; }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap { private get; set; }

        // Used for transforming points
        private Window window = null;

        public IObservable<Timestamped<PointAndKeyValue>> Sequence
        {
            get
            {
                if (pointSequence == null)
                {
                    var touchEvents = Observable.FromEventPattern<TouchFrameEventHandler, TouchFrameEventArgs>(
                        handler => new TouchFrameEventHandler(handler),
                        h => Touch.FrameReported += h,
                        h => Touch.FrameReported -= h)
                        .Where(_ => State == RunningStates.Running)
                        .Where(_ => System.Windows.Application.Current.MainWindow != null)
                        .Select(ep =>
                        {
                            // get handle to mainWindow once
                            if (window == null)
                                window = System.Windows.Application.Current.MainWindow;

                            TouchPoint tp = ep.EventArgs.GetPrimaryTouchPoint(null);
                            return window.PointToScreen(tp.Position);
                            
                        });

                    // set up a separate repeating sequence to ensure we get regular events during periods when no touch is occurring
                    var replayLastEvent = touchEvents
                        .StartWith(default(Point))
                        .SkipWhile(p => p == default(Point))
                        .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(50)),
                            (point, _) => point);

                    pointSequence = Observable.Merge(touchEvents, replayLastEvent)
                        .Timestamp()
                        .PublishLivePointsOnly(pointTtl)
                        .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                        .Replay(1)
                        .RefCount();

                }

                return pointSequence;
            }
        }        

    }
}
