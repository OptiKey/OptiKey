using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
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
        private readonly Func<DockPositions?> getDockPosition;
        private readonly Func<double> getDockThicknessAsPercentageOfScreen;
        private readonly Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Func<Rect?> getFloatingSizeAndPosition;
        private readonly Action<WindowStates> saveWindowState;
        private readonly Action<Rect?> saveFloatingSizeAndPosition;
        private readonly Action<DockPositions?> saveDockPosition;
        private readonly Action<double> saveDockThicknessAsPercentageOfScreen;
        private readonly Action<double> saveOpacity;

        #endregion

        #region Ctor
        
        internal WindowManipulationService(
            Window window,
            Func<double> getOpacity,
            Func<WindowStates> getWindowState,
            Func<Rect?> getFloatingSizeAndPosition,
            Func<DockPositions?> getDockPosition,
            Func<double> getDockThicknessAsPercentageOfScreen,
            Func<double> getCollapsedDockThicknessAsPercentageOfFullDockThickness,
            Action<double> saveOpacity,
            Action<WindowStates> saveWindowState,
            Action<Rect?> saveFloatingSizeAndPosition,
            Action<DockPositions?> saveDockPosition,
            Action<double> saveDockThicknessAsPercentageOfScreen)
        {
            this.window = window;
            this.getOpacity = getOpacity;
            this.getDockPosition = getDockPosition;
            this.getDockThicknessAsPercentageOfScreen = getDockThicknessAsPercentageOfScreen;
            this.getCollapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness;
            this.getFloatingSizeAndPosition = getFloatingSizeAndPosition;
            this.saveOpacity = saveOpacity;
            this.saveWindowState = saveWindowState;
            this.saveFloatingSizeAndPosition = saveFloatingSizeAndPosition;
            this.saveDockPosition = saveDockPosition;
            this.saveDockThicknessAsPercentageOfScreen = saveDockThicknessAsPercentageOfScreen;

            windowHandle = new WindowInteropHelper(window).Handle;
            screen = window.GetScreen();
            screenBoundsInPx = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height);
            var screenBoundsTopLeftInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.TopLeft);
            var screenBoundsBottomRightInDp = window.GetTransformFromDevice().Transform(screenBoundsInPx.BottomRight);
            screenBoundsInDp = new Rect(screenBoundsTopLeftInDp.X, screenBoundsTopLeftInDp.Y, 
                screenBoundsBottomRightInDp.X - screenBoundsTopLeftInDp.X,
                screenBoundsBottomRightInDp.Y - screenBoundsTopLeftInDp.Y);

            ApplyState(getOpacity(), getWindowState(), getDockPosition(), getDockThicknessAsPercentageOfScreen(), getFloatingSizeAndPosition());
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
            throw new NotImplementedException();
        }

        public void Expand(ExpandToDirections direction, int? amountInDp)
        {
            throw new NotImplementedException();
        }

        public void ExpandDock()
        {
            throw new NotImplementedException();
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
        
        private void ApplyState(double opacity, WindowStates windowState, DockPositions? dockPosition,
            double dockThicknessAsPercentageOfScreen, Rect? floatingSizeAndPosition)
        {
            //CREATE OR COERCE STORED VALUES
            //try
            //{
            //    var windowSizeAndPosition = getWindowSizeAndPositionSetting();
            //    if (windowSizeAndPosition != null)
            //    {
            //        //Coerce size and position to screen
            //        var screen = window.GetScreen();
            //        var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            //        var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));
            //        var screenHeight = screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y;
            //        var screenWidth = screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X;

            //        window.Height = windowSizeAndPosition.Value.Height > screenHeight ? screenHeight : windowSizeAndPosition.Value.Height;
            //        window.Width = windowSizeAndPosition.Value.Width > screenWidth ? screenWidth : windowSizeAndPosition.Value.Width;

            //        window.Top = windowSizeAndPosition.Value.Top < screenTopLeftInWpfCoords.Y
            //            ? screenTopLeftInWpfCoords.Y
            //            : windowSizeAndPosition.Value.Top + window.Height > screenBottomRightInWpfCoords.Y
            //                ? screenBottomRightInWpfCoords.Y - window.Height
            //                : windowSizeAndPosition.Value.Top;

            //        window.Left = windowSizeAndPosition.Value.Left < screenTopLeftInWpfCoords.X
            //            ? screenTopLeftInWpfCoords.X
            //            : windowSizeAndPosition.Value.Left + window.Width > screenBottomRightInWpfCoords.X
            //                ? screenBottomRightInWpfCoords.X - window.Width
            //                : windowSizeAndPosition.Value.Left;
            //    }
            //    else
            //    {
            //        //Calculate initial size and position
            //        var screen = window.GetScreen();
            //        var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            //        var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));
            //        window.Left = screenTopLeftInWpfCoords.X;
            //        window.Top = screenTopLeftInWpfCoords.Y;
            //        window.Width = screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X;
            //        window.Height = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y) / 2;
            //    }

            //    window.Opacity = getOpacitySetting();
            //}
            //catch (Exception ex)
            //{
            //    PublishError(this, ex);
            //}
        }
        
        private void PersistState()
        {
            //    setWindowSizeAndPositionSetting(new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight));
            //Map window.WindowState to my version of WindowStates
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
