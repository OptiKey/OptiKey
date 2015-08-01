using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;
using JuliusSweetland.OptiKey.Native.Structs;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;
        private readonly IntPtr windowHandle;
        private Screen screen;
        private Rect screenBoundsInPx;
        private Rect screenBoundsInDp;
        private readonly Func<double> getOpacity;
        private readonly Func<WindowStates> getWindowState;
        private readonly Func<WindowStates> getLastNonMaximisedState;
        private readonly Func<Rect> getFloatingSizeAndPosition;
        private readonly Func<DockEdges> getDockPosition;
        private readonly Func<DockSizes> getDockSize;
        private readonly Func<double> getFullDockThicknessAsPercentageOfScreen;
        private readonly Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Action<WindowStates> saveWindowState;
        private readonly Action<WindowStates> saveLastNonMaximisedState;
        private readonly Action<Rect> saveFloatingSizeAndPosition;
        private readonly Action<DockEdges> saveDockPosition;
        private readonly Action<DockSizes> saveDockSize;
        private readonly Action<double> saveFullDockThicknessAsPercentageOfScreen;
        private readonly Action<double> saveCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Action<double> saveOpacity;

        private int appBarCallBackId = -1;

        private delegate void ApplySizeAndPositionDelegate(Rect rect);

        #endregion

        #region Ctor
        
        internal WindowManipulationService(
            Window window,
            Func<double> getOpacity,
            Func<WindowStates> getWindowState,
            Func<WindowStates> getLastNonMaximisedState,
            Func<Rect> getFloatingSizeAndPosition,
            Func<DockEdges> getDockPosition,
            Func<DockSizes> getDockSize,
            Func<double> getFullDockThicknessAsPercentageOfScreen,
            Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness,
            Action<double> saveOpacity,
            Action<WindowStates> saveWindowState,
            Action<WindowStates> saveLastNonMaximisedState,
            Action<Rect> saveFloatingSizeAndPosition,
            Action<DockEdges> saveDockPosition,
            Action<DockSizes> saveDockSize,
            Action<double> saveFullDockThicknessAsPercentageOfScreen,
            Action<double> saveCollapsedDockThicknessAsPercentageOfFullDockThickness)
        {
            this.window = window;
            this.getOpacity = getOpacity;
            this.getWindowState = getWindowState;
            this.getLastNonMaximisedState = getLastNonMaximisedState;
            this.getDockPosition = getDockPosition;
            this.getDockSize = getDockSize;
            this.getFullDockThicknessAsPercentageOfScreen = getFullDockThicknessAsPercentageOfScreen;
            this.getCollapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness;
            this.getFloatingSizeAndPosition = getFloatingSizeAndPosition;
            this.saveOpacity = saveOpacity;
            this.saveWindowState = saveWindowState;
            this.saveLastNonMaximisedState = saveLastNonMaximisedState;
            this.saveFloatingSizeAndPosition = saveFloatingSizeAndPosition;
            this.saveDockPosition = saveDockPosition;
            this.saveDockSize = saveDockSize;
            this.saveFullDockThicknessAsPercentageOfScreen = saveFullDockThicknessAsPercentageOfScreen;
            this.saveCollapsedDockThicknessAsPercentageOfFullDockThickness = saveCollapsedDockThicknessAsPercentageOfFullDockThickness;

            windowHandle = new WindowInteropHelper(window).EnsureHandle();
            screen = window.GetScreen();
            screenBoundsInPx = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height);
            var screenBoundsTopLeftInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.TopLeft);
            var screenBoundsBottomRightInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.BottomRight);
            screenBoundsInDp = new Rect(screenBoundsTopLeftInDp.X, screenBoundsTopLeftInDp.Y,
                screenBoundsBottomRightInDp.X - screenBoundsTopLeftInDp.X,
                screenBoundsBottomRightInDp.Y - screenBoundsTopLeftInDp.Y);

            RestoreState();
        
            window.Closed += (_, __) => UnRegisterAppBar();
        }

        #endregion
        
        #region Events

        public event EventHandler<Rect> SizeAndPositionInitialised;
        public event EventHandler<Exception> Error;

        #endregion

        #region Properties

        public bool SizeAndPositionIsInitialised { get; private set; }

        #endregion

        #region Public Methods

        public void ChangeOpacity(bool increase)
        {
            window.Opacity += increase ? 0.1 : -0.1;
            window.Opacity.CoerceToLowerLimit(0.1);
            window.Opacity.CoerceToUpperLimit(1);
            saveOpacity(window.Opacity);
        }

        public void Expand(ExpandToDirections direction, double amountInPx)
        {
            var windowState = getWindowState();
            if (windowState == WindowStates.Maximised) return;

            var distanceToBottomBoundary = screenBoundsInDp.Bottom - (window.Top + window.ActualHeight);
            var yAdjustmentToBottom = distanceToBottomBoundary < 0 ? distanceToBottomBoundary : (amountInPx / Graphics.DipScalingFactorY).CoerceToUpperLimit(distanceToBottomBoundary);
            var distanceToTopBoundary = window.Top - screenBoundsInDp.Top;
            var yAdjustmentToTop = distanceToTopBoundary < 0 ? distanceToTopBoundary : (amountInPx / Graphics.DipScalingFactorY).CoerceToUpperLimit(distanceToTopBoundary);
            var distanceToLeftBoundary = window.Left - screenBoundsInDp.Left;
            var xAdjustmentToLeft = distanceToLeftBoundary < 0 ? distanceToLeftBoundary : (amountInPx / Graphics.DipScalingFactorX).CoerceToUpperLimit(distanceToLeftBoundary);
            var distanceToRightBoundary = screenBoundsInDp.Right - (window.Left + window.ActualWidth);
            var xAdjustmentToRight = distanceToRightBoundary < 0 ? distanceToRightBoundary : (amountInPx / Graphics.DipScalingFactorX).CoerceToUpperLimit(distanceToRightBoundary);

            switch (windowState)
            {
                case WindowStates.Floating:
                    switch (direction) //Handle vertical adjustment
                    {
                        case ExpandToDirections.Bottom:
                        case ExpandToDirections.BottomLeft:
                        case ExpandToDirections.BottomRight:
                            window.Height += yAdjustmentToBottom;
                            break;

                        case ExpandToDirections.Top:
                        case ExpandToDirections.TopLeft:
                        case ExpandToDirections.TopRight:
                            var heightBeforeAdjustment = window.ActualHeight;
                            window.Height += yAdjustmentToTop;
                            var actualYAdjustmentToTop = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                            window.Top -= actualYAdjustmentToTop;
                            break;
                    }
                    switch (direction) //Handle horizontal adjustment
                    {
                        case ExpandToDirections.Left:
                        case ExpandToDirections.BottomLeft:
                        case ExpandToDirections.TopLeft:
                            var widthBeforeAdjustment = window.ActualWidth;
                            window.Width += xAdjustmentToLeft;
                            var actualXAdjustmentToLeft = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                            window.Left -= actualXAdjustmentToLeft;
                            break;

                        case ExpandToDirections.Right:
                        case ExpandToDirections.BottomRight:
                        case ExpandToDirections.TopRight:
                            window.Width += xAdjustmentToRight;
                            break;
                    }
                    PersistSizeAndPosition();
                    break;

                case WindowStates.Docked:
                    var dockPosition = getDockPosition();
                    var dockSize = getDockSize();
                    var adjustment = false;
                    if (dockPosition == DockEdges.Top &&
                        (direction == ExpandToDirections.Bottom ||
                         direction == ExpandToDirections.BottomLeft ||
                         direction == ExpandToDirections.BottomRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualHeight + yAdjustmentToBottom) / screenBoundsInDp.Height) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualHeight + yAdjustmentToBottom) / screenBoundsInDp.Height) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Bottom &&
                        (direction == ExpandToDirections.Top ||
                         direction == ExpandToDirections.TopLeft ||
                         direction == ExpandToDirections.TopRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualHeight + yAdjustmentToTop) / screenBoundsInDp.Height) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualHeight + yAdjustmentToTop) / screenBoundsInDp.Height) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Left &&
                        (direction == ExpandToDirections.Right ||
                         direction == ExpandToDirections.TopRight ||
                         direction == ExpandToDirections.BottomRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualWidth + xAdjustmentToRight) / screenBoundsInDp.Width) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualWidth + xAdjustmentToRight) / screenBoundsInDp.Width) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Right &&
                        (direction == ExpandToDirections.Left ||
                         direction == ExpandToDirections.TopLeft ||
                         direction == ExpandToDirections.BottomLeft))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualWidth + xAdjustmentToLeft) / screenBoundsInDp.Width) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualWidth + xAdjustmentToLeft) / screenBoundsInDp.Width) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    if (adjustment)
                    {
                        var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                        SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
                    }
                    break;
            }
        }

        public void Maximise()
        {
            saveLastNonMaximisedState(getWindowState());
            window.WindowState = WindowState.Maximized;
            saveWindowState(WindowStates.Maximised);
        }

        public void Move(MoveToDirections direction, double? amountInPx)
        {
            var windowState = getWindowState();
            if (windowState == WindowStates.Maximised) return;

            var floatingSizeAndPosition = getFloatingSizeAndPosition();
            var distanceToBottomBoundaryIfFloating = screenBoundsInDp.Bottom - (floatingSizeAndPosition.Top + floatingSizeAndPosition.Height);
            var distanceToTopBoundaryIfFloating = floatingSizeAndPosition.Top - screenBoundsInDp.Top;
            var distanceToLeftBoundaryIfFloating = floatingSizeAndPosition.Left - screenBoundsInDp.Left;
            var distanceToRightBoundaryIfFloating = screenBoundsInDp.Right - (floatingSizeAndPosition.Left + floatingSizeAndPosition.Width);

            bool adjustment;
            if (amountInPx != null)
            {
                adjustment = Move(direction, amountInPx.Value, distanceToBottomBoundaryIfFloating,
                    distanceToTopBoundaryIfFloating, distanceToLeftBoundaryIfFloating, distanceToRightBoundaryIfFloating,
                    windowState, floatingSizeAndPosition);
            }
            else
            {
                adjustment = MoveToEdge(direction, windowState, distanceToLeftBoundaryIfFloating,
                    distanceToRightBoundaryIfFloating, distanceToBottomBoundaryIfFloating,
                    distanceToTopBoundaryIfFloating);
            }

            if (adjustment)
            {
                switch (getWindowState())
                {
                    case WindowStates.Floating:
                        PersistSizeAndPosition();
                        break;

                    case WindowStates.Docked:
                        var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                        SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
                        break;
                }
            }
        }

        public void ResizeDockToCollapsed()
        {
            if (getWindowState() != WindowStates.Docked || getDockSize() == DockSizes.Collapsed) return;

            saveDockSize(DockSizes.Collapsed);
            var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Collapsed);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void ResizeDockToFull()
        {
            if (getWindowState() != WindowStates.Docked || getDockSize() == DockSizes.Full) return;
            var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Full);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
            saveDockSize(DockSizes.Full); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void Restore()
        {
            if (getWindowState() != WindowStates.Maximised) return;

            window.WindowState = WindowState.Normal;
            var previousWindowState = getLastNonMaximisedState();
            RestoreSizeAndPosition(previousWindowState, getFloatingSizeAndPosition());
            saveWindowState(previousWindowState);
        }

        public void Shrink(ShrinkFromDirections direction, double amountInPx)
        {
            var windowState = getWindowState();
            if (windowState == WindowStates.Maximised) return;

            var distanceToBottomBoundary = screenBoundsInDp.Bottom - (window.Top + window.ActualHeight);
            var yAdjustmentFromBottom = distanceToBottomBoundary < 0 ? distanceToBottomBoundary : 0 - (amountInPx / Graphics.DipScalingFactorY);
            var distanceToTopBoundary = window.Top - screenBoundsInDp.Top;
            var yAdjustmentFromTop = distanceToTopBoundary < 0 ? distanceToTopBoundary : 0 - (amountInPx / Graphics.DipScalingFactorY);
            var distanceToLeftBoundary = window.Left - screenBoundsInDp.Left;
            var xAdjustmentFromLeft = distanceToLeftBoundary < 0 ? distanceToLeftBoundary : 0 - (amountInPx / Graphics.DipScalingFactorX);
            var distanceToRightBoundary = screenBoundsInDp.Right - (window.Left + window.ActualWidth);
            var xAdjustmentFromRight = distanceToRightBoundary < 0 ? distanceToRightBoundary : 0 - (amountInPx / Graphics.DipScalingFactorX);

            switch (windowState)
            {
                case WindowStates.Floating:
                    switch (direction) //Handle vertical adjustment
                    {
                        case ShrinkFromDirections.Bottom:
                        case ShrinkFromDirections.BottomLeft:
                        case ShrinkFromDirections.BottomRight:
                            window.Height += yAdjustmentFromBottom;
                            break;

                        case ShrinkFromDirections.Top:
                        case ShrinkFromDirections.TopLeft:
                        case ShrinkFromDirections.TopRight:
                            var heightBeforeAdjustment = window.ActualHeight;
                            window.Height += yAdjustmentFromTop;
                            var actualYAdjustmentToTop = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                            window.Top -= actualYAdjustmentToTop;
                            break;
                    }
                    switch (direction) //Handle horizontal adjustment
                    {
                        case ShrinkFromDirections.Left:
                        case ShrinkFromDirections.BottomLeft:
                        case ShrinkFromDirections.TopLeft:
                            var widthBeforeAdjustment = window.ActualWidth;
                            window.Width += xAdjustmentFromLeft;
                            var actualXAdjustmentToLeft = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                            window.Left -= actualXAdjustmentToLeft;
                            break;

                        case ShrinkFromDirections.Right:
                        case ShrinkFromDirections.BottomRight:
                        case ShrinkFromDirections.TopRight:
                            window.Width += xAdjustmentFromRight;
                            break;
                    }
                    PersistSizeAndPosition();
                    break;

                case WindowStates.Docked:
                    var dockPosition = getDockPosition();
                    var dockSize = getDockSize();
                    var adjustment = false;
                    if (dockPosition == DockEdges.Top &&
                        (direction == ShrinkFromDirections.Bottom || direction == ShrinkFromDirections.BottomLeft || direction == ShrinkFromDirections.BottomRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualHeight + yAdjustmentFromBottom) / screenBoundsInDp.Height) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualHeight + yAdjustmentFromBottom) / screenBoundsInDp.Height) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Bottom &&
                        (direction == ShrinkFromDirections.Top || direction == ShrinkFromDirections.TopLeft || direction == ShrinkFromDirections.TopRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualHeight + yAdjustmentFromTop) / screenBoundsInDp.Height) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualHeight + yAdjustmentFromTop) / screenBoundsInDp.Height) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Left &&
                        (direction == ShrinkFromDirections.Right || direction == ShrinkFromDirections.TopRight || direction == ShrinkFromDirections.BottomRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualWidth + xAdjustmentFromRight) / screenBoundsInDp.Width) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualWidth + xAdjustmentFromRight) / screenBoundsInDp.Width) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Right &&
                        (direction == ShrinkFromDirections.Left || direction == ShrinkFromDirections.TopLeft || direction == ShrinkFromDirections.BottomLeft))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualWidth + xAdjustmentFromLeft) / screenBoundsInDp.Width) * 100);
                        }
                        else
                        {
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualWidth + xAdjustmentFromLeft) / screenBoundsInDp.Width) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    if (adjustment)
                    {
                        var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                        SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
                    }
                    break;
            }
        }

        #endregion

        #region Private Methods

        private IntPtr AppBarPositionChangeCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == appBarCallBackId &&
                getWindowState() == WindowStates.Docked)
            {
                if (wParam.ToInt32() == (int)AppBarNotify.PositionChanged)
                {
                    var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                    SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void ApplySizeAndPosition(Rect rect)
        {
            window.Top = rect.Top;
            window.Left = rect.Left;
            window.Width = rect.Width;
            window.Height = rect.Height;

            PersistSizeAndPosition();

            if (!SizeAndPositionIsInitialised)
            {
                SizeAndPositionIsInitialised = true;
                if (SizeAndPositionInitialised != null)
                {
                    SizeAndPositionInitialised(this, rect);
                }
            }
        }

        private Rect CalculateDockSizeAndPositionInPx(DockEdges position, DockSizes size)
        {
            double x, y, width, height;
            var thickness = size == DockSizes.Full
                ? getFullDockThicknessAsPercentageOfScreen()
                : (getFullDockThicknessAsPercentageOfScreen() * getCollapsedDockThicknessAsPercentageOfFullDockThickness()) / 10000; //Percentage of a percentage

            switch (position)
            {
                case DockEdges.Top:
                    x = screenBoundsInPx.X;
                    y = screenBoundsInPx.Y;
                    width = screenBoundsInPx.Width;
                    height = screenBoundsInPx.Height * (thickness / 100);
                    break;

                case DockEdges.Bottom:
                    x = screenBoundsInPx.X;
                    y = screenBoundsInPx.Y + screenBoundsInPx.Height - (screenBoundsInPx.Height * (thickness / 100));
                    width = screenBoundsInPx.Width;
                    height = screenBoundsInPx.Height * (thickness / 100);
                    break;

                case DockEdges.Left:
                    x = screenBoundsInPx.X;
                    y = screenBoundsInPx.Y;
                    width = screenBoundsInPx.Width * (thickness / 100);
                    height = screenBoundsInPx.Height;
                    break;

                default: //case DockEdges.Right:
                    x = screenBoundsInPx.X + screenBoundsInPx.Width - (screenBoundsInPx.Width * (thickness / 100));
                    y = screenBoundsInPx.Y;
                    width = screenBoundsInPx.Width * (thickness / 100);
                    height = screenBoundsInPx.Height;
                    break;
            }
            
            return new Rect(x, y, width, height);
        }

        private bool Move(MoveToDirections direction, double amountInPx, double distanceToBottomBoundaryIfFloating,
            double distanceToTopBoundaryIfFloating, double distanceToLeftBoundaryIfFloating,
            double distanceToRightBoundaryIfFloating, WindowStates windowState, Rect floatingSizeAndPosition)
        {
            bool adjustment = false;
            var yAdjustmentAmount = amountInPx / Graphics.DipScalingFactorY;
            var xAdjustmentAmount = amountInPx / Graphics.DipScalingFactorX;
            var yAdjustmentToBottom = distanceToBottomBoundaryIfFloating < 0 ? distanceToBottomBoundaryIfFloating : yAdjustmentAmount.CoerceToUpperLimit(distanceToBottomBoundaryIfFloating);
            var yAdjustmentToTop = distanceToTopBoundaryIfFloating < 0 ? distanceToTopBoundaryIfFloating : yAdjustmentAmount.CoerceToUpperLimit(distanceToTopBoundaryIfFloating);
            var xAdjustmentToLeft = distanceToLeftBoundaryIfFloating < 0 ? distanceToLeftBoundaryIfFloating : xAdjustmentAmount.CoerceToUpperLimit(distanceToLeftBoundaryIfFloating);
            var xAdjustmentToRight = distanceToRightBoundaryIfFloating < 0 ? distanceToRightBoundaryIfFloating : xAdjustmentAmount.CoerceToUpperLimit(distanceToRightBoundaryIfFloating);

            switch (windowState)
            {
                case WindowStates.Docked:
                    switch (getDockPosition())
                    {
                        case DockEdges.Top:
                            switch (direction)
                            {
                                case MoveToDirections.Bottom:
                                case MoveToDirections.BottomLeft:
                                case MoveToDirections.BottomRight:
                                    UnRegisterAppBar();
                                    saveWindowState(WindowStates.Floating);
                                    saveLastNonMaximisedState(WindowStates.Floating);
                                    window.Top = screenBoundsInDp.Top;
                                    switch (direction)
                                    {
                                        case MoveToDirections.Bottom:
                                            window.Left = floatingSizeAndPosition.Left;
                                            break;

                                        case MoveToDirections.BottomLeft:
                                            window.Left = floatingSizeAndPosition.Left - xAdjustmentToLeft;
                                            break;

                                        case MoveToDirections.BottomRight:
                                            window.Left = floatingSizeAndPosition.Right + xAdjustmentToRight;
                                            break;
                                    }
                                    window.Height = floatingSizeAndPosition.Height;
                                    window.Width = floatingSizeAndPosition.Width;
                                    adjustment = true;
                                    break;
                            }
                            break;

                        case DockEdges.Bottom:
                            switch (direction)
                            {
                                case MoveToDirections.Top:
                                case MoveToDirections.TopLeft:
                                case MoveToDirections.TopRight:
                                    UnRegisterAppBar();
                                    saveWindowState(WindowStates.Floating);
                                    saveLastNonMaximisedState(WindowStates.Floating);
                                    window.Top = screenBoundsInDp.Bottom - floatingSizeAndPosition.Height;
                                    switch (direction)
                                    {
                                        case MoveToDirections.Top:
                                            window.Left = floatingSizeAndPosition.Left;
                                            break;

                                        case MoveToDirections.TopLeft:
                                            window.Left = floatingSizeAndPosition.Left - xAdjustmentToLeft;
                                            break;

                                        case MoveToDirections.TopRight:
                                            window.Left = floatingSizeAndPosition.Right + xAdjustmentToRight;
                                            break;
                                    }
                                    window.Height = floatingSizeAndPosition.Height;
                                    window.Width = floatingSizeAndPosition.Width;
                                    adjustment = true;
                                    break;
                            }
                            break;

                        case DockEdges.Left:
                            switch (direction)
                            {
                                case MoveToDirections.Right:
                                case MoveToDirections.TopRight:
                                case MoveToDirections.BottomRight:
                                    UnRegisterAppBar();
                                    saveWindowState(WindowStates.Floating);
                                    saveLastNonMaximisedState(WindowStates.Floating);
                                    window.Left = screenBoundsInDp.Left;
                                    switch (direction)
                                    {
                                        case MoveToDirections.Right:
                                            window.Top = floatingSizeAndPosition.Top;
                                            break;

                                        case MoveToDirections.TopRight:
                                            window.Top = floatingSizeAndPosition.Top - yAdjustmentToTop;
                                            break;

                                        case MoveToDirections.BottomRight:
                                            window.Top = floatingSizeAndPosition.Top + yAdjustmentToBottom;
                                            break;
                                    }
                                    window.Height = floatingSizeAndPosition.Height;
                                    window.Width = floatingSizeAndPosition.Width;
                                    adjustment = true;
                                    break;
                            }
                            break;

                        case DockEdges.Right:
                            switch (direction)
                            {
                                case MoveToDirections.Left:
                                case MoveToDirections.TopLeft:
                                case MoveToDirections.BottomLeft:
                                    UnRegisterAppBar();
                                    saveWindowState(WindowStates.Floating);
                                    saveLastNonMaximisedState(WindowStates.Floating);
                                    window.Left = screenBoundsInDp.Right - floatingSizeAndPosition.Width;
                                    switch (direction)
                                    {
                                        case MoveToDirections.Left:
                                            window.Top = floatingSizeAndPosition.Top;
                                            break;

                                        case MoveToDirections.TopLeft:
                                            window.Top = floatingSizeAndPosition.Top - yAdjustmentToTop;
                                            break;

                                        case MoveToDirections.BottomLeft:
                                            window.Top = floatingSizeAndPosition.Top + yAdjustmentToBottom;
                                            break;
                                    }
                                    window.Height = floatingSizeAndPosition.Height;
                                    window.Width = floatingSizeAndPosition.Width;
                                    adjustment = true;
                                    break;
                            }
                            break;
                    }
                    break;

                case WindowStates.Floating:
                    switch (direction) //Handle horizontal adjustment
                    {
                        case MoveToDirections.Left:
                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.TopLeft:
                            if (xAdjustmentAmount > xAdjustmentToLeft)
                            {
                                saveWindowState(WindowStates.Docked);
                                saveLastNonMaximisedState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Left);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Left -= xAdjustmentToLeft;
                            }
                            break;

                        case MoveToDirections.Right:
                        case MoveToDirections.BottomRight:
                        case MoveToDirections.TopRight:
                            if (xAdjustmentAmount > xAdjustmentToRight)
                            {
                                saveWindowState(WindowStates.Docked);
                                saveLastNonMaximisedState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Right);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Left += xAdjustmentToRight;
                            }
                            break;
                    }
                    switch (direction) //Handle vertical adjustment
                    {
                        case MoveToDirections.Bottom:
                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.BottomRight:
                            if (yAdjustmentAmount > yAdjustmentToBottom)
                            {
                                saveWindowState(WindowStates.Docked);
                                saveLastNonMaximisedState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Bottom);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Top += yAdjustmentToBottom;
                            }
                            break;

                        case MoveToDirections.Top:
                        case MoveToDirections.TopLeft:
                        case MoveToDirections.TopRight:
                            if (yAdjustmentAmount > yAdjustmentToTop)
                            {
                                saveWindowState(WindowStates.Docked);
                                saveLastNonMaximisedState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Top);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Top -= yAdjustmentToTop;
                            }
                            break;
                    }
                    adjustment = true;
                    break;
            }
            return adjustment;
        }

        private bool MoveToEdge(MoveToDirections direction, WindowStates windowState,
            double distanceToLeftBoundaryIfFloating, double distanceToRightBoundaryIfFloating,
            double distanceToBottomBoundaryIfFloating, double distanceToTopBoundaryIfFloating)
        {
            bool adjustment = false;
            switch (windowState)
            {
                case WindowStates.Docked:
                    //Jump to (and dock on) a different edge
                    var dockPosition = getDockPosition();
                    if (direction == MoveToDirections.Top && dockPosition != DockEdges.Top)
                    {
                        saveDockPosition(DockEdges.Top);
                        adjustment = true;
                    }
                    else if (direction == MoveToDirections.Bottom && dockPosition != DockEdges.Bottom)
                    {
                        saveDockPosition(DockEdges.Bottom);
                        adjustment = true;
                    }
                    else if (direction == MoveToDirections.Left && dockPosition != DockEdges.Left)
                    {
                        saveDockPosition(DockEdges.Left);
                        adjustment = true;
                    }
                    else if (direction == MoveToDirections.Right && dockPosition != DockEdges.Right)
                    {
                        saveDockPosition(DockEdges.Right);
                        adjustment = true;
                    }
                    break;

                case WindowStates.Floating:
                    //Jump to edge(s)
                    switch (direction) //Handle horizontal adjustment
                    {
                        case MoveToDirections.Left:
                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.TopLeft:
                            window.Left -= distanceToLeftBoundaryIfFloating;
                            break;

                        case MoveToDirections.Right:
                        case MoveToDirections.BottomRight:
                        case MoveToDirections.TopRight:
                            window.Left += distanceToRightBoundaryIfFloating;
                            break;
                    }
                    switch (direction) //Handle vertical adjustment
                    {
                        case MoveToDirections.Bottom:
                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.BottomRight:
                            window.Top += distanceToBottomBoundaryIfFloating;
                            break;

                        case MoveToDirections.Top:
                        case MoveToDirections.TopLeft:
                        case MoveToDirections.TopRight:
                            window.Top -= distanceToTopBoundaryIfFloating;
                            break;
                    }
                    adjustment = true;
                    break;
            }
            return adjustment;
        }

        private void RestoreState()
        {
            //Coerce state
            var fullDockThicknessAsPercentageOfScreen = getFullDockThicknessAsPercentageOfScreen();
            if(fullDockThicknessAsPercentageOfScreen <= 0 || fullDockThicknessAsPercentageOfScreen >= 100)
            {
                fullDockThicknessAsPercentageOfScreen = 50;
                saveFullDockThicknessAsPercentageOfScreen(fullDockThicknessAsPercentageOfScreen);
            }
            double collapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness();
            if (collapsedDockThicknessAsPercentageOfFullDockThickness <= 0 || collapsedDockThicknessAsPercentageOfFullDockThickness >= 100)
            {
                collapsedDockThicknessAsPercentageOfFullDockThickness = 20;
                saveCollapsedDockThicknessAsPercentageOfFullDockThickness(collapsedDockThicknessAsPercentageOfFullDockThickness);
            }
            Rect floatingSizeAndPosition = getFloatingSizeAndPosition();
            if (floatingSizeAndPosition == default(Rect) ||
                floatingSizeAndPosition.Left < screenBoundsInDp.Left ||
                floatingSizeAndPosition.Right > screenBoundsInDp.Right ||
                floatingSizeAndPosition.Top < screenBoundsInDp.Top ||
                floatingSizeAndPosition.Bottom > screenBoundsInDp.Bottom)
            {
                //Default to two-thirds of the screen's width and height, positioned centrally
                floatingSizeAndPosition = new Rect(
                    screenBoundsInDp.Left + screenBoundsInDp.Width / 6, 
                    screenBoundsInDp.Top + screenBoundsInDp.Height / 6,
                    2 * (screenBoundsInDp.Width / 3), 2 * (screenBoundsInDp.Height / 3));
                saveFloatingSizeAndPosition(floatingSizeAndPosition);
            }

            //Apply state to window
            var windowState = getWindowState();
            window.Opacity = getOpacity();
            window.WindowState = windowState.ToWindowState();
            RestoreSizeAndPosition(windowState, floatingSizeAndPosition);
        }

        private void PersistDockThickness()
        {
            var dockPosition = getDockPosition();
            switch (getDockSize())
            {
                case DockSizes.Full:
                    var fullDockThicknessAsPercentageOfScreen =
                        dockPosition == DockEdges.Top || dockPosition == DockEdges.Bottom
                            ? (window.ActualHeight / screenBoundsInDp.Height) * 100
                            : (window.ActualWidth / screenBoundsInDp.Width) * 100;
                    saveFullDockThicknessAsPercentageOfScreen(fullDockThicknessAsPercentageOfScreen);
                    break;

                case DockSizes.Collapsed:
                    var collapsedDockThicknessAsPercentageOfFullDockThickness =
                        dockPosition == DockEdges.Top || dockPosition == DockEdges.Bottom
                            ? (window.ActualHeight / screenBoundsInDp.Height) * getFullDockThicknessAsPercentageOfScreen()
                            : (window.ActualWidth / screenBoundsInDp.Width) * getFullDockThicknessAsPercentageOfScreen();
                    saveCollapsedDockThicknessAsPercentageOfFullDockThickness(
                        collapsedDockThicknessAsPercentageOfFullDockThickness);
                    break;
            }
        }

        private void PersistSizeAndPosition()
        {
            var windowState = getWindowState();
            switch (windowState)
            {
                case WindowStates.Floating:
                    saveFloatingSizeAndPosition(new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight));
                    break;

                case WindowStates.Docked:
                    PersistDockThickness();
                    break;
            }
        }

        private void RestoreSizeAndPosition(WindowStates windowState, Rect floatingSizeAndPosition)
        {
            if (windowState == WindowStates.Docked)
            {
                var dockPosition = getDockPosition();
                var dockSize = getDockSize();
                var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(dockPosition, dockSize);
                RegisterAppBar();
                SetAppBarSizeAndPosition(dockPosition, dockSizeAndPositionInPx);
            }
            else if (windowState == WindowStates.Floating)
            {
                window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ApplySizeAndPositionDelegate(ApplySizeAndPosition), floatingSizeAndPosition);
            }
        }

        private void RegisterAppBar()
        {
            if (getWindowState() != WindowStates.Docked) return;

            //Register a new app bar with Windows - this adds it to a list of app bars
            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = windowHandle;
            appBarCallBackId = PInvoke.RegisterWindowMessage("AppBarMessage"); //Get a system wide unique window message (id)
            abd.uCallbackMessage = appBarCallBackId;
            var result = PInvoke.SHAppBarMessage((int)AppBarMessages.New, ref abd);

            //Add hook to receive position change messages from Windows
            HwndSource source = HwndSource.FromHwnd(abd.hWnd);
            source.AddHook(AppBarPositionChangeCallback);
        }

        private void SetAppBarSizeAndPosition(DockEdges dockPosition, Rect sizeAndPosition)
        {
            if (getWindowState() != WindowStates.Docked) return;

            var barData = new APPBARDATA();
            barData.cbSize = Marshal.SizeOf(barData);
            barData.hWnd = windowHandle;
            barData.uEdge = dockPosition.ToAppBarEdge();
            barData.rc.Left = (int)Math.Round(sizeAndPosition.Left);
            barData.rc.Top = (int)Math.Round(sizeAndPosition.Top);
            barData.rc.Right = (int)Math.Round(sizeAndPosition.Right);
            barData.rc.Bottom = (int)Math.Round(sizeAndPosition.Bottom);
            
            //Submit a query for the proposed dock size and position, which might be updated
            PInvoke.SHAppBarMessage(AppBarMessages.QueryPos, ref barData);

            //Compensate for lost thickness due to other app bars
            switch (dockPosition)
            {
                case DockEdges.Top:
                    barData.rc.Bottom += barData.rc.Top - (int) Math.Round(sizeAndPosition.Top);
                    break;
                case DockEdges.Bottom:
                    barData.rc.Top -= (int)Math.Round(sizeAndPosition.Bottom) - barData.rc.Bottom;
                    break;
                case DockEdges.Left:
                    barData.rc.Right += barData.rc.Left - (int)Math.Round(sizeAndPosition.Left);
                    break;
                case DockEdges.Right:
                    barData.rc.Left -= (int)Math.Round(sizeAndPosition.Right) - barData.rc.Right;
                    break;
            }
            
            //Then set the dock size and position, using the potentially updated barData
            PInvoke.SHAppBarMessage(AppBarMessages.SetPos, ref barData);

            //Extract the final size and position and apply to the window. This is dispatched with ApplicationIdle priority 
            //as WPF will send a resize after a new appbar is added. We need to apply the received size & position after this happens.
            //RECT values are in pixels so I need to scale back to DIPs for WPF.
            var rect = new Rect(
                barData.rc.Left / Graphics.DipScalingFactorX, 
                barData.rc.Top / Graphics.DipScalingFactorY,
                (barData.rc.Right - barData.rc.Left) / Graphics.DipScalingFactorX, 
                (barData.rc.Bottom - barData.rc.Top) / Graphics.DipScalingFactorY);
            window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ApplySizeAndPositionDelegate(ApplySizeAndPosition), rect);
        }

        private void UnRegisterAppBar()
        {
            if (getWindowState() != WindowStates.Docked) return;

            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = windowHandle;
            PInvoke.SHAppBarMessage(AppBarMessages.Remove, ref abd);
            appBarCallBackId = -1;
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion
    }
}
