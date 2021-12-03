// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
namespace JuliusSweetland.OptiKey.Models
{
    public struct TriggerSignal
    {
        public TriggerSignal(double? signal, double? progress, PointAndKeyValue pointAndKeyValue)
            : this()
        {
            Signal = signal;
            Progress = progress;
            PointAndKeyValue = pointAndKeyValue;
        }

        //Signals are -1 (low) or 1 (high)
        public double? Signal { get; private set; }

        //Progress ranges from 0 to 1 in 1/100th of a percent
        public double? Progress { get; private set; }

        public PointAndKeyValue PointAndKeyValue { get; private set; }

        public override string ToString()
        {
            return string.Format("Signal:{0}, Progress:{1}, PointAndKeyValue:{2}", Signal, Progress, PointAndKeyValue);
        }
    }
}
