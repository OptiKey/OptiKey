// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using log4net;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using KeyEventHandler = System.Windows.Forms.KeyEventHandler;
using Keys = System.Windows.Forms.Keys;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public class KeyboardKeyDownUpSource : ITriggerSource
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Keys triggerKey;
        private readonly KeyboardHookListener keyboardHookListener;

        private IPointSource pointSource;
        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public KeyboardKeyDownUpSource(
            Enums.Keys triggerKey,
            IPointSource pointSource)
        {
            this.triggerKey = (System.Windows.Forms.Keys)triggerKey; //Cast to the Windows.Forms.Keys enum
            this.pointSource = pointSource;

            keyboardHookListener = new KeyboardHookListener(new GlobalHooker())
            {
                Enabled = true
            };

            System.Windows.Application.Current.Exit += (sender, args) =>
            {
                keyboardHookListener.Dispose();
                Log.Debug("Keyboard hook listener disposed.");
            };

            /*
             * Keys: http://msdn.microsoft.com/en-GB/library/system.windows.forms.keys.aspx
             * KeyDown: happens when the person presses a key (when the keyboard first detects a finger on a key, this happens when the key is pressed down).
             * KeyPress: happens when a key is pressed and then released.
             * KeyUp: happens when the key is released
             */
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
                    var keyDowns = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                            handler => new KeyEventHandler(handler),
                            h => keyboardHookListener.KeyDown += h,
                            h => keyboardHookListener.KeyDown -= h)
                        .Do(ep => Log.DebugFormat("Key down detected with KeyCode:{0} | KeyValue:{1} | KeyData:{2} | Modifiers:{3} | Alt:{4} | Control:{5} | Shift:{6}", ep.EventArgs.KeyCode, ep.EventArgs.KeyValue, ep.EventArgs.KeyData, ep.EventArgs.Modifiers, ep.EventArgs.Alt, ep.EventArgs.Control, ep.EventArgs.Shift))
                        .Where(ep => ep.EventArgs.KeyCode == triggerKey)
                        .Select(_ => true)
                        .Do(_ => Log.DebugFormat("Trigger key down detected ({0})", triggerKey));

                    var keyUps = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                            handler => new KeyEventHandler(handler),
                            h => keyboardHookListener.KeyUp += h,
                            h => keyboardHookListener.KeyUp -= h)
                        .Do(ep => Log.DebugFormat("Key up detected with KeyCode:{0} | KeyValue:{1} | KeyData:{2} | Modifiers:{3} | Alt:{4} | Control:{5} | Shift:{6}", ep.EventArgs.KeyCode, ep.EventArgs.KeyValue, ep.EventArgs.KeyData, ep.EventArgs.Modifiers, ep.EventArgs.Alt, ep.EventArgs.Control, ep.EventArgs.Shift))
                        .Where(ep => ep.EventArgs.KeyCode == triggerKey)
                        .Select(_ => false)
                        .Do(_ => Log.DebugFormat("Trigger key up detected ({0})", triggerKey));

                    sequence = keyDowns.Merge(keyUps)
                        .DistinctUntilChanged()
                        .SkipWhile(b => b == false) //Ensure the first value we hit is a true, i.e. a key down
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
