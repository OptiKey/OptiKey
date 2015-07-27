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
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;
        private readonly IntPtr windowHandle;
        private readonly Screen screen;
        private readonly Rect screenBoundsInPx;
        private readonly Rect screenBoundsInDp;
        private readonly Func<double> getOpacity;
        private readonly Func<WindowStates> getWindowState;
        private readonly Func<Rect> getFloatingSizeAndPosition;
        private readonly Func<DockEdges> getDockPosition;
        private readonly Func<DockSizes> getDockSize;
        private readonly Func<double> getFullDockThicknessAsPercentageOfScreen;
        private readonly Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Action<WindowStates> saveWindowState;
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
            Func<Rect> getFloatingSizeAndPosition,
            Func<DockEdges> getDockPosition,
            Func<DockSizes> getDockSize,
            Func<double> getFullDockThicknessAsPercentageOfScreen,
            Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness,
            Action<double> saveOpacity,
            Action<WindowStates> saveWindowState,
            Action<Rect> saveFloatingSizeAndPosition,
            Action<DockEdges> saveDockPosition,
            Action<DockSizes> saveDockSize,
            Action<double> saveFullDockThicknessAsPercentageOfScreen,
            Action<double> saveCollapsedDockThicknessAsPercentageOfFullDockThickness)
        {
            this.window = window;
            this.getOpacity = getOpacity;
            this.getWindowState = getWindowState;
            this.getDockPosition = getDockPosition;
            this.getDockSize = getDockSize;
            this.getFullDockThicknessAsPercentageOfScreen = getFullDockThicknessAsPercentageOfScreen;
            this.getCollapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness;
            this.getFloatingSizeAndPosition = getFloatingSizeAndPosition;
            this.saveOpacity = saveOpacity;
            this.saveWindowState = saveWindowState;
            this.saveFloatingSizeAndPosition = saveFloatingSizeAndPosition;
            this.saveDockPosition = saveDockPosition;
            this.saveDockSize = saveDockSize;
            this.saveFullDockThicknessAsPercentageOfScreen = saveFullDockThicknessAsPercentageOfScreen;
            this.saveCollapsedDockThicknessAsPercentageOfFullDockThickness = saveCollapsedDockThicknessAsPercentageOfFullDockThickness;

            windowHandle = new WindowInteropHelper(window).Handle;
            screen = window.GetScreen();
            screenBoundsInPx = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height);
            var screenBoundsTopLeftInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.TopLeft);
            var screenBoundsBottomRightInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.BottomRight);
            screenBoundsInDp = new Rect(screenBoundsTopLeftInDp.X, screenBoundsTopLeftInDp.Y, 
                screenBoundsBottomRightInDp.X - screenBoundsTopLeftInDp.X,
                screenBoundsBottomRightInDp.Y - screenBoundsTopLeftInDp.Y);

            RestoreState();
        }

        #endregion
        
        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Public Methods

        public void ChangeOpacity(bool increase)
        {
            window.Opacity += increase ? 0.1 : -0.1;
            window.Opacity.CoerceToLowerLimit(0.1);
            window.Opacity.CoerceToUpperLimit(1);
            PersistState();
        }

        public void CollapseDock()
        {
            if (getWindowState() != WindowStates.Docked || getDockSize() == DockSizes.Collapsed) return;
            saveDockSize(DockSizes.Collapsed);
            var currentDockSizeAndPosition = CalculateDockSizeAndPosition(getDockPosition(), DockSizes.Collapsed);
            SetAppBarSizeAndPosition(getDockPosition(), currentDockSizeAndPosition);
            //PersistState() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void Expand(ExpandToDirections direction, int? amountInDp)
        {
            throw new NotImplementedException();
        }

        public void ExpandDock()
        {
            if (getWindowState() != WindowStates.Docked || getDockSize() == DockSizes.Full) return;
            var currentDockSizeAndPosition = CalculateDockSizeAndPosition(getDockPosition(), DockSizes.Full);
            SetAppBarSizeAndPosition(getDockPosition(), currentDockSizeAndPosition);
            saveDockSize(DockSizes.Full);
            //PersistState() is called indirectly by SetAppBarSizeAndPosition - no need to call explicitly
        }

        public void Maximise()
        {
            window.WindowState = WindowState.Maximized;
            PersistState();
        }

        public void Move(MoveToDirections direction, int? amountInDp)
        {
            throw new NotImplementedException();
        }

        public void Restore()
        {
            window.WindowState = WindowState.Normal;
            PersistState();
        }

        public void Shrink(ShrinkFromDirections direction, int? amountInDp)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        public IntPtr AppBarPositionChangeCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == appBarCallBackId &&
                getWindowState() == WindowStates.Docked)
            {
                if (wParam.ToInt32() == (int)AppBarNotify.PositionChanged)
                {
                    var currentDockSizeAndPosition = CalculateDockSizeAndPosition(getDockPosition(), getDockSize());
                    SetAppBarSizeAndPosition(getDockPosition(), currentDockSizeAndPosition);
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

            PersistState();
        }

        private Rect CalculateDockSizeAndPosition(DockEdges position, DockSizes size)
        {
            //TODO: Use screen size and getDockThicknessAsPercentageOfScreen() and potentiall getCollapsedDockThicknessAsPercentageOfFullDockThickness()
            return new Rect();
        }

        private void RestoreState()
        {
            try
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
                    floatingSizeAndPosition = new Rect(
                        screenBoundsInDp.Left + screenBoundsInDp.Width / 4, 
                        screenBoundsInDp.Top + screenBoundsInDp.Height / 4,
                        screenBoundsInDp.Width / 2, screenBoundsInDp.Height / 2);
                    saveFloatingSizeAndPosition(floatingSizeAndPosition);
                }

                //Apply state to window
                var windowState = getWindowState();
                window.Opacity = getOpacity();
                window.WindowState = windowState.ToWindowState();
                if (windowState == WindowStates.Docked)
                {
                    var dockPosition = getDockPosition();
                    var dockSize = getDockSize();
                    var dockSizeAndPosition = CalculateDockSizeAndPosition(dockPosition, dockSize);
                    RegisterAppBar();
                    SetAppBarSizeAndPosition(dockPosition, dockSizeAndPosition);
                }
                else if (windowState == WindowStates.Floating)
                {
                    ApplySizeAndPosition(floatingSizeAndPosition);
                }
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        private void PersistState()
        {
            //    setWindowSizeAndPositionSetting(new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight));
            //Map window.WindowState to my version of WindowStates
        }

        private void RegisterAppBar()
        {
            //Get a system wide unique window message (id)
            appBarCallBackId = PInvoke.RegisterWindowMessage("AppBarMessage");
            
            //Register a new app bar with Windows - this adds it to a list of app bars
            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = windowHandle;
            abd.uCallbackMessage = appBarCallBackId;
            PInvoke.SHAppBarMessage(AppBarMessages.New, ref abd);

            //Add hook to receive position change messages from Windows
            HwndSource source = HwndSource.FromHwnd(abd.hWnd);
            source.AddHook(AppBarPositionChangeCallback);
        }

        private void SetAppBarSizeAndPosition(DockEdges dockPosition, Rect sizeAndPosition)
        {
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
            //as WPF will send a resize after a new appbar is added. We need to apply the received size & position after this happens
            var rect = new Rect(barData.rc.Left, barData.rc.Top, barData.rc.Right - barData.rc.Left, barData.rc.Bottom - barData.rc.Top);
            window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new ApplySizeAndPositionDelegate(ApplySizeAndPosition), rect);
        }

        private void UnRegisterAppBar()
        {
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
