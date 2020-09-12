// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.DataFilters
{
    /// <summary>
    /// For a continuous stream of points and optional KeyValues the GazeFilter 
    /// will apply logic intended to smooth the inputs and provide silky smooth 
    /// positioning in accordance with settings configurable by the user
    /// </summary>
    public class GazeFilter
    {
        public double DampingLevel { get; set; } = 1f; // from .01 to 1. rate at which the point moves in accordance with the gaze
        public double FixationRadius { get; set; } = 0f; // the "gravity well" where damping varies
        public double LockRadius { get; set; } = 0f; // the distance within which the point will not move
        private Point LastPoint;

        public GazeFilter()
        {
            var level = Settings.Default.TobiiEyeXProcessingLevel; 
            this.DampingLevel = level == Enums.DataStreamProcessingLevels.High
                ? .1f : level == Enums.DataStreamProcessingLevels.Medium
                ? .4f : level == Enums.DataStreamProcessingLevels.Low
                ? .7f : 1;
            this.FixationRadius = Settings.Default.PointSelectionTriggerFixationRadiusInPixels;
            this.LockRadius = Settings.Default.PointSelectionTriggerLockOnRadiusInPixels;
            this.LastPoint = new Point();
        }


        /// <summary>
        /// If point is on a key then prioritize speed over smoothing.
        /// Else if inside the lock radius then do not move.
        /// Else if inside the fixation radius then prioritize smoothing.
        /// Else if outside the fixation radius then balance speed with smoothing.
        /// </summary>
        public Point Update(Point point, KeyValue keyValue = null)
        {

            Vector delta = point - LastPoint;
            double multiplier = keyValue != null
                    ? .5 * (1 + DampingLevel) 
                : delta.Length > FixationRadius 
                    ? .2 * (1 + 4 * DampingLevel) 
                : delta.Length > LockRadius
                    ? DampingLevel * (System.Math.Pow(delta.Length, 1 + DampingLevel) / System.Math.Pow(FixationRadius, 1 + DampingLevel))
                : 0;
            Point result = LastPoint + multiplier * (point - LastPoint);
            LastPoint = result;
            return result;
        }
    }
}
