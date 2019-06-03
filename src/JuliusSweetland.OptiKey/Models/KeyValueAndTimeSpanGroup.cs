// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
        private bool monitorHierarchy = true;

        public KeyValueAndTimeSpanGroup(string name, List<KeyValueAndTimeSpan> keyValueAndTimeSpans)
        {
            this.name = name;
            this.keyValueAndTimeSpans = keyValueAndTimeSpans;

            Action calculateCommonTimeSpan = () =>
            {
                monitorHierarchy = false;
                CommonTimeSpanTotalMilliseconds = keyValueAndTimeSpans.Any()
                                              && keyValueAndTimeSpans.Select(kvats => kvats.TimeSpanTotalMilliseconds)
                                                  .Distinct()
                                                  .Count() == 1
                    ? keyValueAndTimeSpans.First().TimeSpanTotalMilliseconds
                    : null;
                monitorHierarchy = true;
            };

            //Monitor children for a common timespan
            keyValueAndTimeSpans.ForEach(kvats1 => 
                kvats1.OnPropertyChanges(kvats2 => kvats2.TimeSpanTotalMilliseconds)
                    .Where(tstm => monitorHierarchy)
                    .Subscribe(timeSpan => calculateCommonTimeSpan()));
            
            //Propagate common time span changes to children
            this.OnPropertyChanges(kwatsg => kwatsg.CommonTimeSpanTotalMilliseconds)
                .Where(tstm => monitorHierarchy)
                .Subscribe(commonTimeSpan =>
                {
                    monitorHierarchy = false;
                    keyValueAndTimeSpans.ForEach(kvats => kvats.TimeSpanTotalMilliseconds = commonTimeSpan);
                    monitorHierarchy = true;
                });

            calculateCommonTimeSpan();
        }

        public string Name { get { return name; } }
        public List<KeyValueAndTimeSpan> KeyValueAndTimeSpans { get { return keyValueAndTimeSpans; } }
        public double? CommonTimeSpanTotalMilliseconds
        {
            get { return commonTimeSpan != null ? commonTimeSpan.Value.TotalMilliseconds : (double?)null; }
            set { SetProperty(ref commonTimeSpan, value != null ? TimeSpan.FromMilliseconds(value.Value) : (TimeSpan?)null); }
        }
    }
}
