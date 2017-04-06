using System;
using System.Reactive;
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IPointService : INotifyErrors
    {
        bool CanHaveSmoothingApplied { get; }
        event EventHandler<Timestamped<Point>> Point;
    }
}
