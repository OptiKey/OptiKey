// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Contracts;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IWindowManipulationService : INotifyErrors
    {
        event EventHandler SizeAndPositionInitialised;

        bool SizeAndPositionIsInitialised { get; }
        IntPtr WindowHandle { get; }
        Rect WindowBounds { get; }
        WindowStates WindowState { get; }

        void ChangeState(WindowStates state, DockEdges dockPosition);
        void Expand(ExpandToDirections direction, double amountInPx);
        double GetOpacity();
        bool GetPersistedState();
        void Hide();
        void IncrementOrDecrementOpacity(bool increment);
        void Maximise();
        void Minimise();
        void Move(MoveToDirections direction, double? amountInPx);
        void PersistSizeAndPosition();
        void ResizeDockToCollapsed();
        void ResizeDockToFull();
        void Restore();
        void RestoreSavedState();
        void SetOpacity(double opacity);
        void Shrink(ShrinkFromDirections direction, double amountInPx);
        void OverridePersistedState(bool inPersistNewState, string inWindowState, string inPosition, string inDockSize, string inWidth, string inHeight, string inHorizontalOffset, string inVerticalOffset);
        void RestorePersistedState();
        void DisableResize();
        void SetResizeState();
        void InvokeMoveWindow(string parameterString);
        void SetFocusable(bool focusable);
        void SetShowInTaskbar(bool showInTaskbar);
    }
}
