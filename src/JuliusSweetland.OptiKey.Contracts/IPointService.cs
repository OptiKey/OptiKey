// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Reactive;
using System.Windows;

namespace JuliusSweetland.OptiKey.Contracts
{
    public interface IPointService : INotifyErrors
    {
        event EventHandler<Timestamped<Point>> Point;
    }
}