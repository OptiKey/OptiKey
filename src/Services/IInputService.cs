using System;
using System.Collections.Generic;
using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public interface IInputService
    {
        event EventHandler<int> PointsPerSecond;
        event EventHandler<Tuple<Point?, KeyValue?>> CurrentPosition;
        event EventHandler<Tuple<PointAndKeyValue?, double>> SelectionProgress;
        event EventHandler<PointAndKeyValue> Selection;
        event EventHandler<Tuple<List<Point>, FunctionKeys?, string, List<string>>> SelectionResult;
        event EventHandler<Exception> Error;

        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        KeyEnabledStates KeyEnabledStates { set; }
        SelectionModes SelectionMode { set; }
        bool CapturingMultiKeySelection { get; }
    }
}
