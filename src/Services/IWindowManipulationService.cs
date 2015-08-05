using System;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IWindowManipulationService : INotifyErrors
    {
        event EventHandler<Rect> SizeAndPositionInitialised;

        bool SizeAndPositionIsInitialised { get; }

        void ChangeOpacity(bool increase);
        void Expand(ExpandToDirections direction, double amountInPx);
        void Maximise();
        void Minimise();
        void Move(MoveToDirections direction, double? amountInPx);
        void ResizeDockToCollapsed();
        void ResizeDockToFull();
        void Restore();
        void Shrink(ShrinkFromDirections direction, double amountInPx);
    }
}
