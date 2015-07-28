using System;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IWindowManipulationService : INotifyErrors
    {
        event EventHandler<Rect> SizePositionChanged;

        void ChangeOpacity(bool increase);
        void CollapseDock();
        void Expand(ExpandToDirections direction, int? amountInDp);
        void ExpandDock();
        void Maximise();
        void Move(MoveToDirections direction, int? amountInDp);
        void Restore();
        void Shrink(ShrinkFromDirections direction, int? amountInDp);
    }
}
