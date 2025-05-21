// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
using JuliusSweetland.OptiKey.Properties;
using log4net;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKey.Native.Common.Static;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService
    {
        public enum GetWindowLongPtrIndex : int
        {
            GWL_EXSTYLE = -20,  // Extended window styles
            GWL_STYLE = -16,  // Window styles  
            GWL_WNDPROC = -4,   // Window procedure address
            GWL_HINSTANCE = -6,   // Application instance handle
            GWL_HWNDPARENT = -8,   // Parent window handle
            GWL_ID = -12,  // Control identifier
            GWL_USERDATA = -21,  // User data associated with window

            // Dialog box specific
            DWL_DLGPROC = 4,    // Dialog procedure address
            DWL_MSGRESULT = 0,    // Message result
            DWL_USER = 8     // Application-specific data
        }
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
        private Rect appBarBoundsInPx;
        private Rect screenBoundsInPx;
        private Rect screenBoundsInDp;
        private Func<bool> getPersistedState;
        private readonly Func<double> getOpacity;
        private readonly Func<WindowStates> getWindowState;
        private readonly Func<WindowStates> getPreviousWindowState;
        private readonly Func<Rect> getFloatingSizeAndPosition;
        private readonly Func<DockEdges> getDockPosition;
        private readonly Func<DockSizes> getDockSize;
        private readonly Func<double> getFullDockThicknessAsPercentageOfScreen;
        private readonly Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Func<MinimisedEdges> getMinimisedPosition;
        private readonly Action<bool> savePersistedState;
        private readonly Action<double> saveOpacity;
        private readonly Action<WindowStates> saveWindowState;
        private readonly Action<WindowStates> savePreviousWindowState;
        private readonly Action<Rect> saveFloatingSizeAndPosition;
        private readonly Action<DockEdges> saveDockPosition;
        private readonly Action<DockSizes> saveDockSize;
        private readonly Action<double> saveFullDockThicknessAsPercentageOfScreen;
        private readonly Action<double> saveCollapsedDockThicknessAsPercentageOfFullDockThickness;

        private int appBarCallBackId = -1;
        private bool mouseResizeUnderway = false;

        private delegate void ApplySizeAndPositionDelegate(Rect rect);

        #endregion

        #region Ctor

        public WindowManipulationService(
            Window window,
            Func<bool> getPersistedState,
            Func<double> getOpacity,
            Func<WindowStates> getWindowState,
            Func<WindowStates> getPreviousWindowState,
            Func<Rect> getFloatingSizeAndPosition,
            Func<DockEdges> getDockPosition,
            Func<DockSizes> getDockSize,
            Func<double> getFullDockThicknessAsPercentageOfScreen,
            Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness,
            Func<MinimisedEdges> getMinimisedPosition,
            Action<bool> savePersistedState,
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
            this.getPersistedState = getPersistedState;
            this.getOpacity = getOpacity;
            this.getWindowState = getWindowState;
            this.getPreviousWindowState = getPreviousWindowState;
            this.getDockPosition = getDockPosition;
            this.getDockSize = getDockSize;
            this.getFullDockThicknessAsPercentageOfScreen = getFullDockThicknessAsPercentageOfScreen;
            this.getCollapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness;
            this.getMinimisedPosition = getMinimisedPosition;
            this.getFloatingSizeAndPosition = getFloatingSizeAndPosition;
            this.savePersistedState = savePersistedState;
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

            UpdateScreenSizeAndPosition();
            Log.DebugFormat("Screen bounds in Px: {0}", screenBoundsInPx);
            Log.DebugFormat("Screen bounds in Dp: {0}", screenBoundsInDp);

            PreventInvalidRestoreState();
            CoerceSavedStateAndApply();
            PreventWindowActivation();

            window.SizeChanged += (sender, args) => Log.Info($"Window SizeChange event detected from {args.PreviousSize} to {args.NewSize}. (Window state is {window.WindowState}, location is left:{window.Left}, right:{window.Left + window.Width}, top:{window.Top}, bottom:{window.Top + window.Height}).");

            window.Closed += (_, __) => UnRegisterAppBar();

            HwndSource hwndSource = HwndSource.FromHwnd(windowHandle);
            if (hwndSource != null)
            {
                hwndSource.AddHook(WndProc);
            }
        }

        private const Int32 WM_ENTERSIZEMOVE = 0x0231;
        private const Int32 WM_EXITSIZEMOVE = 0x0232;
        private const Int32 WM_NCLBUTTONDBLCLK = 0x00A3;

        private const Int32 WM_SYSCOMMAND = 0x112;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:
                    // This prevents Windows aero-snap on edges whilst move is underway
                    switch (wParam.ToInt32() & 0xFFF0) // lower-order bits are used internally
                    {
                        case 0xF010: // SC_MOVE
                        case 0xF000: // SC_SIZE
                            window.ResizeMode = ResizeMode.NoResize;
                            break;
                    }
                    break;
                case WM_ENTERSIZEMOVE:
                    mouseResizeUnderway = true;
                    break;
                case WM_EXITSIZEMOVE:

                    // This message is sent at the end of a user-resize (via window drag handles)
                    Log.Info("WM_EXITSIZEMOVE called");
                    mouseResizeUnderway = false;
                    CoerceDockSizeAndPosition();
                    // restore resize grips 
                    window.ResizeMode = ResizeMode.CanResizeWithGrip;
                    break;
                case WM_NCLBUTTONDBLCLK:
                    handled = true;  //prevent double click from maximizing the window.
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion

        #region Events

        public event EventHandler SizeAndPositionInitialised;
        public event EventHandler<Exception> Error;

        #endregion

        #region Properties

        public bool SizeAndPositionIsInitialised { get; private set; }

        public IntPtr WindowHandle { get { return windowHandle; } }

        public Rect WindowBounds
        {
            get
            {
                return new Rect
                {
                    X = window.Left,
                    Y = window.Top,
                    Width = window.ActualWidth,
                    Height = window.ActualHeight,
                };
            }
        }

        public WindowStates WindowState { get { return getWindowState(); } }

        #endregion

        #region Public Methods

        public void DisableResize()
        {
            if (window.ResizeMode == ResizeMode.NoResize)
                return;
            window.ResizeMode = ResizeMode.NoResize;
            window.UpdateLayout();
        }

        public void SetResizeState()
        {
            if (!Settings.Default.EnableResizeWithMouse)
            {
                DisableResize();
                return;
            }

            var windowState = getWindowState();
            switch (windowState)
            {
                case WindowStates.Floating:
                    window.ResizeMode = ResizeMode.CanResizeWithGrip;
                    break;

                case WindowStates.Docked:
                    if (getDockSize() == DockSizes.Full)
                        window.ResizeMode = ResizeMode.CanResizeWithGrip;
                    else
                        window.ResizeMode = ResizeMode.NoResize;
                    break;

                case WindowStates.Maximised:
                case WindowStates.Minimised:
                case WindowStates.Hidden:
                    window.ResizeMode = ResizeMode.NoResize;
                    break;
            }
            window.UpdateLayout();
        }

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

        /// <summary>
        /// Hide the window, but don't save window state as this can break calls to the Restore, RestoreSavedState, or ApplySavedSate methods
        /// </summary>
        public void Hide()
        {
            Log.Info("Hide called");

            var windowState = getWindowState();
            if (windowState == WindowStates.Hidden) return;

            if (windowState == WindowStates.Docked)
            {
                UnRegisterAppBar();
            }
            saveWindowState(WindowStates.Hidden);
            ApplySavedState();
            saveWindowState(windowState);
        }

        public bool GetPersistedState()
        {
            return getPersistedState();
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

            if (windowState == WindowStates.Maximised)
                return;

            if (windowState == WindowStates.Docked)
                UnRegisterAppBar();

            savePreviousWindowState(windowState);
            saveWindowState(WindowStates.Maximised);
            ApplySavedState();
        }

        public void Minimise()
        {
            Log.Info("Minimise called");
            var windowState = getWindowState();

            if (windowState == WindowStates.Minimised)
                return;

            if (windowState == WindowStates.Docked)
                UnRegisterAppBar();

            savePreviousWindowState(windowState);
            saveWindowState(WindowStates.Minimised);
            ApplySavedState();
        }

        public void Move(MoveToDirections direction, double? amountInPx)
        {
            Log.InfoFormat("Move called with direction {0} and amount (px) {1}", direction, amountInPx);

            var windowState = getWindowState();
            if (windowState == WindowStates.Maximised) return;

            var floatingSizeAndPosition = new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight);
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

        public void OverridePersistedState(bool inPersistNewState, string inWindowState, string inPosition, string inDockSize, string inWidth, string inHeight, string inHorizontalOffset, string inVerticalOffset)
        {
            Log.InfoFormat("OverridePersistedState called with PersistNewState {0}, WindowState {1}, Position {2}, Width {3}, Height {4}, horizontalOffset {5}, verticalOffset {6}", inPersistNewState, inWindowState, inPosition, inWidth, inHeight, inHorizontalOffset, inVerticalOffset);

            PersistSizeAndPosition();

            WindowStates activeWindowState = getWindowState();
            WindowStates newWindowState = Enum.TryParse(inWindowState, out newWindowState) ? newWindowState : getWindowState();
            DockEdges newDockPosition = Enum.TryParse(inPosition, out newDockPosition) ? newDockPosition : getDockPosition();
            DockSizes newDockSize = Enum.TryParse(inDockSize, out newDockSize) ? newDockSize : getDockSize();
            var dockThicknessInPx = CalculateDockSizeAndPositionInPx(newDockPosition, newDockSize);
            double validNumber;
            // if no value from file, use default value
            // if value from file is numeric, use it as is
            // if value from file is numeric with % symbol, use it as percent
            var newWidth = string.IsNullOrWhiteSpace(inWidth) || !double.TryParse(inWidth.Replace("%", ""), out validNumber) || validNumber < -9999 || validNumber > 9999
                ? newWindowState == WindowStates.Floating
                    ? getFloatingSizeAndPosition().Width
                    : dockThicknessInPx.Width / Graphics.DipScalingFactorX //Scale to dp
                : inWidth.Contains("%") && validNumber > 0
                    ? (validNumber / 100d) * screenBoundsInDp.Width
                    : inWidth.Contains("%")
                        ? (validNumber / 100d + 1) * screenBoundsInDp.Width
                        : validNumber > 0
                            ? validNumber / Graphics.DipScalingFactorX
                : validNumber / Graphics.DipScalingFactorX + screenBoundsInDp.Width;

             newWidth = Math.Max(newWidth, .03 * screenBoundsInDp.Width);

            var newHeight = string.IsNullOrWhiteSpace(inHeight) || !double.TryParse(inHeight.Replace("%", ""), out validNumber) || validNumber < -9999 || validNumber > 9999
                ? newWindowState == WindowStates.Floating
                    ? getFloatingSizeAndPosition().Height
                    : dockThicknessInPx.Height / Graphics.DipScalingFactorY //Scale to dp
                : inHeight.Contains("%") && validNumber > 0
                    ? (validNumber / 100d) * screenBoundsInDp.Height
                    : inHeight.Contains("%")
                        ? (validNumber / 100d + 1) * screenBoundsInDp.Height
                        : validNumber > 0
                            ? validNumber / Graphics.DipScalingFactorY
                : validNumber / Graphics.DipScalingFactorY + screenBoundsInDp.Height;

            newHeight = Math.Max(newHeight, .03 * screenBoundsInDp.Width);

            var newFullDockThicknessPercent = (newDockPosition == DockEdges.Top || newDockPosition == DockEdges.Bottom)
                ? (100 * newHeight / screenBoundsInDp.Height) : (100 * newWidth / screenBoundsInDp.Width);

            var horizontalOffset = string.IsNullOrWhiteSpace(inHorizontalOffset) || !double.TryParse(inHorizontalOffset.Replace("%", ""), out validNumber) || validNumber < -9999 || validNumber > 9999
                ? screenBoundsInDp.Left
                : inHorizontalOffset.Contains("%")
                    ? validNumber / 100d * screenBoundsInDp.Width
                : validNumber / Graphics.DipScalingFactorX;

            var verticalOffset = string.IsNullOrWhiteSpace(inVerticalOffset) || !double.TryParse(inVerticalOffset.Replace("%", ""), out validNumber) || validNumber < -9999 || validNumber > 9999
                ? screenBoundsInDp.Top
                : inVerticalOffset.Contains("%")
                    ? validNumber / 100d * screenBoundsInDp.Height
                : validNumber / Graphics.DipScalingFactorY;

            double newTop = getFloatingSizeAndPosition().Top;
            double newLeft = getFloatingSizeAndPosition().Left;
            if (Enum.TryParse(inPosition, out MoveToDirections newMovePosition))
            {
                newTop = (newMovePosition == MoveToDirections.Top || newMovePosition == MoveToDirections.TopLeft || newMovePosition == MoveToDirections.TopRight)
                    ? screenBoundsInDp.Top + verticalOffset
                    : (newMovePosition == MoveToDirections.Bottom || newMovePosition == MoveToDirections.BottomLeft || newMovePosition == MoveToDirections.BottomRight)
                    ? screenBoundsInDp.Bottom - newHeight + verticalOffset
                    : screenBoundsInDp.Height / 2d - newHeight / 2d + verticalOffset;

                newLeft = (newMovePosition == MoveToDirections.Left || newMovePosition == MoveToDirections.TopLeft || newMovePosition == MoveToDirections.BottomLeft)
                    ? screenBoundsInDp.Left + horizontalOffset
                    : (newMovePosition == MoveToDirections.Right || newMovePosition == MoveToDirections.TopRight || newMovePosition == MoveToDirections.BottomRight)
                    ? screenBoundsInDp.Right - newWidth + horizontalOffset
                    : screenBoundsInDp.Width / 2d - newWidth / 2d + horizontalOffset;
            }

            if (activeWindowState == WindowStates.Docked && newWindowState != WindowStates.Docked)
                UnRegisterAppBar();

            savePersistedState(inPersistNewState);
            saveFloatingSizeAndPosition(new Rect(newLeft, newTop, newWidth, newHeight));
            saveDockPosition(newDockPosition);
            saveDockSize(newDockSize);
            saveFullDockThicknessAsPercentageOfScreen(newFullDockThicknessPercent);
            saveCollapsedDockThicknessAsPercentageOfFullDockThickness(100 * newFullDockThicknessPercent / getFullDockThicknessAsPercentageOfScreen());
            savePreviousWindowState(activeWindowState);
            saveWindowState(newWindowState);
            ApplySavedState();
        }

        public void ResizeDockToCollapsed()
        {
            Log.Info("ResizeDockToCollapsed called");

            if (getWindowState() != WindowStates.Docked) return;

            // Turn off grab handles, to avoid ambiguous requests
            DisableResize();

            saveDockSize(DockSizes.Collapsed);
            var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Collapsed);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void ResizeDockToFull()
        {
            Log.Info("ResizeDockToFull called");

            if (getWindowState() != WindowStates.Docked) return;

            SetResizeState();

            saveDockSize(DockSizes.Full);
            var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Full);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void RestorePersistedState()
        {
            Log.Info("RestorePersistedState called");

            if (getPersistedState()) return;

            var activeWindowState = getWindowState();
            if (activeWindowState != WindowStates.Docked && activeWindowState != WindowStates.Floating && activeWindowState != WindowStates.Maximised) return;

            savePersistedState(true);
            var persistedWindowState = getWindowState();

            if (activeWindowState == WindowStates.Docked && persistedWindowState != WindowStates.Docked)
                UnRegisterAppBar();

            ApplySavedState();
            ResizeDockToFull();
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

        public void RestoreSavedState()
        {
            Log.Info("RestoreSavedState called (applying saved state only)");
            ApplySavedState();
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

        private enum ResizeStrategy { ForceResize, ForceFit, SoftResize, SoftFit, Move }

        public void InvokeMoveWindow(string parameterString)
        {
            // parameter string is one of:
            // "ma..." to maximise (i.e. could be 'ma' or 'max', or 'maximum', only first 2 chars count)
            // "mi..." to minimise
            // "left, top, right, bottom, resizeStrategy"
            // where left/top/etc are either raw pixels or %, e.g. "400" = 400 pixels, "40%" = 40% of screen dimension
            // resizeStrategy is one of:
            // empty: defaults to forceResize for backward compatibility
            // forceResize: requests the window resize to target rect
            // softResize:  as 'forceResize' but *only* if window has WS_THICKFRAME style (strong proxy of 'is intended to be resizable')
            // forceFit:    requests the window is scaled down in any necessary dimension, but not scaled up. The window will be centred
            //              in any dimension that is smaller than the target rect.
            // softFit:     as 'forceFit', but *only* if window has WS_THICKFRAME style 
            // move:        does not resize. Centres the window in the target rect.            

            var parameterArray = parameterString.Split(',');
            var handle = PInvoke.GetForegroundWindow();
            if (handle == windowHandle)
                return;
            if (parameterString.Length < 2)
                return;
            
            var showStyle = parameterString.Substring(0, 2).ToLower() == "ma" ? WindowShowStyle.ShowMaximized : 
                           parameterString.Substring(0, 2).ToLower() == "mi" ? WindowShowStyle.ShowMinimized : 
                           WindowShowStyle.Restore;

            // Query current state before changing anything
            var style = Windows.GetWindowStyle(handle);
            var bounds = GetWindowBounds(handle);

            // Restore window, then resize after
            PInvoke.ShowWindow(handle, (int)showStyle);

            if (showStyle == WindowShowStyle.ShowMaximized ||
                showStyle == WindowShowStyle.ShowMinimized)
                return;
            else
            {
                // Else we will have repositioning to do
                if (parameterArray.Length < 4)
                {
                    Log.Error($"Invalid parameter string for MoveWindow");
                }
            }
            
            Func<string, double, double> getAndScaleValue = (param, multiplier) =>
            {
                return string.IsNullOrWhiteSpace(param) ? double.NaN
                : double.TryParse(param.Replace("%", ""), out double result) && !param.Contains("%") ? result
                : multiplier * result / 100;
            };

            ResizeStrategy resizeStrategy;
            if (parameterArray.Length < 5)
            {
                resizeStrategy = ResizeStrategy.ForceResize;
            }
            else
            {
                var resizeParam = parameterArray[4].ToLower();
                if (!Enum.TryParse(resizeParam, true, out resizeStrategy)) {
                    Log.Info($"Couldn't parse MoveWindow resize strategy: {resizeStrategy} ");
                }
            }

            if (bounds == null)
            {
                Log.Error($"Unable to move window {handle} with invalid bounds ");
                return;
            }
            
            // Get target rectangle 
            var left = getAndScaleValue(parameterArray[0], screenBoundsInPx.Width);
            var top = getAndScaleValue(parameterArray[1], screenBoundsInPx.Height);
            var right = getAndScaleValue(parameterArray[2], screenBoundsInPx.Width);
            var bottom = getAndScaleValue(parameterArray[3], screenBoundsInPx.Height);
            
            bool isResizeable = (style & WindowStyles.WS_THICKFRAME) != 0;

            var origWidth = bounds.Value.Width;
            var origHeight = bounds.Value.Height;
            var targetWidth = right - left;
            var targetHeight = bottom - top;

            // Fallback to moving to a centred location
            var leftFinal = (int)(left + (targetWidth - origWidth) / 2);
            var topFinal = (int)(top + (targetHeight - origHeight) / 2);
            var widthFinal = (int)origWidth;
            var heightFinal = (int)origHeight;

            switch (resizeStrategy)
            {
                case ResizeStrategy.ForceResize:
                case ResizeStrategy.SoftResize:
                    if (resizeStrategy == ResizeStrategy.ForceResize ||
                        isResizeable)
                    {
                        leftFinal = (int)left;
                        topFinal = (int)top;
                        widthFinal = (int)(right - left);
                        heightFinal = (int)(bottom - top);
                    }
                    // else fallback to centred
                    break;

                case ResizeStrategy.ForceFit:
                case ResizeStrategy.SoftFit:
                    if (resizeStrategy == ResizeStrategy.ForceFit ||
                        isResizeable)
                    {
                        widthFinal = (int)Math.Min(targetWidth, origWidth);
                        heightFinal = (int)Math.Min(targetHeight, origHeight);
                        leftFinal = (int)(left + (targetWidth - widthFinal) / 2);
                        topFinal = (int)(top + (targetHeight - heightFinal) / 2);
                    }
                    // else fallback to centred
                    break;

                case ResizeStrategy.Move:
                    // fallback to centred
                    break;

            }

            PInvoke.MoveWindow(handle, leftFinal, topFinal, widthFinal, heightFinal, true);
            
        }

        public void SetFocusable(bool focusable)
        {
            if (focusable)
            {
                Static.Windows.SetExtendedWindowStyle(windowHandle,
                Static.Windows.GetExtendedWindowStyle(windowHandle) & ~ExtendedWindowStyles.WS_EX_NOACTIVATE);
            }
            else
            {
                Static.Windows.SetExtendedWindowStyle(windowHandle,
                Static.Windows.GetExtendedWindowStyle(windowHandle) | ExtendedWindowStyles.WS_EX_NOACTIVATE);
            }
        }

        public void SetShowInTaskbar(bool showInTaskbar)
        {
            if (showInTaskbar)
            {
                Static.Windows.SetExtendedWindowStyle(windowHandle,
                Static.Windows.GetExtendedWindowStyle(windowHandle) & ~ExtendedWindowStyles.WS_EX_TOOLWINDOW);
            }
            else
            {
                Static.Windows.SetExtendedWindowStyle(windowHandle,
                Static.Windows.GetExtendedWindowStyle(windowHandle) | ExtendedWindowStyles.WS_EX_TOOLWINDOW);
            }
        }

        #endregion

        #region Private Methods


        private Rect? GetWindowBounds(IntPtr hWnd)
        {
            RECT rawRect;
            if (PInvoke.IsWindow(hWnd))
            {
                if (PInvoke.DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out rawRect, Marshal.SizeOf<RECT>()) != 0
                    || (PInvoke.GetWindowRect(hWnd, out rawRect)))
                    return new Rect
                    {
                        X = rawRect.Left,
                        Y = rawRect.Top,
                        Width = rawRect.Right - rawRect.Left,
                        Height = rawRect.Bottom - rawRect.Top
                    };
            }
            return null;
        }

        private IntPtr AppBarPositionChangeCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == appBarCallBackId &&
                getWindowState() == WindowStates.Docked)
            {
                if (wParam.ToInt32() == (int)AppBarNotify.PositionChanged)
                {
                    if (!mouseResizeUnderway)
                    {
                        Log.Info("AppBarPositionChangeCallback called with PositionChanged message.");
                        UpdateAppBarPosition();
                        handled = true;
                    }
                }
            }
            return IntPtr.Zero;
        }

        private void UpdateAppBarPosition()
        {
            var dockSizeAndPositionInPx = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPositionInPx);
        }

        private void ApplyAndPersistSizeAndPosition(Rect rect)
        {
            Log.InfoFormat("ApplyAndPersistSizeAndPosition called with rect.Top:{0}, rect.Bottom:{1}, rect.Left:{2}, rect.Right:{3}",
                rect.Top, rect.Bottom, rect.Left, rect.Right);

            this.ApplySizeAndPosition(rect);
            PersistSizeAndPosition();
        }

        private void CoerceDockSizeAndPosition()
        {
            Log.InfoFormat("CoerceDockSizeAndPosition called");

            if (getWindowState() != WindowStates.Docked) return;

            // If app has been manually resized in a way that doesn't respect the docking, recompute appropriate dock position 
            DockEdges dockEdge = getDockPosition();
            double dockThickness;
            if (dockEdge == DockEdges.Bottom || dockEdge == DockEdges.Top) {
                double thicknessAsPercentage = screenBoundsInPx.Height / window.Height;

                var distanceToBottomBoundary = screenBoundsInDp.Bottom - (window.Top + window.ActualHeight);
                var yAdjustmentToBottom = distanceToBottomBoundary < 0 ? distanceToBottomBoundary : 0;
                dockThickness = ((window.ActualHeight + yAdjustmentToBottom) / screenBoundsInDp.Height) * 100;
            }
            else
            {
                var distanceToLeftBoundary = window.Left - screenBoundsInDp.Left;
                var xAdjustmentToLeft = distanceToLeftBoundary < 0 ? distanceToLeftBoundary : 0;
                dockThickness = ((window.ActualWidth + xAdjustmentToLeft) / screenBoundsInDp.Width) * 100;
            }

            if (getDockSize() == DockSizes.Collapsed)
            {
                //Interpret the full size dock from the collapsed size
                dockThickness = dockThickness * (100 / getCollapsedDockThicknessAsPercentageOfFullDockThickness());
            }

            saveFullDockThicknessAsPercentageOfScreen(dockThickness);

            UpdateAppBarPosition();
        }

        private void ApplySizeAndPosition(Rect rect)
        {
            Log.InfoFormat("ApplySizeAndPosition called with rect.Top:{0}, rect.Bottom:{1}, rect.Left:{2}, rect.Right:{3}",
                rect.Top, rect.Bottom, rect.Left, rect.Right);

            //Changed code to use PInvoke.MoveWindow because it will perform all 4 adjustments and then repaint the window
            //Previously 4 commands were required to set the 4 values and had the undesirable behavior of triggerring a repaint after each of the 4 commands
            PInvoke.MoveWindow(windowHandle,
                (int)Math.Round(rect.Left * Graphics.DipScalingFactorX),
                (int)Math.Round(rect.Top * Graphics.DipScalingFactorY),
                (int)Math.Round(rect.Width * Graphics.DipScalingFactorX),
                (int)Math.Round(rect.Height * Graphics.DipScalingFactorY), true);

            PublishSizeAndPositionInitialised();
        }

        private void ApplySavedState(bool isInitialising = false)
        {
            Log.Info("ApplySavedState called");

            var windowState = getWindowState();
            window.Opacity = getOpacity();
            var dockPosition = getDockPosition();

            SetResizeState();
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

            // Check if screen bounds have changed (e.g. change in resolution)
            UpdateScreenSizeAndPosition();

            double x, y, width, height;
            double thicknessAsPercentage = size == DockSizes.Full
                    ? getFullDockThicknessAsPercentageOfScreen() / 100
                    : (getFullDockThicknessAsPercentageOfScreen() *
                       getCollapsedDockThicknessAsPercentageOfFullDockThickness()) / 10000; //Percentage of a percentage

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

        private void UpdateScreenSizeAndPosition()
        {
            screen = window.GetScreen();
            screenBoundsInPx = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height);
            Log.DebugFormat("Screen bounds in Px: {0}", screenBoundsInPx);
            var screenBoundsTopLeftInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.TopLeft);
            var screenBoundsBottomRightInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.BottomRight);
            screenBoundsInDp = new Rect(screenBoundsTopLeftInDp.X, screenBoundsTopLeftInDp.Y,
                screenBoundsBottomRightInDp.X - screenBoundsTopLeftInDp.X,
                screenBoundsBottomRightInDp.Y - screenBoundsTopLeftInDp.Y);
        }

        private Rect CalculateMinimisedSizeAndPosition()
        {
            Log.Info("CalculateMinimisedSizeAndPosition called.");

            var persitedState = getPersistedState();
            savePersistedState(true);
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

            savePersistedState(persitedState);
            return new Rect(x, y, width, height);
        }

        public void ChangeState(WindowStates newState, DockEdges dockPosition)
        {
            var windowState = getWindowState();
            var dockPos = getDockPosition();

            bool changeCurrentState = !(windowState == WindowStates.Minimised || windowState == WindowStates.Hidden);
            if (newState == WindowStates.Docked)
            {
                if (windowState != WindowStates.Docked)
                {  
                    RegisterAppBar(true);
                }   
                savePreviousWindowState(WindowStates.Docked);
                saveDockPosition(dockPosition);
                if (changeCurrentState)
                {
                    saveWindowState(WindowStates.Docked);
                    ResizeDockToFull();
                }
            }
            else
            {
                ResizeDockToFull(); // in case we're in Collapsed state
                UnRegisterAppBar();
                savePreviousWindowState(WindowStates.Floating);
                if (changeCurrentState)
                {
                    saveWindowState(WindowStates.Floating);
                    CoerceSavedStateAndApply();
                    Restore();
                }
            }
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
                    Log.WarnFormat("Saved full docked thickness of {0} is invalid. Restoring to default.", fullDockThicknessAsPercentageOfScreen);
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

                // If position extends beyond the screen bounds, force back in
                Rect screenRect = new Rect(new Point(screenBoundsInDp.Left, screenBoundsInDp.Top),
                                           new Size(screenBoundsInDp.Width, screenBoundsInDp.Height));
                Rect intersection = Rect.Intersect(floatingSizeAndPosition, screenRect);
                if (!intersection.IsCloseTo(floatingSizeAndPosition))
                {
                    floatingSizeAndPosition = intersection;
                    saveFloatingSizeAndPosition(intersection);
                }

                // If we still can't validate size/position, revert to default 
                if (floatingSizeAndPosition == default(Rect) ||
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

        public void PersistSizeAndPosition()
        {
            Log.Info("PersistSizeAndPosition called");

            if (!getPersistedState() || !SizeAndPositionIsInitialised) return;

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

        private void RegisterAppBar(bool force=false)
        {
            Log.Info("RegisterAppBar called");

            if (!force && getWindowState() != WindowStates.Docked) return;

            Log.Debug("WindowState is Docked, continuing to register app bar");

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

        private void PreventInvalidRestoreState()
        {
            //We don't want OptiKey to be restored to a Minimised, Hidden,
            //or Maximised state, especially if it starts up in Conversation
            //mode. Thus we modify the initial window states if necessary.
            var windowState = getWindowState();
            var previousWindowState = getPreviousWindowState();

            if (windowState != WindowStates.Docked
                && windowState != WindowStates.Floating)
            {
                if (previousWindowState == WindowStates.Docked
                    || previousWindowState == WindowStates.Floating)
                {
                    saveWindowState(previousWindowState);
                }
                else
                {
                    saveWindowState(WindowStates.Docked);
                }
            }

            if (previousWindowState != WindowStates.Docked
                && previousWindowState != WindowStates.Floating)
            {
                savePreviousWindowState(WindowStates.Docked);
            }
        }

        //If we are using mouse button clicks as a key selection source we should prevent 
        //the window from changing focus so the target window receives the keyboard input.
        //when using touchscreen, each touch will trigger mouse click so we need to prevent focus steal from active window 
        private void PreventWindowActivation()
        {
            if (Settings.Default.KeySelectionTriggerSource == TriggerSources.MouseButtonDownUps || 
                Settings.Default.PointsSource == PointsSources.TouchScreenPosition)
            {
                const int WS_EX_APPWINDOW = 0x40000;
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int GWL_EXSTYLE = -0x14;

                PInvoke.SetWindowLong(windowHandle, GWL_EXSTYLE,
                    (int)PInvoke.GetWindowLong(windowHandle, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_APPWINDOW);
            }
        }

        private void SetAppBarSizeAndPosition(DockEdges dockPosition, Rect sizeAndPosition, bool isInitialising = false, bool persist = true)
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
                    barData.rc.Bottom += barData.rc.Top - (int)Math.Round(sizeAndPosition.Top);
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

            var finalDockLeftInDp = barData.rc.Left / Graphics.DipScalingFactorX;
            var finalDockTopInDp = barData.rc.Top / Graphics.DipScalingFactorY;
            var finalDockWidthInDp = (barData.rc.Right - barData.rc.Left) / Graphics.DipScalingFactorX;
            var finalDockHeightInDp = (barData.rc.Bottom - barData.rc.Top) / Graphics.DipScalingFactorY;

            Log.InfoFormat("finalDockLeftInDp:{0}, finalDockTopInDp:{1}, finalDockWidthInDp:{2}, finalDockHeightInDp:{3}", finalDockLeftInDp, finalDockTopInDp, finalDockWidthInDp, finalDockHeightInDp);
            Log.InfoFormat("Screen bounds in dp - Top:{0}, Left:{1}, Width:{2}, Height:{3}", screenBoundsInDp.Top, screenBoundsInDp.Left, screenBoundsInDp.Width, screenBoundsInDp.Height);

            if (isInitialising) return;

            if (finalDockHeightInDp <= 0 || finalDockWidthInDp <= 0)
            {
                Log.WarnFormat("Unable to set inappropriate window size - height:{0}, width:{1}; Width and height must all be positive values.", finalDockHeightInDp, finalDockWidthInDp);
                return;
            }

            //Apply final size and position to the window. This is dispatched with ApplicationIdle priority 
            //as WPF will send a resize after a new appbar is added. We need to apply the received size & position after this happens.
            //RECT values are in pixels so I need to scale back to DIPs for WPF.
            appBarBoundsInPx = new Rect(finalDockLeftInDp, finalDockTopInDp, finalDockWidthInDp, finalDockHeightInDp);
            window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                persist
                    ? new ApplySizeAndPositionDelegate(ApplyAndPersistSizeAndPosition)
                    : new ApplySizeAndPositionDelegate(ApplySizeAndPosition), appBarBoundsInPx);
        }

        private void UnRegisterAppBar()
        {
            Log.Info("UnRegisterAppBar called");

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
