using System;
using System.Reactive;
using System.Windows;
using TETCSharpClient;

namespace JuliusSweetland.ETTA.Services
{
    public interface ITheEyeTribePointService : IGazeListener, INotifyErrors
    {
        event EventHandler<Timestamped<Point>> Point;
    }
}
