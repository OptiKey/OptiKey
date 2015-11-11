namespace JuliusSweetland.OptiKey.Models
{
    public struct TriggerSignal
    {
        public TriggerSignal(double? signal, double? progress, PointAndKeyValue? pointAndKeyValue, string notification = null)
            : this()
        {
            Signal = signal;
            Progress = progress;
            PointAndKeyValue = pointAndKeyValue;
            Notification = notification;
        }

        //Signals are -1 (low) or 1 (high)
        public double? Signal { get; private set; }

        //Progress ranges from 0 to 1 in 1/100th of a percent
        public double? Progress { get; private set; }

        public PointAndKeyValue? PointAndKeyValue { get; private set; }

        //Notification text will be displayed/played to user by InputService if set
        public string Notification { get; private set; }

        public override string ToString()
        {
            return string.Format("Signal:{0}, Progress:{1}, PointAndKeyValue:{2}, Notification:{3}", Signal, Progress, PointAndKeyValue, Notification);
        }
    }
}
