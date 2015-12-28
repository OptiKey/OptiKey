using System;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyValueAndTimeSpan : BindableBase
    {
        private readonly string name;
        private readonly KeyValue keyValue;
        private TimeSpan? timeSpan;

        public KeyValueAndTimeSpan(string name, KeyValue keyValue, TimeSpan? timeSpan)
        {
            this.name = name;
            this.keyValue = keyValue;
            this.timeSpan = timeSpan;
        }

        public string Name { get { return name; } }
        public KeyValue KeyValue { get { return keyValue; } }
        public double? TimeSpanTotalMilliseconds
        {
            get { return timeSpan != null ? timeSpan.Value.TotalMilliseconds : (double?)null; }
            set { SetProperty(ref timeSpan, value != null ? TimeSpan.FromMilliseconds(value.Value) : (TimeSpan?)null); }
        }
    }
}
