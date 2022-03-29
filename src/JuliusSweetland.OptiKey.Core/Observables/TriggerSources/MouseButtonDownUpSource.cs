// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive.Linq;
using System.Windows.Forms;
using log4net;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public class MouseButtonDownUpSource : ITriggerSource
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MouseButtons triggerButton;
        private readonly MouseHookListener mouseHookListener;

        private IPointSource pointSource;
        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public MouseButtonDownUpSource(
            Enums.MouseButtons triggerButton,
            IPointSource pointSource)
        {
            this.triggerButton = (System.Windows.Forms.MouseButtons)triggerButton; //Cast to the Windows.Forms.MouseButtons enum
            this.pointSource = pointSource;

            mouseHookListener = new MouseHookListener(new GlobalHooker())
            {
                Enabled = true
            };

            System.Windows.Application.Current.Exit += (sender, args) =>
            {
                mouseHookListener.Dispose();
                Log.Debug("Mouse hook listener disposed.");
            };
        }

        #endregion

        #region Properties

        public RunningStates State { get; set; }

        /// <summary>
        /// Change the point and key value source. N.B. After setting this any existing subscription 
        /// to the sequence must be disposed and the getter called again to recreate the sequence again.
        /// </summary>
        public IPointSource PointSource
        {
            get { return pointSource; }
            set { pointSource = value; }
        }

        public IObservable<TriggerSignal> Sequence
        {
            get
            {
                if (sequence == null)
                {
                    var buttonDowns = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => new MouseEventHandler(handler),
                        h => mouseHookListener.MouseDown += h,
                        h => mouseHookListener.MouseDown -= h)
                        .Where(ep => ep.EventArgs.Button == triggerButton)
                        .Select(_ => true);

                    var buttonUps = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => new MouseEventHandler(handler),
                        h => mouseHookListener.MouseUp += h,
                        h => mouseHookListener.MouseUp -= h)
                        .Where(ep => ep.EventArgs.Button == triggerButton)
                        .Select(_ => false);

                    sequence = buttonDowns.Merge(buttonUps)
                        .DistinctUntilChanged()
                        .SkipWhile(b => b == false) //Ensure the first value we hit is a true, i.e. a mouse down
                        .CombineLatest(pointSource.Sequence, (b, point) => new TriggerSignal(b ? 1 : -1, null, point.Value))
                        .DistinctUntilChanged(signal => signal.Signal) //Combining latest will output a trigger signal for every change in BOTH sequences - only output when the trigger signal changes
                        .Where(_ => State == RunningStates.Running)
                        .Publish()
                        .RefCount()
                        .Finally(() => {
                            sequence = null;
                        });
                }

                return sequence;
            }
        }

        #endregion
    }
}
