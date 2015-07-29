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
        void Expand(ExpandToDirections direction, double amountInDp);
        void Maximise();
        void Move(MoveToDirections direction, double? amountInDp);
        void ResizeDockToCollapsed();
        void ResizeDockToFull();
        void Restore();
        void Shrink(ShrinkFromDirections direction, double amountInDp);
    }
}
