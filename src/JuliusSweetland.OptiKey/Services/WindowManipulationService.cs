using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Common.Enums;
using JuliusSweetland.OptiKey.Native.Common.Structs;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService
    {
        #region Constants

        private const double MIN_FULL_DOCK_THICKNESS_AS_PERCENTAGE_OF_SCREEN = 10;
        private const double MIN_COLLAPSED_DOCK_THICKNESS_AS_PERCENTAGE_OF_FULL_DOCK_THICKNESS = 10;
        private const double MIN_FLOATING_WIDTH_AS_PERCENTAGE_OF_SCREEN = 10;
        private const double MIN_FLOATING_HEIGHT_AS_PERCENTAGE_OF_SCREEN = 10;

        #endregion

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;
        private readonly IntPtr windowHandle;
        private Screen screen;
        private Rect screenBoundsInPx;
        private Rect screenBoundsInDp;
        private readonly Func<double> getOpacity;
        private readonly Func<WindowStates> getWindowState;
        private readonly Func<WindowStates> getPreviousWindowState;
        private readonly Func<Rect> getFloatingSizeAndPosition;
        private readonly Func<DockEdges> getDockPosition;
        private readonly Func<DockSizes> getDockSize;
        private readonly Func<double> getFullDockThicknessAsPercentageOfScreen;
        private readonly Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Func<MinimisedEdges> getMinimisedPosition;
        private readonly Action<WindowStates> saveWindowState;
        private readonly Action<WindowStates> savePreviousWindowState;
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
            Func<WindowStates> getPreviousWindowState,
            Func<Rect> getFloatingSizeAndPosition,
            Func<DockEdges> getDockPosition,
            Func<DockSizes> getDockSize,
            Func<double> getFullDockThicknessAsPercentageOfScreen,
            Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness,
            Func<MinimisedEdges> getMinimisedPosition,
            Action<double> saveOpacity,
            Action<WindowStates> saveWindowState,
            Action<WindowStates> savePreviousWindowState,
            Action<Rect> saveFloatingSizeAndPosition,
            Action<DockEdges> saveDockPosition,
            Action<DockSizes> saveDockSize,
            Action<double> saveFullDockThicknessAsPercentageOfScreen,
            Action<double> saveCollapsedDockThicknessAsPercentageOfFullDockThickness)
        {
            this.window = window;
            this.getOpacity = getOpacity;
            this.getWindowState = getWindowState;
            this.getPreviousWindowState = getPreviousWindowState;
            this.getDockPosition = getDockPosition;
            this.getDockSize = getDockSize;
            this.getFullDockThicknessAsPercentageOfScreen = getFullDockThicknessAsPercentageOfScreen;
            this.getCollapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness;
            this.getMinimisedPosition = getMinimisedPosition;
            this.getFloatingSizeAndPosition = getFloatingSizeAndPosition;
            this.saveOpacity = saveOpacity;
            this.saveWindowState = saveWindowState;
            this.savePreviousWindowState = savePreviousWindowState;
            this.saveFloatingSizeAndPosition = saveFloatingSizeAndPosition;
            this.saveDockPosition = saveDockPosition;
            this.saveDockSize = saveDockSize;
            this.saveFullDockThicknessAsPercentageOfScreen = saveFullDockThicknessAsPercentageOfScreen;
            this.saveCollapsedDockThicknessAsPercentageOfFullDockThickness = saveCollapsedDockThicknessAsPercentageOfFullDockThickness;

            windowHandle = new WindowInteropHelper(window).EnsureHandle();
            screen = window.GetScreen();
            screenBoundsInPx = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height);
            Log.DebugFormat("Screen bounds in Px: {0}", screenBoundsInPx);
            var screenBoundsTopLeftInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.TopLeft);
            var screenBoundsBottomRightInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.BottomRight);
            screenBoundsInDp = new Rect(screenBoundsTopLeftInDp.X, screenBoundsTopLeftInDp.Y,
                screenBoundsBottomRightInDp.X - screenBoundsTopLeftInDp.X,
                screenBoundsBottomRightInDp.Y - screenBoundsTopLeftInDp.Y);
            Log.DebugFormat("Screen bounds in Dp: {0}", screenBoundsInDp);

            CoerceSavedStateAndApply();
        
            window.Closed += (_, __) => UnRegisterAppBar();
        }

        #endregion
        
        #region Events

        public event EventHandler SizeAndPositionInitialised;
        public event EventHandler<Exception> Error;

        #endregion

        #region Properties

        public bool SizeAndPositionIsInitialised { get; private set; }
        public WindowStates WindowState { get { return getWindowState(); } }

        #endregion

        #region Public Methods

        public void Expand(ExpandToDirections direction, double amountInPx)
        {
            Log.InfoFormat("Expand called with direction {0} and amount (px) {1}", direction, amountInPx);

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

                    //Recalculate distance to boundaries and check if we are now aligned with 3 edges
                    distanceToBottomBoundary = screenBoundsInDp.Bottom - (window.Top + window.ActualHeight);
                    distanceToTopBoundary = window.Top - screenBoundsInDp.Top;
                    distanceToLeftBoundary = window.Left - screenBoundsInDp.Left;
                    distanceToRightBoundary = screenBoundsInDp.Right - (window.Left + window.ActualWidth);

                    DockEdges? dockToEdge = null;
                    if (distanceToTopBoundary == 0 && distanceToLeftBoundary == 0 && distanceToRightBoundary == 0)
                    {
                        dockToEdge = DockEdges.Top;
                    }
                    else if (distanceToBottomBoundary == 0 && distanceToLeftBoundary == 0 && distanceToRightBoundary == 0)
                    {
                        dockToEdge = DockEdges.Bottom;
                    }
                    else if (distanceToTopBoundary == 0 && distanceToLeftBoundary == 0 && distanceToBottomBoundary == 0)
                    {
                        dockToEdge = DockEdges.Left;
                    }
                    else if (distanceToTopBoundary == 0 && distanceToRightBoundary == 0 && distanceToBottomBoundary == 0)
                    {
                        dockToEdge = DockEdges.Right;
                    }

                    if (dockToEdge != null)
                    {
                        //We are aligned with 3 edges and currently floating, so switch to docked mode
                        saveWindowState(WindowStates.Docked);
                        savePreviousWindowState(WindowStates.Docked);
                        saveDockPosition(dockToEdge.Value);
                        RegisterAppBar();
                        var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                        SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
                    }
                    else
                    {
                        PersistSizeAndPosition();
                    }
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

        public double GetOpacity()
        {
            return window.Opacity;
        }

        public void Hide()
        {
            Log.Info("Hide called");

            var windowState = getWindowState();
            if (windowState != WindowStates.Hidden)
            {
                savePreviousWindowState(windowState);
            }
            if (getWindowState() == WindowStates.Docked)
            {
                UnRegisterAppBar();
            }
            saveWindowState(WindowStates.Hidden);
            ApplySavedState();
        }

        public void IncrementOrDecrementOpacity(bool increment)
        {
            Log.InfoFormat("IncrementOrDecrementOpacity called with increment {0}", increment);

            var opacity = window.Opacity;
            opacity += increment ? 0.1 : -0.1;
            opacity = opacity.CoerceToLowerLimit(0.1);
            opacity = opacity.CoerceToUpperLimit(1);
            window.Opacity = opacity;
            saveOpacity(window.Opacity);
        }

        public void Maximise()
        {
            Log.Info("Maximise called");

            var windowState = getWindowState();
            if (windowState != WindowStates.Maximised)
            {
                savePreviousWindowState(windowState);
            }
            if (getWindowState() == WindowStates.Docked)
            {
                UnRegisterAppBar();
            }
            saveWindowState(WindowStates.Maximised);
            ApplySavedState();
        }

        public void Minimise()
        {
            Log.Info("Minimise called");

            var windowState = getWindowState();
            if (windowState != WindowStates.Minimised)
            {
                savePreviousWindowState(windowState);
            }
            if (getWindowState() == WindowStates.Docked)
            {
                UnRegisterAppBar();
            }
            saveWindowState(WindowStates.Minimised);
            ApplySavedState();
        }

        public void Move(MoveToDirections direction, double? amountInPx)
        {
            Log.InfoFormat("Move called with direction {0} and amount (px) {1}", direction, amountInPx);

            var windowState = getWindowState();
            if (windowState == WindowStates.Maximised) return;

            var floatingSizeAndPosition = getFloatingSizeAndPosition();
            var distanceToTopBoundaryIfFloating = floatingSizeAndPosition.Top - screenBoundsInDp.Top;
            var distanceToBottomBoundaryIfFloating = screenBoundsInDp.Bottom - (floatingSizeAndPosition.Top + floatingSizeAndPosition.Height);
            var distanceToLeftBoundaryIfFloating = floatingSizeAndPosition.Left - screenBoundsInDp.Left;
            var distanceToRightBoundaryIfFloating = screenBoundsInDp.Right - (floatingSizeAndPosition.Left + floatingSizeAndPosition.Width);

            bool adjustment;
            if (amountInPx != null)
            {
                adjustment = Move(direction, amountInPx.Value, distanceToTopBoundaryIfFloating,
                    distanceToBottomBoundaryIfFloating, distanceToLeftBoundaryIfFloating, distanceToRightBoundaryIfFloating,
                    windowState, floatingSizeAndPosition);
            }
            else
            {
                adjustment = MoveToEdge(direction, windowState, distanceToTopBoundaryIfFloating, 
                    distanceToBottomBoundaryIfFloating, distanceToLeftBoundaryIfFloating, distanceToRightBoundaryIfFloating);
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
            Log.Info("ResizeDockToCollapsed called");

            if (getWindowState() != WindowStates.Docked) return;
            saveDockSize(DockSizes.Collapsed);
            var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Collapsed);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void ResizeDockToFull()
        {
            Log.Info("ResizeDockToFull called");

            if (getWindowState() != WindowStates.Docked) return;
            saveDockSize(DockSizes.Full); 
            var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Full);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void Restore()
        {
            Log.Info("Restore called");

            var windowState = getWindowState();
            if (windowState != WindowStates.Maximised && windowState != WindowStates.Minimised && windowState != WindowStates.Hidden) return;
            saveWindowState(getPreviousWindowState()); 
            ApplySavedState();
            savePreviousWindowState(windowState);
        }

        public void SetOpacity(double opacity)
        {
            Log.InfoFormat("SetOpacity called with opacity {0}", opacity);

            opacity = opacity.CoerceToLowerLimit(0.1);
            opacity = opacity.CoerceToUpperLimit(1);
            window.Opacity = opacity;
            saveOpacity(window.Opacity);
        }

        public void Shrink(ShrinkFromDirections direction, double amountInPx)
        {
            Log.InfoFormat("Shrink called with direction {0} and amount (px) {1}", direction, amountInPx);

            var windowState = getWindowState();
            if (windowState == WindowStates.Maximised) return;

            var distanceToBottomBoundary = screenBoundsInDp.Bottom - (window.Top + window.ActualHeight);
            var yAdjustment = amountInPx / Graphics.DipScalingFactorY;
            var yAdjustmentFromBottom = distanceToBottomBoundary < 0 
                ? distanceToBottomBoundary
                : 0 - yAdjustment;
            var distanceToTopBoundary = window.Top - screenBoundsInDp.Top;
            var yAdjustmentFromTop = distanceToTopBoundary < 0 ? distanceToTopBoundary : 0 - yAdjustment;
            
            var distanceToLeftBoundary = window.Left - screenBoundsInDp.Left;
            var xAdjustment = amountInPx / Graphics.DipScalingFactorX;
            var xAdjustmentFromLeft = distanceToLeftBoundary < 0 
                ? distanceToLeftBoundary
                : 0 - xAdjustment;
            var distanceToRightBoundary = screenBoundsInDp.Right - (window.Left + window.ActualWidth);
            var xAdjustmentFromRight = distanceToRightBoundary < 0 ? distanceToRightBoundary : 0 - xAdjustment;

            switch (windowState)
            {
                case WindowStates.Floating:
                    var maxFloatingHeightAdjustment = window.Height - ((MIN_FLOATING_HEIGHT_AS_PERCENTAGE_OF_SCREEN / 100) * screenBoundsInDp.Height);
                    switch (direction) //Handle vertical adjustment
                    {
                        case ShrinkFromDirections.Bottom:
                        case ShrinkFromDirections.BottomLeft:
                        case ShrinkFromDirections.BottomRight:
                            yAdjustmentFromBottom = yAdjustmentFromBottom.CoerceToLowerLimit(0 - maxFloatingHeightAdjustment);
                            window.Height += yAdjustmentFromBottom;
                            break;

                        case ShrinkFromDirections.Top:
                        case ShrinkFromDirections.TopLeft:
                        case ShrinkFromDirections.TopRight:
                            var heightBeforeAdjustment = window.ActualHeight;
                            yAdjustmentFromTop = yAdjustmentFromTop.CoerceToLowerLimit(0 - maxFloatingHeightAdjustment);
                            window.Height += yAdjustmentFromTop;
                            var actualYAdjustmentToTop = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                            window.Top -= actualYAdjustmentToTop;
                            break;
                    }
                    var maxFloatingWidthAdjustment = window.Width - ((MIN_FLOATING_WIDTH_AS_PERCENTAGE_OF_SCREEN / 100) * screenBoundsInDp.Width);
                    switch (direction) //Handle horizontal adjustment
                    {
                        case ShrinkFromDirections.Left:
                        case ShrinkFromDirections.BottomLeft:
                        case ShrinkFromDirections.TopLeft:
                            var widthBeforeAdjustment = window.ActualWidth;
                            xAdjustmentFromLeft = xAdjustmentFromLeft.CoerceToLowerLimit(0 - maxFloatingWidthAdjustment);
                            window.Width += xAdjustmentFromLeft;
                            var actualXAdjustmentToLeft = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                            window.Left -= actualXAdjustmentToLeft;
                            break;

                        case ShrinkFromDirections.Right:
                        case ShrinkFromDirections.BottomRight:
                        case ShrinkFromDirections.TopRight:
                            xAdjustmentFromRight = xAdjustmentFromRight.CoerceToLowerLimit(0 - maxFloatingWidthAdjustment);
                            window.Width += xAdjustmentFromRight;
                            break;
                    }
                    PersistSizeAndPosition();
                    break;

                case WindowStates.Docked:
                    var dockPosition = getDockPosition();
                    var dockSize = getDockSize();
                    var adjustment = false;
                    var maxFullDockHeightAdjustment = window.Height - ((MIN_FULL_DOCK_THICKNESS_AS_PERCENTAGE_OF_SCREEN / 100) * screenBoundsInDp.Height);
                    var maxFullDockWidthAdjustment = window.Width - ((MIN_FULL_DOCK_THICKNESS_AS_PERCENTAGE_OF_SCREEN / 100) * screenBoundsInDp.Width);
                    var maxCollapsedDockHeightAdjustment = window.Height - ((MIN_COLLAPSED_DOCK_THICKNESS_AS_PERCENTAGE_OF_FULL_DOCK_THICKNESS / 100) * ((getCollapsedDockThicknessAsPercentageOfFullDockThickness() / 100) * screenBoundsInDp.Height));
                    var maxCollapsedDockWidthAdjustment = window.Width - ((MIN_COLLAPSED_DOCK_THICKNESS_AS_PERCENTAGE_OF_FULL_DOCK_THICKNESS / 100) * ((getCollapsedDockThicknessAsPercentageOfFullDockThickness() / 100) * screenBoundsInDp.Width));
                    if (dockPosition == DockEdges.Top &&
                        (direction == ShrinkFromDirections.Bottom || direction == ShrinkFromDirections.BottomLeft || direction == ShrinkFromDirections.BottomRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            yAdjustmentFromBottom = yAdjustmentFromBottom.CoerceToLowerLimit(0 - maxFullDockHeightAdjustment);
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualHeight + yAdjustmentFromBottom) / screenBoundsInDp.Height) * 100);
                        }
                        else
                        {
                            yAdjustmentFromBottom = yAdjustmentFromBottom.CoerceToLowerLimit(0 - maxCollapsedDockHeightAdjustment);
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualHeight + yAdjustmentFromBottom) / screenBoundsInDp.Height) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Bottom &&
                        (direction == ShrinkFromDirections.Top || direction == ShrinkFromDirections.TopLeft || direction == ShrinkFromDirections.TopRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            yAdjustmentFromTop = yAdjustmentFromTop.CoerceToLowerLimit(0 - maxFullDockHeightAdjustment);
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualHeight + yAdjustmentFromTop) / screenBoundsInDp.Height) * 100);
                        }
                        else
                        {
                            yAdjustmentFromTop = yAdjustmentFromTop.CoerceToLowerLimit(0 - maxCollapsedDockHeightAdjustment);
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualHeight + yAdjustmentFromTop) / screenBoundsInDp.Height) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Left &&
                        (direction == ShrinkFromDirections.Right || direction == ShrinkFromDirections.TopRight || direction == ShrinkFromDirections.BottomRight))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            xAdjustmentFromRight = xAdjustmentFromRight.CoerceToLowerLimit(0 - maxFullDockWidthAdjustment);
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualWidth + xAdjustmentFromRight) / screenBoundsInDp.Width) * 100);
                        }
                        else
                        {
                            xAdjustmentFromRight = xAdjustmentFromRight.CoerceToLowerLimit(0 - maxCollapsedDockWidthAdjustment);
                            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(((window.ActualWidth + xAdjustmentFromRight) / screenBoundsInDp.Width) * getFullDockThicknessAsPercentageOfScreen());
                        }
                        adjustment = true;
                    }
                    else if (dockPosition == DockEdges.Right &&
                        (direction == ShrinkFromDirections.Left || direction == ShrinkFromDirections.TopLeft || direction == ShrinkFromDirections.BottomLeft))
                    {
                        if (dockSize == DockSizes.Full)
                        {
                            xAdjustmentFromLeft = xAdjustmentFromLeft.CoerceToLowerLimit(0 - maxFullDockWidthAdjustment);
                            saveFullDockThicknessAsPercentageOfScreen(((window.ActualWidth + xAdjustmentFromLeft) / screenBoundsInDp.Width) * 100);
                        }
                        else
                        {
                            xAdjustmentFromLeft = xAdjustmentFromLeft.CoerceToLowerLimit(0 - maxCollapsedDockWidthAdjustment);
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
                    Log.Info("AppBarPositionChangeCallback called with PositionChanged message.");
                    var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                    SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void ApplyAndPersistSizeAndPosition(Rect rect)
        {
            Log.InfoFormat("ApplyAndPersistSizeAndPosition called with rect.Top:{0}, rect.Bottom:{1}, rect.Left:{2}, rect.Right:{3}", 
                rect.Top, rect.Bottom, rect.Left, rect.Right);

            window.Top = rect.Top;
            window.Left = rect.Left;
            window.Width = rect.Width;
            window.Height = rect.Height;
            
            PersistSizeAndPosition();
            PublishSizeAndPositionInitialised();
        }

        private void ApplySavedState(bool isInitialising = false)
        {
            Log.Info("ApplySavedState called");

            var windowState = getWindowState();
            window.Opacity = getOpacity();
            var dockPosition = getDockPosition();
            switch (windowState)
            {
                case WindowStates.Docked:
                    window.WindowState = System.Windows.WindowState.Normal;
                    var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(dockPosition, getDockSize());
                    RegisterAppBar();
                    SetAppBarSizeAndPosition(dockPosition, dockSizeAndPositionInPx, isInitialising);
                    break;

                case WindowStates.Floating:
                    window.WindowState = System.Windows.WindowState.Normal;
                    window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                        new ApplySizeAndPositionDelegate(ApplyAndPersistSizeAndPosition), getFloatingSizeAndPosition());
                    break;

                case WindowStates.Maximised:
                    window.WindowState = System.Windows.WindowState.Maximized;
                    PublishSizeAndPositionInitialised();
                    break;

                case WindowStates.Minimised:
                    window.WindowState = System.Windows.WindowState.Normal;
                    var minimisedSizeAndPosition = CalculateMinimisedSizeAndPosition();
                    window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                        new ApplySizeAndPositionDelegate(ApplyAndPersistSizeAndPosition), minimisedSizeAndPosition);
                    break;

                case WindowStates.Hidden:
                    window.WindowState = System.Windows.WindowState.Minimized;
                    break;
            }
        }

        private Rect CalculateDockSizeAndPositionInPx(DockEdges position, DockSizes size)
        {
            Log.InfoFormat("CalculateDockSizeAndPositionInPx called with position:{0}, size:{1}", position, size);

            double x, y, width, height;
            var thicknessAsPercentage = size == DockSizes.Full
                ? getFullDockThicknessAsPercentageOfScreen() / 100
                : (getFullDockThicknessAsPercentageOfScreen() * getCollapsedDockThicknessAsPercentageOfFullDockThickness()) / 10000; //Percentage of a percentage

            switch (position)
            {
                case DockEdges.Top:
                    x = screenBoundsInPx.X;
                    y = screenBoundsInPx.Y;
                    width = screenBoundsInPx.Width;
                    height = screenBoundsInPx.Height * thicknessAsPercentage;
                    break;

                case DockEdges.Bottom:
                    x = screenBoundsInPx.X;
                    y = screenBoundsInPx.Y + screenBoundsInPx.Height - (screenBoundsInPx.Height * thicknessAsPercentage);
                    width = screenBoundsInPx.Width;
                    height = screenBoundsInPx.Height * thicknessAsPercentage;
                    break;

                case DockEdges.Left:
                    x = screenBoundsInPx.X;
                    y = screenBoundsInPx.Y;
                    width = screenBoundsInPx.Width * thicknessAsPercentage;
                    height = screenBoundsInPx.Height;
                    break;

                default: //case DockEdges.Right:
                    x = screenBoundsInPx.X + screenBoundsInPx.Width - (screenBoundsInPx.Width * thicknessAsPercentage);
                    y = screenBoundsInPx.Y;
                    width = screenBoundsInPx.Width * thicknessAsPercentage;
                    height = screenBoundsInPx.Height;
                    break;
            }
            
            return new Rect(x, y, width, height);
        }

        private Rect CalculateMinimisedSizeAndPosition()
        {
            Log.Info("CalculateMinimisedSizeAndPosition called.");

            double x, y;
            var thicknessAsPercentage = (getFullDockThicknessAsPercentageOfScreen() * getCollapsedDockThicknessAsPercentageOfFullDockThickness()) / 10000; //Percentage of a percentage
            var height = screenBoundsInPx.Height * thicknessAsPercentage;
            var width = screenBoundsInPx.Width * thicknessAsPercentage;

            var minimisedEdge = getMinimisedPosition();
            switch (minimisedEdge == MinimisedEdges.SameAsDockedPosition ? getDockPosition().ToMinimisedEdge() : minimisedEdge)
            {
                case MinimisedEdges.Top:
                    if (screenBoundsInDp.Height > screenBoundsInDp.Width)
                    {
                        //Ensure the minimise button's long edge is against the docked edge,
                        //so swap width and height if aspect ratio is taller than it is wide
                        var temp = width;
                        width = height;
                        height = temp;
                    }
                    x = screenBoundsInDp.Left + (screenBoundsInDp.Width / 2) - (width / 2);
                    y = screenBoundsInDp.Top;
                    break;

                case MinimisedEdges.Bottom:
                    if (screenBoundsInDp.Height > screenBoundsInDp.Width)
                    {
                        //Ensure the minimise button's long edge is against the docked edge,
                        //so swap width and height if aspect ratio is taller than it is wide
                        var temp = width;
                        width = height;
                        height = temp;
                    }
                    x = screenBoundsInDp.Left + (screenBoundsInDp.Width / 2) - (width / 2);
                    y = screenBoundsInDp.Bottom - height;
                    break;

                case MinimisedEdges.Left:
                    if (screenBoundsInDp.Width > screenBoundsInDp.Height)
                    {
                        //Ensure the minimise button's long edge is against the docked edge,
                        //so swap width and height if aspect ratio is wider than it is high
                        var temp = width;
                        width = height;
                        height = temp;
                    }
                    x = screenBoundsInDp.Left;
                    y = screenBoundsInDp.Top + (screenBoundsInDp.Height / 2) - (height / 2);
                    break;

                default: //case DockEdges.Right:
                    if (screenBoundsInDp.Width > screenBoundsInDp.Height)
                    {
                        //Ensure the minimise button's long edge is against the docked edge,
                        //so swap width and height if aspect ratio is wider than it is high
                        var temp = width;
                        width = height;
                        height = temp;
                    }
                    x = screenBoundsInDp.Right - width;
                    y = screenBoundsInDp.Top + (screenBoundsInDp.Height / 2) - (height / 2);
                    break;
            }

            return new Rect(x, y, width, height);
        }

        private void CoerceSavedStateAndApply()
        {
            Log.Info("CoerceSavedStateAndApply called.");

            var windowState = getWindowState();
            if (windowState != WindowStates.Maximised
                && windowState != WindowStates.Minimised
                && windowState != WindowStates.Hidden)
            {
                //Coerce state
                var fullDockThicknessAsPercentageOfScreen = getFullDockThicknessAsPercentageOfScreen();
                if (fullDockThicknessAsPercentageOfScreen < MIN_FULL_DOCK_THICKNESS_AS_PERCENTAGE_OF_SCREEN 
                    || fullDockThicknessAsPercentageOfScreen >= 100)
                {
                    Log.WarnFormat("Saved full cked thickness of {0} is invalid. Restoring to default.", fullDockThicknessAsPercentageOfScreen);
                    fullDockThicknessAsPercentageOfScreen = 50;
                    saveFullDockThicknessAsPercentageOfScreen(fullDockThicknessAsPercentageOfScreen);
                }
                double collapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness();
                if (collapsedDockThicknessAsPercentageOfFullDockThickness < MIN_COLLAPSED_DOCK_THICKNESS_AS_PERCENTAGE_OF_FULL_DOCK_THICKNESS 
                    || collapsedDockThicknessAsPercentageOfFullDockThickness >= 100)
                {
                    Log.WarnFormat("Saved collased docked thickness of {0} is invalid. Restoring to default.", collapsedDockThicknessAsPercentageOfFullDockThickness);
                    collapsedDockThicknessAsPercentageOfFullDockThickness = 20;
                    saveCollapsedDockThicknessAsPercentageOfFullDockThickness(collapsedDockThicknessAsPercentageOfFullDockThickness);
                }
                Rect floatingSizeAndPosition = getFloatingSizeAndPosition();
                if (floatingSizeAndPosition == default(Rect) ||
                    floatingSizeAndPosition.Left < screenBoundsInDp.Left ||
                    floatingSizeAndPosition.Right > screenBoundsInDp.Right ||
                    floatingSizeAndPosition.Top < screenBoundsInDp.Top ||
                    floatingSizeAndPosition.Bottom > screenBoundsInDp.Bottom ||
                    floatingSizeAndPosition.Width < (screenBoundsInDp.Width * (MIN_FLOATING_WIDTH_AS_PERCENTAGE_OF_SCREEN / 100)) ||
                    floatingSizeAndPosition.Height < (screenBoundsInDp.Height * (MIN_FLOATING_HEIGHT_AS_PERCENTAGE_OF_SCREEN / 100)))
                {
                    //Default to two-thirds of the screen's width and height, positioned centrally
                    Log.WarnFormat("Saved floating size and position was invalid (Top:{0}, Bottom:{1}, Left:{2}, Right:{3}, Width:{4}, Height:{5}). Restoring to default.",
                        floatingSizeAndPosition.Top, floatingSizeAndPosition.Bottom, floatingSizeAndPosition.Left, floatingSizeAndPosition.Right, floatingSizeAndPosition.Width, floatingSizeAndPosition.Height);
                    floatingSizeAndPosition = new Rect(
                        screenBoundsInDp.Left + screenBoundsInDp.Width / 6,
                        screenBoundsInDp.Top + screenBoundsInDp.Height / 6,
                        2 * (screenBoundsInDp.Width / 3), 2 * (screenBoundsInDp.Height / 3));
                    saveFloatingSizeAndPosition(floatingSizeAndPosition);
                }    
            }

            ApplySavedState(true);
        }

        private bool Move(MoveToDirections direction, double amountInPx, double distanceToTopBoundaryIfFloating,
            double distanceToBottomBoundaryIfFloating, double distanceToLeftBoundaryIfFloating,
            double distanceToRightBoundaryIfFloating, WindowStates windowState, Rect floatingSizeAndPosition)
        {
            Log.InfoFormat("Move called with direction:{0}, amountInPx:{1}, distanceToTopBoundaryIfFloating:{2}, distanceToBottomBoundaryIfFloating:{3}, distanceToLeftBoundaryIfFloating:{4}, distanceToRightBoundaryIfFloating: {5}, windowState:{6}, floatingSizeAndPosition.Top:{7}, floatingSizeAndPosition.Bottom:{8}, floatingSizeAndPosition.Left:{9}, floatingSizeAndPosition.Right:{10}",
                direction, amountInPx, distanceToTopBoundaryIfFloating, distanceToBottomBoundaryIfFloating, distanceToLeftBoundaryIfFloating, distanceToRightBoundaryIfFloating, windowState, floatingSizeAndPosition.Top, floatingSizeAndPosition.Bottom, floatingSizeAndPosition.Left, floatingSizeAndPosition.Right);

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
                                    savePreviousWindowState(WindowStates.Floating);
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
                                            window.Left = floatingSizeAndPosition.Left + xAdjustmentToRight;
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
                                    savePreviousWindowState(WindowStates.Floating);
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
                                            window.Left = floatingSizeAndPosition.Left + xAdjustmentToRight;
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
                                    savePreviousWindowState(WindowStates.Floating);
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
                                    savePreviousWindowState(WindowStates.Floating);
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
                            if (xAdjustmentAmount > xAdjustmentToLeft)
                            {
                                saveWindowState(WindowStates.Docked);
                                savePreviousWindowState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Left);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Left -= xAdjustmentToLeft;
                            }
                            break;

                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.TopLeft:
                            window.Left -= xAdjustmentToLeft;
                            break;

                        case MoveToDirections.Right:
                            if (xAdjustmentAmount > xAdjustmentToRight)
                            {
                                saveWindowState(WindowStates.Docked);
                                savePreviousWindowState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Right);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Left += xAdjustmentToRight;
                            }
                            break;

                        case MoveToDirections.BottomRight:
                        case MoveToDirections.TopRight:
                            window.Left += xAdjustmentToRight;
                            break;
                    }
                    switch (direction) //Handle vertical adjustment
                    {
                        case MoveToDirections.Bottom:
                            if (yAdjustmentAmount > yAdjustmentToBottom)
                            {
                                saveWindowState(WindowStates.Docked);
                                savePreviousWindowState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Bottom);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Top += yAdjustmentToBottom;
                            }
                            break;

                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.BottomRight:
                            window.Top += yAdjustmentToBottom;
                            break;

                        case MoveToDirections.Top:
                            if (yAdjustmentAmount > yAdjustmentToTop)
                            {
                                saveWindowState(WindowStates.Docked);
                                savePreviousWindowState(WindowStates.Docked);
                                saveDockPosition(DockEdges.Top);
                                RegisterAppBar();
                            }
                            else
                            {
                                window.Top -= yAdjustmentToTop;
                            }
                            break;

                        case MoveToDirections.TopLeft:
                        case MoveToDirections.TopRight:
                            window.Top -= yAdjustmentToTop;
                            break;
                    }
                    adjustment = true;
                    break;
            }
            return adjustment;
        }

        private bool MoveToEdge(MoveToDirections direction, WindowStates windowState,
            double distanceToTopBoundaryIfFloating, double distanceToBottomBoundaryIfFloating,
            double distanceToLeftBoundaryIfFloating, double distanceToRightBoundaryIfFloating)
        {
            Log.InfoFormat("MoveToEdge called with direction:{0}, windowState:{1}, distanceToTopBoundaryIfFloating:{2}, distanceToBottomBoundaryIfFloating:{3}, distanceToLeftBoundaryIfFloating:{4}, distanceToRightBoundaryIfFloating: {5}",
                direction, windowState, distanceToTopBoundaryIfFloating, distanceToBottomBoundaryIfFloating, distanceToLeftBoundaryIfFloating, distanceToRightBoundaryIfFloating);

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
                    //Jump to edge(s), or dock against edge if we are already against that edge
                    DockEdges? dockToEdge = null;
                    switch (direction) //Handle horizontal adjustment
                    {
                        case MoveToDirections.Left:
                            if (distanceToLeftBoundaryIfFloating == 0)
                            {
                                dockToEdge = DockEdges.Left;
                            }
                            else
                            {
                                window.Left -= distanceToLeftBoundaryIfFloating; 
                            }
                            break;

                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.TopLeft:
                            window.Left -= distanceToLeftBoundaryIfFloating;
                            break;

                        case MoveToDirections.Right:
                            if (distanceToRightBoundaryIfFloating == 0)
                            {
                                dockToEdge = DockEdges.Right;
                            }
                            else
                            {
                                window.Left += distanceToRightBoundaryIfFloating;
                            }
                            break;

                        case MoveToDirections.BottomRight:
                        case MoveToDirections.TopRight:
                            window.Left += distanceToRightBoundaryIfFloating;
                            break;
                    }
                    switch (direction) //Handle vertical adjustment
                    {
                        case MoveToDirections.Bottom:
                            if (distanceToBottomBoundaryIfFloating == 0)
                            {
                                dockToEdge = DockEdges.Bottom;
                            }
                            else
                            {
                                window.Top += distanceToBottomBoundaryIfFloating;
                            }
                            break;

                        case MoveToDirections.BottomLeft:
                        case MoveToDirections.BottomRight:
                            window.Top += distanceToBottomBoundaryIfFloating;
                            break;

                        case MoveToDirections.Top:
                            if (distanceToTopBoundaryIfFloating == 0)
                            {
                                dockToEdge = DockEdges.Top;
                            }
                            else
                            {
                                window.Top -= distanceToTopBoundaryIfFloating;
                            }
                            break;

                        case MoveToDirections.TopLeft:
                        case MoveToDirections.TopRight:
                            window.Top -= distanceToTopBoundaryIfFloating;
                            break;
                    }

                    if (dockToEdge != null)
                    {
                        saveWindowState(WindowStates.Docked);
                        savePreviousWindowState(WindowStates.Docked);
                        saveDockPosition(dockToEdge.Value);
                        RegisterAppBar();
                    }

                    adjustment = true;
                    break;
            }
            return adjustment;
        }

        private void PersistDockThickness()
        {
            Log.Info("PersistDockThickness called");

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
                            ? ((window.ActualHeight / screenBoundsInDp.Height) / getFullDockThicknessAsPercentageOfScreen()) * 10000
                            : ((window.ActualWidth / screenBoundsInDp.Width) / getFullDockThicknessAsPercentageOfScreen()) * 10000;
                    saveCollapsedDockThicknessAsPercentageOfFullDockThickness(collapsedDockThicknessAsPercentageOfFullDockThickness);
                    break;
            }
        }

        private void PersistSizeAndPosition()
        {
            Log.Info("PersistSizeAndPosition called");

            var windowState = getWindowState();
            switch (windowState)
            {
                case WindowStates.Floating:
                    saveFloatingSizeAndPosition(new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight));
                    break;

                case WindowStates.Docked:
                    PersistDockThickness();
                    break;

                case WindowStates.Maximised:
                case WindowStates.Minimised:
                case WindowStates.Hidden:
                    //Do not save anything
                    break;
            }
        }

        private void PublishSizeAndPositionInitialised()
        {
            Log.Info("PublishSizeAndPositionInitialised called");

            if (!SizeAndPositionIsInitialised)
            {
                SizeAndPositionIsInitialised = true;
                if (SizeAndPositionInitialised != null)
                {
                    SizeAndPositionInitialised(this, new EventArgs());
                }
            }
        }

        private void RegisterAppBar()
        {
            Log.Info("RegisterAppBar called");

            if (getWindowState() != WindowStates.Docked) return;

            Log.Debug("WindowState is Docked, continuing to register app bar");

            //Register a new app bar with Windows - this adds it to a list of app bars
            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = windowHandle;
            appBarCallBackId = PInvoke.RegisterWindowMessage("AppBarMessage"); //Get a system wide unique window message (id)
            abd.uCallbackMessage = appBarCallBackId;
            PInvoke.SHAppBarMessage((int)AppBarMessages.New, ref abd);

            //Add hook to receive position change messages from Windows
            HwndSource source = HwndSource.FromHwnd(abd.hWnd);
            source.AddHook(AppBarPositionChangeCallback);
        }

        private void SetAppBarSizeAndPosition(DockEdges dockPosition, Rect sizeAndPosition, bool isInitialising = false)
        {
            Log.InfoFormat("SetAppBarSizeAndPosition called with dockPosition:{0}, sizeAndPosition.Top:{1}, sizeAndPosition.Bottom:{2}, sizeAndPosition.Left:{3}, sizeAndPosition.Right:{4}",
                    dockPosition, sizeAndPosition.Top, sizeAndPosition.Bottom, sizeAndPosition.Left, sizeAndPosition.Right);
            Log.InfoFormat("Screen bounds in px - Top:{0}, Left:{1}, Width:{2}, Height:{3}", screenBoundsInPx.Top, screenBoundsInPx.Left, screenBoundsInPx.Width, screenBoundsInPx.Height);

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

            Log.InfoFormat("QueryPos returned barData.rc.Top:{0}, barData.rc.Bottom:{1}, barData.rc.Left:{2}, barData.rc.Right:{3}",
                    barData.rc.Top, barData.rc.Bottom, barData.rc.Left, barData.rc.Right);

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

            Log.InfoFormat("Rect values adjusted (to compensate for other app bars) to barData.rc.Top:{0}, barData.rc.Bottom:{1}, barData.rc.Left:{2}, barData.rc.Right:{3}",
                    barData.rc.Top, barData.rc.Bottom, barData.rc.Left, barData.rc.Right);
            
            //Then set the dock size and position, using the potentially updated barData
            PInvoke.SHAppBarMessage(AppBarMessages.SetPos, ref barData);
            
            Log.InfoFormat("SetPos returned barData.rc.Top:{0}, barData.rc.Bottom:{1}, barData.rc.Left:{2}, barData.rc.Right:{3}",
                    barData.rc.Top, barData.rc.Bottom, barData.rc.Left, barData.rc.Right);

            var finalDockLeftInDp = barData.rc.Left/Graphics.DipScalingFactorX;
            var finalDockTopInDp = barData.rc.Top / Graphics.DipScalingFactorY;
            var finalDockWidthInDp = (barData.rc.Right - barData.rc.Left) / Graphics.DipScalingFactorX;
            var finalDockHeightInDp = (barData.rc.Bottom - barData.rc.Top) / Graphics.DipScalingFactorY;

            Log.InfoFormat("finalDockLeftInDp:{0}, finalDockTopInDp:{1}, finalDockWidthInDp:{2}, finalDockHeightInDp:{3}", finalDockLeftInDp, finalDockTopInDp, finalDockWidthInDp, finalDockHeightInDp);
            Log.InfoFormat("Screen bounds in dp - Top:{0}, Left:{1}, Width:{2}, Height:{3}", screenBoundsInDp.Top, screenBoundsInDp.Left, screenBoundsInDp.Width, screenBoundsInDp.Height);

            if (finalDockLeftInDp < 0 ||
                finalDockTopInDp < 0 ||
                finalDockWidthInDp <= 0 ||
                finalDockHeightInDp <= 0 ||
                (finalDockLeftInDp + finalDockWidthInDp) > screenBoundsInDp.Right ||
                (finalDockTopInDp + finalDockHeightInDp) > screenBoundsInDp.Bottom)
            {
                Log.Error("Final dock size and/or position is invalid - reverting to floating size and position");
                UnRegisterAppBar();
                saveWindowState(WindowStates.Floating);
                savePreviousWindowState(WindowStates.Floating);
                PublishError(this, new ApplicationException("There was a problem positioning OptiKey - reverting to floating position"));
            }
            else if (!isInitialising)
            {
                //Apply final size and position to the window. This is dispatched with ApplicationIdle priority 
                //as WPF will send a resize after a new appbar is added. We need to apply the received size & position after this happens.
                //RECT values are in pixels so I need to scale back to DIPs for WPF.
                var rect = new Rect(finalDockLeftInDp, finalDockTopInDp, finalDockWidthInDp, finalDockHeightInDp);
                window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ApplySizeAndPositionDelegate(ApplyAndPersistSizeAndPosition), rect);
            }
        }

        private void UnRegisterAppBar()
        {
            Log.Info("UnRegisterAppBar called");

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
