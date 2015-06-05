using System;
using System.Reactive;
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IPointGeneratingService : INotifyErrors
    {
        event EventHandler<Timestamped<Point>> Point;
    }
}
