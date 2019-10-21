// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IWindowManipulationService : INotifyErrors
    {
        event EventHandler SizeAndPositionInitialised;

        bool SizeAndPositionIsInitialised { get; }
        Rect WindowBounds { get; }
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
        void Restore();
        void RestoreSavedState();
        void SetOpacity(double opacity);
        void Shrink(ShrinkFromDirections direction, double amountInPx);
        void OverrideSizeAndPosition(bool inPersistNewState, string inWindowState, string inPosition, string inDockSize, string inWidth, string inHeight, string inHorizontalOffset, string inVerticalOffset);
        void RollbackOverride();
    }
}
