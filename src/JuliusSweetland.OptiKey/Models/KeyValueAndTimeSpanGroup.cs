using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

            var monitorChildren = true;

            //Monitor children for a common timespan
            keyValueAndTimeSpans.ForEach(kvats1 => 
                kvats1.OnPropertyChanges(kvats2 => kvats2.TimeSpanTotalMilliseconds)
                    .Where(tstm => monitorChildren)
                    .Subscribe(timeSpan => CalculateCommonTimeSpan()));
            CalculateCommonTimeSpan();

            //Propogate common time span changes to children
            this.OnPropertyChanges(kwatsg => kwatsg.CommonTimeSpanTotalMilliseconds).Subscribe(
                commonTimeSpan =>
                {
                    monitorChildren = false;
                    keyValueAndTimeSpans.ForEach(kvats => kvats.TimeSpanTotalMilliseconds = commonTimeSpan);
                    monitorChildren = true;
                });
        }

        public string Name { get { return name; } }
        public List<KeyValueAndTimeSpan> KeyValueAndTimeSpans { get { return keyValueAndTimeSpans; } }
        public double? CommonTimeSpanTotalMilliseconds
        {
            get { return commonTimeSpan != null ? commonTimeSpan.Value.TotalMilliseconds : (double?)null; }
            set { SetProperty(ref commonTimeSpan, value != null ? TimeSpan.FromMilliseconds(value.Value) : (TimeSpan?)null); }
        }
        
        private void CalculateCommonTimeSpan()
        {
            if (keyValueAndTimeSpans.Any()
                && keyValueAndTimeSpans.Select(kvats => kvats.TimeSpanTotalMilliseconds).Distinct().Count() == 1)
            {
                CommonTimeSpanTotalMilliseconds = keyValueAndTimeSpans.First().TimeSpanTotalMilliseconds;
            }
            else
            {
                CommonTimeSpanTotalMilliseconds = null;
            }
        }
    }
}
