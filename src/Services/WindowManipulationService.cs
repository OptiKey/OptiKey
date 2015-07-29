using System;
using System.Runtime.InteropServices;
using System.Windows;
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
        private readonly Func<WindowStates> getPreviousWindowState;
        private readonly Func<Rect> getFloatingSizeAndPosition;
        private readonly Func<DockEdges> getDockPosition;
        private readonly Func<DockSizes> getDockSize;
        private readonly Func<double> getFullDockThicknessAsPercentageOfScreen;
        private readonly Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Action<WindowStates> saveWindowState;
        private readonly Action<WindowStates> savePreviousWindowState;
        private readonly Action<Rect> saveFloatingSizeAndPosition;
        private Action<DockEdges> saveDockPosition;
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

        public void Expand(ExpandToDirections direction, double amountInDp)
        {
            var windowState = getWindowState();
            if (windowState == WindowStates.Maximised) return;

            var distanceToBottomBoundary = screenBoundsInDp.Bottom - (window.Top + window.ActualHeight);
            var yAdjustmentToBottom = distanceToBottomBoundary < 0 ? distanceToBottomBoundary : amountInDp.CoerceToUpperLimit(distanceToBottomBoundary);
            var distanceToTopBoundary = window.Top - screenBoundsInDp.Top;
            var yAdjustmentToTop = distanceToTopBoundary < 0 ? distanceToTopBoundary : amountInDp.CoerceToUpperLimit(distanceToTopBoundary);
            var distanceToLeftBoundary = window.Left - screenBoundsInDp.Left;
            var xAdjustmentToLeft = distanceToLeftBoundary < 0 ? distanceToLeftBoundary : amountInDp.CoerceToUpperLimit(distanceToLeftBoundary);
            var distanceToRightBoundary = screenBoundsInDp.Right - (window.Left + window.ActualWidth);
            var xAdjustmentToRight = distanceToRightBoundary < 0 ? distanceToRightBoundary : amountInDp.CoerceToUpperLimit(distanceToRightBoundary);

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

            if (windowState == WindowStates.Docked)
            {
                var dockSizeAndPosition = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPosition);
            }
            
            PersistSizeAndPosition();
        }

        public void Maximise()
        {
            savePreviousWindowState(getWindowState());
            window.WindowState = WindowState.Maximized;
            saveWindowState(WindowStates.Maximised);
        }

        public void Move(MoveToDirections direction, double? amountInDp)
        {
            if (getWindowState() == WindowStates.Maximised) return;

            throw new NotImplementedException();
            
            //TODO: If transitioning from floating <-> docked call savePreviousWindowState(getWindowState());
            PersistSizeAndPosition();
        }

        public void ResizeDockToCollapsed()
        {
            if (getWindowState() != WindowStates.Docked || getDockSize() == DockSizes.Collapsed) return;

            saveDockSize(DockSizes.Collapsed);
            var dockSizeAndPosition = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Collapsed);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPosition); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void ResizeDockToFull()
        {
            if (getWindowState() != WindowStates.Docked || getDockSize() == DockSizes.Full) return;
            var dockSizeAndPosition = CalculateDockSizeAndPositionInPx(getDockPosition(), DockSizes.Full);
            SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPosition);
            saveDockSize(DockSizes.Full); //PersistSizeAndPosition() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void Restore()
        {
            if (getWindowState() != WindowStates.Maximised) return;

            var previousWindowState = getPreviousWindowState();
            window.WindowState = WindowState.Normal;
            RestoreSizeAndPosition(previousWindowState, getFloatingSizeAndPosition());
            savePreviousWindowState(WindowStates.Maximised);
            saveWindowState(previousWindowState);
        }

        public void Shrink(ShrinkFromDirections direction, double amountInDp)
        {
            if (getWindowState() == WindowStates.Maximised) return;

            throw new NotImplementedException();

            PersistSizeAndPosition();
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
                    var dockSizeAndPosition = CalculateDockSizeAndPositionInPx(getDockPosition(), getDockSize());
                    SetAppBarSizeAndPosition(getDockPosition(), dockSizeAndPosition);
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
                var dockSizeAndPosition = CalculateDockSizeAndPositionInPx(dockPosition, dockSize);
                RegisterAppBar();
                SetAppBarSizeAndPosition(dockPosition, dockSizeAndPosition);
            }
            else if (windowState == WindowStates.Floating)
            {
                ApplySizeAndPosition(floatingSizeAndPosition);
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
            PInvoke.SHAppBarMessage((int)AppBarMessages.New, ref abd);

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
