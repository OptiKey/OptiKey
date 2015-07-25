using System;
using System.Windows;
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
        private readonly Func<double> getOpacity;
        private readonly Func<bool> getCanDock;
        private readonly Func<int> getCollapsedDockThicknessAsPercentageOfFullDockThickness;
        private readonly Func<DockPositions?> getDockPosition;
        private readonly Func<int?> getFullHorizontalDockedThickness;
        private readonly Func<int?> getFullVerticalDockedThickness;
        private readonly Func<Rect?> getFloatingSizeAndPosition;
        private readonly Action<WindowStates> saveWindowState;
        private readonly Action<Rect?> saveFloatingSizeAndPosition;
        private readonly Action<DockPositions?> saveDockPosition;
        private readonly Action<int?> saveFullHorizontalDockedThickness;
        private readonly Action<int?> saveFullVerticalDockedThickness;
        private readonly Action<double> saveOpacity;

        #endregion

        #region Ctor
        
        internal WindowManipulationService(
            Window window,
            Func<WindowStates> getWindowState,
            Func<double> getOpacity,
            Func<bool> getCanDock,
            Func<int> getCollapsedDockThicknessAsPercentageOfFullDockThickness,
            Func<DockPositions?> getDockPosition,
            Func<int?> getFullHorizontalDockedThickness,
            Func<int?> getFullVerticalDockedThickness,
            Func<Rect?> getFloatingSizeAndPosition,
            Action<WindowStates> saveWindowState,
            Action<double> saveOpacity,
            Action<DockPositions?> saveDockPosition,
            Action<int?> saveFullHorizontalDockedThickness,
            Action<int?> saveFullVerticalDockedThickness,
            Action<Rect?> saveFloatingSizeAndPosition)
        {
            this.window = window;
            this.getOpacity = getOpacity;
            this.getCanDock = getCanDock;
            this.getCollapsedDockThicknessAsPercentageOfFullDockThickness = getCollapsedDockThicknessAsPercentageOfFullDockThickness;
            this.getDockPosition = getDockPosition;
            this.getFullHorizontalDockedThickness = getFullHorizontalDockedThickness;
            this.getFullVerticalDockedThickness = getFullVerticalDockedThickness;
            this.getFloatingSizeAndPosition = getFloatingSizeAndPosition;
            this.saveWindowState = saveWindowState;
            this.saveFloatingSizeAndPosition = saveFloatingSizeAndPosition;
            this.saveDockPosition = saveDockPosition;
            this.saveFullHorizontalDockedThickness = saveFullHorizontalDockedThickness;
            this.saveFullVerticalDockedThickness = saveFullVerticalDockedThickness;
            this.saveOpacity = saveOpacity;

            ApplyState(getWindowState(), getOpacity(), getCanDock(), getDockPosition(), 
                getFullHorizontalDockedThickness(), getFullVerticalDockedThickness(), getCollapsedDockThicknessAsPercentageOfFullDockThickness(),
                getFloatingSizeAndPosition());
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

        public void Maximise()
        {
            window.WindowState = WindowState.Maximized;
            PersistState();
        }

        public void Restore()
        {
            window.WindowState = WindowState.Normal;
            PersistState();
        }

        #endregion

        #region Private Methods
        
        private void ApplyState(WindowStates windowState, double opacity, bool canDock, DockPositions? dockPosition,
            int? fullHorizontalDockedThickness, int? fullVerticalDockedThickness, int collapsedDockThicknessAsPercentageOfFullDockThickness, 
            Rect? floatingSizeAndPosition)
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
