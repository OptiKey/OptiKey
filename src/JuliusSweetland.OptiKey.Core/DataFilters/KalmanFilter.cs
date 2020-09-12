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

        public double Update(double measurement)
        {
            Gain = (EstimationConfidence + ProcessNoise) / (EstimationConfidence + ProcessNoise + MeasurementNoise);
            EstimationConfidence = MeasurementNoise * (EstimationConfidence + ProcessNoise) / (MeasurementNoise + EstimationConfidence + ProcessNoise);
            double result = EstimatedValue + (measurement - EstimatedValue) * Gain;
            EstimatedValue = result;
            return result;
        }
    }
}
