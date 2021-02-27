// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Static;

namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    /// <summary>
    /// Monitors point source for input patterns. If the pattern is completed an action is triggered
    /// </summary>
    public class EyeGestureSource : ITriggerSource
    {
        #region Fields

        private IPointSource pointSource;
        private IObservable<TriggerSignal> sequence;

        #endregion

        #region Ctor

        public EyeGestureSource(IPointSource pointSource)
        {
            this.pointSource = pointSource;
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
                    sequence = Observable.Create<TriggerSignal>(observer =>
                    {
                        bool disposed = false;
                        Action disposeAllSubscriptions = null;
                        List<EyeGesture> gestureList = null;
                        double moveDelta = 0;

                        var eyeGestureSubscription = pointSource.Sequence
                        .Where(_ => disposed == false)
                        .Where(_ => State == RunningStates.Running)
                        .Where(tp => tp.Value != null) //Filter out stale indicators - the fixation progress is not reset by the points sequence being stale
                        .Select(tp => new Timestamped<PointAndKeyValue>(tp.Value, tp.Timestamp))
                        .Buffer(2, 1) //Sliding buffer of 2 (last & current) that moves by 1 value at a time
                        .Subscribe(tps =>
                        {
                            var checkPoint = tps.Last().Value.Point;
                            moveDelta = (2 * moveDelta + (checkPoint - tps.First().Value.Point).Length) / 3;
                            

                            if (gestureList == null || Settings.Default.EyeGestureUpdated)
                            {
                                Settings.Default.EyeGestureUpdated = false;
                                try
                                {
                                    gestureList = XmlEyeGestures.ReadFromString(Settings.Default.EyeGestureString).GestureList.ToList();
                                }
                                catch { gestureList = null; }
                            }

                            if (gestureList != null && Settings.Default.EyeGesturesEnabled)
                            {
                                for(int i = 0; i < gestureList.Count; i++)
                                {
                                    var gesture = gestureList[i];
                                    if (gesture.enabled)
                                    {
                                        if (gesture.StepIndex >= gesture.Steps.Count
                                            && DateTime.Now > gesture.TimeStamp + TimeSpan.FromMilliseconds(gesture.Cooldown))
                                        {
                                            gesture.StepIndex = 0;
                                            gesture.TimeStamp = DateTime.Now;
                                        }
                                        else if (gesture.StepIndex < gesture.Steps.Count)
                                        {
                                            var step = gesture.Steps[gesture.StepIndex];

                                            if (gesture.StepIndex > 0 && DateTime.Now > gesture.TimeStamp + TimeSpan.FromMilliseconds(step.TimeLimit))
                                            {
                                                gesture.StepIndex = 0;
                                                gesture.TimeStamp = DateTime.Now;
                                                step = gesture.Steps[0];
                                            }

                                            if (PointAdvancesGesture(checkPoint, moveDelta, ref gesture, ref step))
                                            {
                                                gesture.StepIndex++;
                                                if (step.Commands != null && step.Commands.Any())
                                                {
                                                    var keyCommands = new List<KeyCommand>();
                                                    foreach (var command in step.Commands)
                                                    {
                                                        keyCommands.Add(new KeyCommand()
                                                        {
                                                            Name = command.CommandName,
                                                            Value = command.Value,
                                                            BackAction = command.BackAction,
                                                            Method = command.Method,
                                                            Argument = command.Argument
                                                        });
                                                    }
                                                    var pakv = new PointAndKeyValue(checkPoint, new KeyValue(gesture.Name) { Commands = keyCommands });
                                                    observer.OnNext(new TriggerSignal(1, 1, pakv));
                                                    observer.OnNext(new TriggerSignal(null, 0, null));
                                                }
                                            }
                                            else if (gesture.StepIndex == 1 && gesture.Steps.Count > 1)
                                            {
                                                step = gesture.Steps[0];
                                                //advance the expiration if we still meet the parameters of the starting step
                                                _ = PointAdvancesGesture(checkPoint, moveDelta, ref gesture, ref step);
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        ex =>
                        {
                            observer.OnError(ex);
                            disposeAllSubscriptions();
                        });

                        disposeAllSubscriptions = () =>
                        {
                            disposed = true;

                            if (eyeGestureSubscription != null)
                            {
                                eyeGestureSubscription.Dispose();
                                eyeGestureSubscription = null;
                            }

                            sequence = null;
                        };

                        return disposeAllSubscriptions;
                    })
                    .Publish()
                    .RefCount();
                }

                return sequence;
            }
        }

        #endregion

        #region Methods

        private bool PointAdvancesGesture(Point checkpoint, double moveDelta, ref EyeGesture gesture, ref EyeGestureStep step)
        {
            var result = false;

            if (step.type == EyeGestureStepTypes.Fixation)
            {
                if (moveDelta > step.Radius)
                    step.DwellStart = DateTime.Now;
                else if (DateTime.Now > step.DwellStart + TimeSpan.FromMilliseconds(step.DwellTime))
                {
                    result = true;
                    gesture.FixationPoint = checkpoint;
                }
            }

            else if (step.type == EyeGestureStepTypes.LookInDirection)
            {
                result = (checkpoint.X - gesture.PointStamp.X) / (Graphics.VirtualScreenWidthInPixels * step.X / 100d) > 1
                      && (checkpoint.Y - gesture.PointStamp.Y) / (Graphics.VirtualScreenHeightInPixels * step.Y / 100d) > 1;
            }
            else if (step.type == EyeGestureStepTypes.LookAtArea)
            {
                var left = step.Left * Graphics.VirtualScreenWidthInPixels / 100d;
                var top = step.Top * Graphics.VirtualScreenHeightInPixels / 100d;

                if (step.Round)
                {
                    var xRadius = step.Width * Graphics.VirtualScreenWidthInPixels / 50d;
                    var yRadius = step.Height * Graphics.VirtualScreenHeightInPixels / 50d;
                    var xLength = checkpoint.X - (left + xRadius);
                    var yLength = checkpoint.Y - (top + yRadius);

                    if (Math.Pow(xLength, 2) / Math.Pow(xRadius, 2) + Math.Pow(yLength, 2) / Math.Pow(yRadius, 2) > 1)
                        step.DwellStart = DateTime.Now;
                    else if (DateTime.Now > step.DwellStart + TimeSpan.FromMilliseconds(step.DwellTime))
                        result = true;
                }
                else
                {
                    var width = step.Width * Graphics.VirtualScreenWidthInPixels / 100d;
                    var height = step.Height * Graphics.VirtualScreenHeightInPixels / 100d;

                    if (checkpoint.X < left || checkpoint.X > left + width || checkpoint.Y < top || checkpoint.Y > top + height)
                        step.DwellStart = DateTime.Now;
                    else if (DateTime.Now > step.DwellStart + TimeSpan.FromMilliseconds(step.DwellTime))
                        result = true;
                }
            }
            else
            {
                if ((checkpoint - gesture.FixationPoint).Length > Graphics.VirtualScreenHeightInPixels * step.Width / 50d)
                    step.DwellStart = DateTime.Now;
                else if (DateTime.Now > step.DwellStart + TimeSpan.FromMilliseconds(step.DwellTime))
                    result = true;
            }

            if (result)
            {
                gesture.PointStamp = checkpoint;
                gesture.TimeStamp = DateTime.Now;
            }
            return result;
        }

        #endregion
    }
}
