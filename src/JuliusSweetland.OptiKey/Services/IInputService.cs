using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IInputService : INotifyPropertyChanged, INotifyErrors
    {
        event EventHandler<int> PointsPerSecond;
        event EventHandler<Tuple<Point, KeyValue?>> CurrentPosition;
        event EventHandler<Tuple<PointAndKeyValue?, double>> SelectionProgress;
        event EventHandler<PointAndKeyValue> Selection;
        event EventHandler<Tuple<List<Point>, FunctionKeys?, string, List<string>>> SelectionResult;

        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        SelectionModes SelectionMode { set; }
        bool MultiKeySelectionSupported { set; }

        void RequestSuspend();
        void RequestResume();
    }
}
