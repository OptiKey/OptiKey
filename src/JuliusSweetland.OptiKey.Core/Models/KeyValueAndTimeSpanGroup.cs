// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
        private string commonTimeSpan;
        private bool monitorHierarchy = true;
        private bool containsVarious = false;

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
                bool atLeastOneValue = keyValueAndTimeSpans.Any()
                                  && keyValueAndTimeSpans.Select(kvats => kvats.TimeSpanTotalMilliseconds)
                                  .Where(ms => ms != null)
                                  .Any();
                ContainsVarious = atLeastOneValue && (CommonTimeSpanTotalMilliseconds == null);
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
                    bool atLeastOneValue = keyValueAndTimeSpans.Any()
                                  && keyValueAndTimeSpans.Select(kvats => kvats.TimeSpanTotalMilliseconds)
                                  .Where(ms => ms != null)
                                  .Any();
                    ContainsVarious = atLeastOneValue && (CommonTimeSpanTotalMilliseconds == null);
                    monitorHierarchy = true;
                });

            calculateCommonTimeSpan();
        }

        public string Name { get { return name; } }
        public List<KeyValueAndTimeSpan> KeyValueAndTimeSpans { get { return keyValueAndTimeSpans; } }
        public bool ContainsVarious {
            get { return containsVarious;  }        
            set { SetProperty(ref containsVarious, value); }
        }
        public string CommonTimeSpanTotalMilliseconds
        {
            get { return commonTimeSpan; }
            set { SetProperty(ref commonTimeSpan, value); }
        }
    }
}
