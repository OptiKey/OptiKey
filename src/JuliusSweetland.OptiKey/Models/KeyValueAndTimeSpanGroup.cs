using System;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKey.Extensions;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyValueAndTimeSpanGroup : BindableBase
    {
        private readonly string name;
        private readonly List<KeyValueAndTimeSpan> keyValueAndTimeSpans;
        private TimeSpan? commonTimeSpan;

        public KeyValueAndTimeSpanGroup(string name, List<KeyValueAndTimeSpan> keyValueAndTimeSpans)
        {
            this.name = name;
            this.keyValueAndTimeSpans = keyValueAndTimeSpans;

            CheckIfAllChildTimeSpansAreTheSame();

            //Subscribe to all child timespan changes
            keyValueAndTimeSpans.ForEach(kvats1 => 
                kvats1.OnPropertyChanges(kvats2 => kvats2.TimeSpanTotalMilliseconds)
                    .Subscribe(timeSpan => CheckIfAllChildTimeSpansAreTheSame()));
        }

        public string Name { get { return name; } }
        public double? CommonTimeSpanTotalMilliseconds
        {
            get { return commonTimeSpan != null ? commonTimeSpan.Value.TotalMilliseconds : (double?)null; }
            set { SetProperty(ref commonTimeSpan, value != null ? TimeSpan.FromMilliseconds(value.Value) : (TimeSpan?)null); }
        }
        public List<KeyValueAndTimeSpan> KeyValueAndTimeSpans { get { return keyValueAndTimeSpans; } }

        private void CheckIfAllChildTimeSpansAreTheSame()
        {
            if (keyValueAndTimeSpans.Any()
                && keyValueAndTimeSpans.Select(kvats => kvats.TimeSpanTotalMilliseconds).Distinct().Count() == 1)
            {
                CommonTimeSpanTotalMilliseconds = keyValueAndTimeSpans.First().TimeSpanTotalMilliseconds;
            }
        }
    }
}
