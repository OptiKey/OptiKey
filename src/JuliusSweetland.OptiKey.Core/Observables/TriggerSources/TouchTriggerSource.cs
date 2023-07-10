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
    public class TouchTriggerSource : ITriggerSource
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private IObservable<TriggerSignal> triggerSequence;

        private IPointSource pointSource;

        public TouchTriggerSource(IPointSource pointSource)
        {
            this.pointSource = pointSource;
        }

        public RunningStates State { get; set; }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap { private get; set; }        

        public IObservable<TriggerSignal> Sequence
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
                        .CombineLatest(pointSource.Sequence, (tp, point) =>
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

        /// <summary>
        /// Change the point and key value source. N.B. After setting this any existing subscription 
        /// to the sequence must be disposed and the getter called again to recreate the sequence again.
        /// </summary>
        public IPointSource PointSource
        {
            get { return pointSource; }
            set { pointSource = value; }
        }

    }
}
