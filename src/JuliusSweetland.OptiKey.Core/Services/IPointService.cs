// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive;
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IPointService : INotifyErrors
    {
        bool KalmanFilterSupported { get; }
        event EventHandler<Timestamped<Point>> Point;
    }
}
