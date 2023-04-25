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

namespace JuliusSweetland.OptiKey.Observables.PointSources
{
    public class TouchScreenPositionSource : IPointSource, ITriggerSource
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        //public event XInputButtonDownEventHandler ButtonDown;
        //public event XInputButtonUpEventHandler ButtonUp;

        
        Point touchPt = new Point();
        bool is_touch_down = false;
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
            //// Making sure it will only use the most top-left touch point - the pointing finger of a right-hand person
            //// i'm not sure why i eventually didn't use it. anyhow, if used, should have an option for left-handed person too.
            //touchPt.X = e.GetTouchPoints(null)[0].Position.X;
            //touchPt.Y = e.GetTouchPoints(null)[0].Position.Y;
            //foreach (TouchPoint _touchPoint in e.GetTouchPoints(null))
            //{
            //    if ((_touchPoint.Position.X < touchPt.X) & (_touchPoint.Position.Y < touchPt.Y))
            //    {
            //        touchPt.X = _touchPoint.Position.X;
            //        touchPt.Y = _touchPoint.Position.Y;
            //    }
            //}

            //TouchPoint pointFromWindow = e.GetTouchPoint(null); //relative to Top-Left corner of Window
            //Point locationFromScreen = this.PointToScreen(new Point(pointFromWindow.Position.X, pointFromWindow.Position.Y)); //translate to coordinates relative to Top-Left corner of Screen

            TouchPoint tp = e.GetPrimaryTouchPoint(null);
            touchPt.X = tp.Position.X;
            touchPt.Y = tp.Position.Y;
            
            Log.InfoFormat("KMCN touch point vs cursor, {0}, {1}, {2}, {3}, {4}", System.Windows.Forms.Cursor.Position, tp.Position, tp.Size, tp.Action, tp.Bounds);
//            Log.InfoFormat("KMCN cursor position {0}", System.Windows.Forms.Cursor.Position);

            TouchPointCollection touchPoints = e.GetTouchPoints(null);
            if ((touchPoints.Count == 1) & (touchPoints[0].Action == TouchAction.Up))
            {
                this.is_touch_down = false;
            }
            else
            {
                this.is_touch_down = true;
            }
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
                        .Do(ep =>
                        {
                            TouchPoint tp = ep.EventArgs.GetPrimaryTouchPoint(null);
                            Log.InfoFormat("touch point {0}, {1}, {2}, {3}", tp.Position, tp.Size, tp.Action, tp.Bounds);
                            Log.InfoFormat("cursor position {0}", System.Windows.Forms.Cursor.Position);
                            
                            //TouchPointCollection touchPoints = ep.EventArgs.GetTouchPoints(null);
                            //foreach (var tp in touchPoints)
                            //{
                            //    // bounds = [left, top, w, h]. point = centre point. size = [w, h]
                            //    // isActive
                            //    //new Point(System.Windows.Forms.Cursor.Position.X * Convert.ToInt32(this.is_touch_down), System.Windows.Forms.Cursor.Position.Y * Convert.ToInt32(this.is_touch_down)

                            //Log.InfoFormat("touch point {0}, {1}, {2}, {3}", tp.Position, tp.Size, tp.Action, tp.Bounds);
                            //}
                        })                        
                        .Where(_ => State == RunningStates.Running)
                        .Select(ep =>
                        {
                            TouchPoint tp = ep.EventArgs.GetPrimaryTouchPoint(null);
                            return tp.Position;
                        })
                        .Timestamp()
                        .PublishLivePointsOnly(pointTtl)
                        .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                        .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                        .RefCount();
                    /*
                    sequence = Observable
                        .Interval(Settings.Default.PointsMousePositionSampleInterval)
                        .Where(_ => State == RunningStates.Running)
                        .Select(ep => {                            
                        //System.Windows.Input.TouchPoint tp = GetPrimaryTouchPoint(null);
                            return new Point(System.Windows.Forms.Cursor.Position.X * Convert.ToInt32(this.is_touch_down), System.Windows.Forms.Cursor.Position.Y * Convert.ToInt32(this.is_touch_down));  // this is where it gets it's coordinates
                        })
                        //.Select(_ => new Point(touchPt.X, touchPt.Y))  // need to convert them to relative coords
                        .Timestamp()
                        .PublishLivePointsOnly(pointTtl)
                        .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                        .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                        .RefCount();
                        */
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
                        .Where(tp => tp.Action != TouchAction.Move) // only up/down events for trigger
                        .DistinctUntilChanged() // shouldn't be necessary?!
                        .SkipWhile(tp => tp.Action == TouchAction.Up) // Ensure the first value we hit is a touch down
                        .CombineLatest(pointSequence, (tp, point) =>
                        {
                            Log.Info($"Trigger: {tp.Action} {tp.Position}");
                            // we seem to be getting multiple ups and downs here - maybe just from two sources (key trigger + point trigger)
                            return new TriggerSignal(tp.Action == TouchAction.Down ? 1 : -1, null, point.Value);
                        }) // FIXME - down or up?
                        .DistinctUntilChanged(signal => signal.Signal) //Combining latest will output a trigger signal for every change in BOTH sequences - only output when the trigger signal changes
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
                // no-op
                 }
        }

        IPointSource ITriggerSource.PointSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
