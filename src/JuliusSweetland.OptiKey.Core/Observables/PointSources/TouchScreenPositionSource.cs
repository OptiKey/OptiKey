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
    public class TouchScreenPositionSource : IPointSource, ITriggerSource
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly TimeSpan pointTtl;

        private IObservable<Timestamped<PointAndKeyValue>> pointSequence;
        private IObservable<TriggerSignal> triggerSequence;

        public TouchScreenPositionSource(TimeSpan pointTtl)
        {
            this.pointTtl = pointTtl;
            Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);            
        }

        void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            // This method is just used for logging / testing - elsewhere we sign up to events in our sequence
            TouchPoint tp = e.GetPrimaryTouchPoint(null);
            TouchPointCollection tpc = e.GetTouchPoints(null);
            //Log.InfoFormat("KMCN touch point vs cursor, {0}, {1}, {2}, {3}, {4}", System.Windows.Forms.Cursor.Position, tp.Position, tp.Size, tp.Action, tp.Bounds);
        }

        public RunningStates State { get; set; }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap { private get; set; }

        public IObservable<Timestamped<PointAndKeyValue>> Sequence
        {
            get
            {
                if (pointSequence == null)
                {
                    pointSequence = Observable.FromEventPattern<TouchFrameEventHandler, TouchFrameEventArgs>(
                            handler => new TouchFrameEventHandler(handler),
                            h => Touch.FrameReported += h,
                            h => Touch.FrameReported -= h)
                        .Where(_ => State == RunningStates.Running)
                        .Select(ep =>
                        {
                            TouchPoint tp = ep.EventArgs.GetPrimaryTouchPoint(null);
                            var x = tp.Position.X * Graphics.DipScalingFactorX;
                            var y = tp.Position.Y * Graphics.DipScalingFactorY;
                            Log.Info($"KMCN: point event = ({x}, {y}) {tp.Action} ({this.GetHashCode()})");
                            return new Point(x, y);
                        })
                        .Timestamp()
                        .PublishLivePointsOnly(pointTtl)
                        .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                        .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                        .RefCount();
                }

                return pointSequence;
            }
        }

        IObservable<TriggerSignal> ITriggerSource.Sequence
        {
            get
            {
                if (triggerSequence == null)
                {
                    triggerSequence = Observable.FromEventPattern<TouchFrameEventHandler, TouchFrameEventArgs>(
                            handler => new TouchFrameEventHandler(handler),
                            h => Touch.FrameReported += h,
                            h => Touch.FrameReported -= h)
                        .Where(_ => State == RunningStates.Running)                        
                        .Select(ep => ep.EventArgs.GetPrimaryTouchPoint(null))
                        .Where(tp => tp.Action != TouchAction.Move) // only use up/down events for trigger
                        .DistinctUntilChanged() 
                        .SkipWhile(tp => tp.Action == TouchAction.Up) // Ensure the first value we hit is a touch down
                        .CombineLatest(pointSequence, (tp, point) =>
                        {                            
                            return new TriggerSignal(tp.Action == TouchAction.Down ? 1 : -1, null, point.Value);
                        })
                        .DistinctUntilChanged(signal => signal.Signal) //Combining latest will output a trigger signal for every change in BOTH sequences - only output when the trigger signal changes
                        .Do(ts => {
                            Log.Info($"KMCN Trigger: {ts.Signal} {ts.PointAndKeyValue} ({this.GetHashCode()})");
                        })
                        .Where(_ => State == RunningStates.Running)
                        .Publish()
                        .RefCount()
                        .Finally(() => {
                            triggerSequence = null;
                        });
                }

                return triggerSequence;
            }
        }

        public IPointSource PointSource
        {
            get { return this; }        
            set {
                // no-op - we use this class as a pointsource too
                 }
        }

        IPointSource ITriggerSource.PointSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
