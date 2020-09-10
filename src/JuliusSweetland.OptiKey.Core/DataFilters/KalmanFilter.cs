using JuliusSweetland.OptiKey.Properties;
// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
namespace JuliusSweetland.OptiKey.DataFilters
{
    public class KalmanFilter
    {
        double ProcessNoise; //Standard deviation - Q
        double MeasurementNoise; // R
        double EstimationConfidence; //P
        double EstimatedValue; // X 
        double Gain; // K

        public KalmanFilter(double initialValue = 0f, double confidenceOfInitialValue = 1f, double processNoise = 0.0001f, double measurementNoise = 0.01f)
        {
            this.ProcessNoise = processNoise;
            this.MeasurementNoise = measurementNoise;
            this.EstimationConfidence = confidenceOfInitialValue;
            this.EstimatedValue = initialValue;
        }

        public double UpdateOld(double measurement)
        {
            Gain = (EstimationConfidence + ProcessNoise) / (EstimationConfidence + ProcessNoise + MeasurementNoise);
            EstimationConfidence = MeasurementNoise * (EstimationConfidence + ProcessNoise) / (MeasurementNoise + EstimationConfidence + ProcessNoise);
            double result = EstimatedValue + (measurement - EstimatedValue) * Gain;
            EstimatedValue = result;
            return result;
        }

        public double Update(double measurement)
        {
            double dampingLevel = Settings.Default.TobiiEyeXProcessingLevel == Enums.DataStreamProcessingLevels.High
                ? .1f : Settings.Default.TobiiEyeXProcessingLevel == Enums.DataStreamProcessingLevels.Medium
                ? .5f : Settings.Default.TobiiEyeXProcessingLevel == Enums.DataStreamProcessingLevels.Low
                ? .9f : 1;
            double fixationRadius = Settings.Default.PointSelectionTriggerFixationRadiusInPixels;
            double delta = System.Math.Abs(measurement - EstimatedValue);
            double multiplier = delta <= Settings.Default.PointSelectionTriggerLockOnRadiusInPixels
                ? 0 : dampingLevel == 1 
                ? 1 : delta <= fixationRadius
                ? dampingLevel * (System.Math.Pow(delta, 1.7) / System.Math.Pow(fixationRadius, 1.7))
                : dampingLevel;
            double result = EstimatedValue + (measurement - EstimatedValue) * multiplier;
            EstimatedValue = result;
            return result;
        }
    }
}
