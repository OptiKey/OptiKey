using System;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IWindowManipulationService : INotifyErrors
    {
        event EventHandler SizeAndPositionInitialised;

        bool SizeAndPositionIsInitialised { get; }
        WindowStates WindowState { get; }

        void Expand(ExpandToDirections direction, double amountInPx);
        double GetOpacity();
        void Hide();
        void IncrementOrDecrementOpacity(bool increment);
        void Maximise();
        void Minimise();
        void Move(MoveToDirections direction, double? amountInPx);
        void ResizeDockToCollapsed();
        void ResizeDockToFull();
        void ResizeDockToSpecificHeight(double heightAsPercentScreen, bool persistNewSize);
        void Restore();
        void SetOpacity(double opacity);
        void Shrink(ShrinkFromDirections direction, double amountInPx);
    }
}
