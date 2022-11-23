// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IInputService : INotifyPropertyChanged, INotifyErrors
    {
        event EventHandler<int> PointsPerSecond;
        event EventHandler<Tuple<Point, KeyValue>> CurrentPosition;
        event EventHandler<Tuple<TriggerTypes, PointAndKeyValue, double>> SelectionProgress;
        event EventHandler<Tuple<TriggerTypes, PointAndKeyValue>> Selection; //This event occurs every time a key or point is selected, e.g. when a dwell is completed
        event EventHandler<Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>> SelectionResult; //This event occurs every time a selection triggers a meaningful result, e.g. if not using multi-key selection then this will fire straight after the selection to carry the key/point value. If multi key selection is enabled then the first selection will not have a result, but the next selection may complete a multi-key selection and so be followed by a result event containing the match.
        IDictionary<KeyValue, TimeSpanOverrides> OverrideTimesByKey { get; }
        IPointSource PointSource { get; set; }
        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        SelectionModes SelectionMode { get;  set; }
        bool MultiKeySelectionSupported { set; }

        void RequestSuspend();
        void RequestResume();
    }
}
