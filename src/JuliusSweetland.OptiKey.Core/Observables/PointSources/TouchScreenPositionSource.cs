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

namespace JuliusSweetland.OptiKey.Observables.PointSources
{
    public class TouchScreenPositionSource : IPointSource
    {
        //Point touchPt = new Point();
        bool is_touch_down = false;
        private readonly TimeSpan pointTtl;

        private IObservable<Timestamped<PointAndKeyValue>> sequence;

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
                if (sequence == null)
                {
                    sequence = Observable
                        .Interval(Settings.Default.PointsMousePositionSampleInterval)
                        .Where(_ => State == RunningStates.Running)
                        .Select(_ => new Point(System.Windows.Forms.Cursor.Position.X * Convert.ToInt32(this.is_touch_down), System.Windows.Forms.Cursor.Position.Y * Convert.ToInt32(this.is_touch_down)))  // this is where it gets it's coordinates
                        //.Select(_ => new Point(touchPt.X, touchPt.Y))  // need to convert them to relative coords
                        .Timestamp()
                        .PublishLivePointsOnly(pointTtl)
                        .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value.ToPointAndKeyValue(PointToKeyValueMap), tp.Timestamp))
                        .Replay(1) //Buffer one value for every subscriber so there is always a 'most recent' point available
                        .RefCount();
                }

                return sequence;
            }
        }
    }
}
