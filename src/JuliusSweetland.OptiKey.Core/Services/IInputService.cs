// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
        event EventHandler<Point> LivePosition; // Get up-to-date position, regardless of whether it has changed.
        event EventHandler<Tuple<PointAndKeyValue, double>> SelectionProgress;
        event EventHandler<PointAndKeyValue> Selection;
        event EventHandler<Tuple<List<Point>, KeyValue, List<string>>> SelectionResult;

        IPointSource PointSource { get; set; }
        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        SelectionModes SelectionMode { set; }
        bool MultiKeySelectionSupported { set; }

        void RequestSuspend();
        void RequestResume();
    }
}
