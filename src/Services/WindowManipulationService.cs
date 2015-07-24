using System;
using System.Windows;
using System.Windows.Interop;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;
        private readonly Func<WindowStates> getWindowStateSetting;
        private readonly Func<Rect?> getFloatingSizeAndPositionSetting;
        private readonly Action<Rect?> setFloatingSizeAndPositionSetting;
        private readonly Func<DockPositions?> getDockPositionSetting;
        private readonly Func<int?> getMinimisedWidthSetting;
        private readonly Func<int?> getMinimisedHeightSetting;
        private readonly Func<int?> getCollapsedDockedThicknessSetting;
        private readonly Func<int?> getFullDockedThicknessSetting;
        private readonly Func<int?> getFullDockThicknessSetting;
        private readonly Func<double> getOpacitySetting;
        private readonly Action<double> setOpacitySetting;
        private readonly Settings settings;

        #endregion

        #region Ctor
        
        internal WindowManipulationService(
            Window window,
            Func<WindowStates> getWindowStateSetting,
            Func<Rect?> getFloatingSizeAndPositionSetting,
            Action<Rect?> setFloatingSizeAndPositionSetting,
            Func<DockPositions?> getDockPositionSetting,
            Func<int?> getMinimisedWidthSetting,
            Func<int?> getMinimisedHeightSetting,
            Func<int?> getCollapsedDockedThicknessSetting,
            Func<int?> getFullDockedThicknessSetting,
            Func<double> getOpacitySetting,
            Action<double> setOpacitySetting,
            Settings settings)
        {
            this.window = window;
            this.getWindowStateSetting = getWindowStateSetting;
            this.getFloatingSizeAndPositionSetting = getFloatingSizeAndPositionSetting;
            this.setFloatingSizeAndPositionSetting = setFloatingSizeAndPositionSetting;
            this.getDockPositionSetting = getDockPositionSetting;
            this.getMinimisedWidthSetting = getMinimisedWidthSetting;
            this.getMinimisedHeightSetting = getMinimisedHeightSetting;
            this.getCollapsedDockedThicknessSetting = getCollapsedDockedThicknessSetting;
            this.getFullDockedThicknessSetting = getFullDockedThicknessSetting;
            this.getOpacitySetting = getOpacitySetting;
            this.setOpacitySetting = setOpacitySetting;
            this.settings = settings;

            Initialise();
        }

        #endregion
        
        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Public Methods

        public void DecreaseOpacity()
        {
            window.Opacity -= 0.1;
            if (window.Opacity < 0.1)
            {
                window.Opacity = 0.1;
            }
            PersistWindowOpacity();
        }

        public void Dock(DockSizes dockSize)
        {
            //docked = true;
            //var dockPosition = getDockPositionSetting();
            //double dockedThickness;
            //if (getDockedThicknessSetting() == null)
            //{
            //    var sizeAndPosition = getWindowSizeAndPositionSetting().Value; //This should never be null as it is initialised in the ctor
            //    switch (dockPosition)
            //    {
            //        case DockPositions.Top:
            //        case DockPositions.Bottom:
            //            dockedThickness = sizeAndPosition.Width / 11; //The alpha keyboard is 11 keys wide
            //            break;

            //        case DockPositions.Left:
            //        case DockPositions.Right:
            //            dockedThickness = sizeAndPosition.Height / 6; //The alpha keyboard is 6 keys tall
            //            break;

            //        default:
            //            dockedThickness = getDockedThicknessSetting().Value;
            //            break;
            //    }
            //}
            //else
            //{
            //    dockedThickness = getDockedThicknessSetting().Value;
            //}

            //var screen = window.GetScreen();
            //var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            //var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));
            //var screenWidth = (screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X);
            //var screenHeight = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y);

            //switch (dockPosition)
            //{
            //    case DockPositions.Top:
            //        window.Height = dockedThickness;
            //        window.Width = screenWidth;
            //        MoveToTopAndLeftBoundaries();
            //        break;

            //    case DockPositions.Bottom:
            //        window.Height = dockedThickness;
            //        window.Width = screenWidth;
            //        MoveToBottomAndLeftBoundaries();
            //        break;

            //    case DockPositions.Left:
            //        window.Height = screenHeight;
            //        window.Width = dockedThickness;
            //        MoveToTopAndLeftBoundaries();
            //        break;

            //    case DockPositions.Right:
            //        window.Height = screenHeight;
            //        window.Width = dockedThickness;
            //        MoveToTopAndRightBoundaries();
            //        break;
            //}
        }

        public void ExpandToBottom(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Height += yAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToBottomAndLeft(double pixels)
        {
            ExpandToBottom(pixels);
            ExpandToLeft(pixels);
        }

        public void ExpandToBottomAndRight(double pixels)
        {
            ExpandToBottom(pixels);
            ExpandToRight(pixels);
        }
        
        public void ExpandToLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);

                var widthBeforeAdjustment = window.ActualWidth;
                window.Width += xAdjustment;
                var actualXAdjustment = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                window.Left -= actualXAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {   
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Width += xAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToTop(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);

                var heightBeforeAdjustment = window.ActualHeight;
                window.Height += yAdjustment;
                var actualYAdjustment = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                window.Top -= actualYAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToTopAndLeft(double pixels)
        {
            ExpandToTop(pixels);
            ExpandToLeft(pixels);
        }

        public void ExpandToTopAndRight(double pixels)
        {
            ExpandToTop(pixels);
            ExpandToRight(pixels);
        }

        public void Float()
        {
            throw new NotImplementedException();
        }

        public void IncreaseOpacity()
        {
            window.Opacity += 0.1;
            if (window.Opacity > 1)
            {
                window.Opacity = 1;
            }
            PersistWindowOpacity();
        }
        
        public void Maximise()
        {
            //docked = false;
            //minimised = false;
            //window.WindowState = WindowState.Maximized;
        }

        public void Minimise()
        {
            //try
            //{
            //    minimised = true; //Do this first to prevent the calls to move methods from persisting the current size/position

            //    window.WindowState = WindowState.Normal;
                
            //    //Set width
            //    double minimisedWidth;
            //    var minimisedWidthSetting = getMinimisedWidthSetting();
            //    if (minimisedWidthSetting == null)
            //    {
            //        var sizeAndPosition = getWindowSizeAndPositionSetting().Value; //This should never be null as it is initialised in the ctor
            //        minimisedWidth = sizeAndPosition.Width / 11; //The alpha keyboard is 11 keys wide
            //    }
            //    else
            //    {
            //        minimisedWidth = minimisedWidthSetting.Value / Graphics.DipScalingFactorX;
            //    }
            //    window.Width = minimisedWidth;

            //    //Set height
            //    double minimisedHeight;
            //    var minimisedHeightSetting = getMinimisedHeightSetting();
            //    if (minimisedHeightSetting == null)
            //    {
            //        var sizeAndPosition = getWindowSizeAndPositionSetting().Value; //This should never be null as it is initialised in the ctor
            //        minimisedHeight = sizeAndPosition.Height / 6; //The alpha keyboard is 6 keys tall
            //    }
            //    else
            //    {
            //        minimisedHeight = getMinimisedHeightSetting().Value / Graphics.DipScalingFactorY;
            //    }
            //    window.Height = minimisedHeight;

            //    //Set position
            //    var screen = window.GetScreen();
            //    var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            //    var screenDimensionsInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Width, screen.Bounds.Height));
            //    switch (getMinimisePosition())
            //    {
            //        case MinimisePositions.BottomEdge:
            //            MoveToBottomBoundary();
            //            window.Left = screenTopLeftInWpfCoords.X + (screenDimensionsInWpfCoords.X/2) - (minimisedWidth / 2);
            //            break;

            //        case MinimisePositions.BottomLeftCorner:
            //            MoveToBottomAndLeftBoundaries();
            //            break;

            //        case MinimisePositions.BottomRightCorner:
            //            MoveToBottomAndRightBoundaries();
            //            break;

            //        case MinimisePositions.LeftEdge:
            //            MoveToLeftBoundary();
            //            window.Top = screenTopLeftInWpfCoords.Y + (screenDimensionsInWpfCoords.Y / 2) - (minimisedHeight / 2);
            //            break;

            //        case MinimisePositions.RightEdge:
            //            MoveToRightBoundary();
            //            window.Top = screenTopLeftInWpfCoords.Y + (screenDimensionsInWpfCoords.Y / 2) - (minimisedHeight / 2);
            //            break;

            //        case MinimisePositions.TopEdge:
            //            MoveToTopBoundary();
            //            window.Left = screenTopLeftInWpfCoords.X + (screenDimensionsInWpfCoords.X / 2) - (minimisedWidth / 2);
            //            break;

            //        case MinimisePositions.TopLeftCorner:
            //            MoveToTopAndLeftBoundaries();
            //            break;

            //        case MinimisePositions.TopRightCorner:
            //            MoveToTopAndRightBoundaries();
            //            break;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    PublishError(this, ex);
            //}
        }
        
        public void MoveToBottom(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Top += yAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }
        
        public void MoveToBottomAndLeft(double pixels)
        {
            MoveToBottom(pixels);
            MoveToLeft(pixels);
        }
        
        public void MoveToBottomAndLeftBoundaries()
        {
            MoveToBottomBoundary();
            MoveToLeftBoundary();
        }
        
        public void MoveToBottomAndRight(double pixels)
        {
            MoveToBottom(pixels);
            MoveToRight(pixels);
        }
        
        public void MoveToBottomAndRightBoundaries()
        {
            MoveToBottomBoundary();
            MoveToRightBoundary();
        }
        
        public void MoveToBottomBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary;
                window.Top += yAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Left -= xAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToLeftBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary;
                window.Left -= xAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {   
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Left += xAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToRightBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {   
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary;
                window.Left += xAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToTop(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Top -= yAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }
        
        public void MoveToTopAndLeft(double pixels)
        {
            MoveToTop(pixels);
            MoveToLeft(pixels);
        }
        
        public void MoveToTopAndLeftBoundaries()
        {
            MoveToTopBoundary();
            MoveToLeftBoundary();
        }
        
        public void MoveToTopAndRight(double pixels)
        {
            MoveToTop(pixels);
            MoveToRight(pixels);
        }
        
        public void MoveToTopAndRightBoundaries()
        {
            MoveToTopBoundary();
            MoveToRightBoundary();
        }

        public void MoveToTopBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary;
                window.Top -= yAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void Restore()
        {
            //docked = false;
            //minimised = false;
            //window.WindowState = WindowState.Normal;
            //var rect = getWindowSizeAndPositionSetting();
            //if (rect.HasValue)
            //{
            //    window.Top = rect.Value.Top;
            //    window.Left = rect.Value.Left;
            //    window.Width = rect.Value.Width;
            //    window.Height = rect.Value.Height;
            //}
        }

        public void ShrinkFromBottom(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;
                window.Height += yAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromBottomAndLeft(double pixels)
        {
            ShrinkFromBottom(pixels);
            ShrinkFromLeft(pixels);
        }

        public void ShrinkFromBottomAndRight(double pixels)
        {
            ShrinkFromBottom(pixels);
            ShrinkFromRight(pixels);
        }

        public void ShrinkFromLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;

                var widthBeforeAdjustment = window.ActualWidth;
                window.Width += xAdjustment;
                var actualXAdjustment = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                window.Left -= actualXAdjustment;
                
                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;
                window.Width += xAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromTop(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;

                var heightBeforeAdjustment = window.ActualHeight;
                window.Height += yAdjustment;
                var actualYAdjustment = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                window.Top -= actualYAdjustment;

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromTopAndLeft(double pixels)
        {
            ShrinkFromTop(pixels);
            ShrinkFromLeft(pixels);
        }

        public void ShrinkFromTopAndRight(double pixels)
        {
            ShrinkFromTop(pixels);
            ShrinkFromRight(pixels);
        }

        #endregion

        #region Private Methods
        
        private void Initialise()
        {
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

        private void PersistWindowOpacity()
        {
            setOpacitySetting(window.Opacity);
            settings.Save();
        }

        private void PersistWindowSizeAndPosition()
        {
            //if (window.WindowState == WindowState.Normal && !minimised && !docked)
            //{
            //    setWindowSizeAndPositionSetting(new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight));
            //    settings.Save();
            //}
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
