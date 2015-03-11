using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService, INotifyPropertyChanged
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly Window window;
        private readonly Func<double> getWindowTopSetting;
        private readonly Action<double> setWindowTopSetting;
        private readonly Func<double> getWindowLeftSetting;
        private readonly Action<double> setWindowLeftSetting;
        private readonly Func<double> getWindowHeightSetting;
        private readonly Action<double> setWindowHeightSetting;
        private readonly Func<double> getWindowWidthSetting;
        private readonly Action<double> setWindowWidthSetting;
        private readonly Func<WindowState> getWindowStateSetting;
        private readonly Action<WindowState> setWindowStateSetting;
        private readonly Settings settings;

        #endregion

        #region Ctor
        
        internal WindowManipulationService(
            Window window,
            Func<double> getWindowTopSetting,
            Action<double> setWindowTopSetting,
            Func<double> getWindowLeftSetting,
            Action<double> setWindowLeftSetting,
            Func<double> getWindowHeightSetting,
            Action<double> setWindowHeightSetting,
            Func<double> getWindowWidthSetting,
            Action<double> setWindowWidthSetting,
            Func<WindowState> getWindowStateSetting,
            Action<WindowState> setWindowStateSetting,
            Settings settings,
            bool loadSavedState = false,
            bool saveStateOnClose = false)
        {
            this.window = window;
            this.getWindowTopSetting = getWindowTopSetting;
            this.setWindowTopSetting = setWindowTopSetting;
            this.getWindowLeftSetting = getWindowLeftSetting;
            this.setWindowLeftSetting = setWindowLeftSetting;
            this.getWindowHeightSetting = getWindowHeightSetting;
            this.setWindowHeightSetting = setWindowHeightSetting;
            this.getWindowWidthSetting = getWindowWidthSetting;
            this.setWindowWidthSetting = setWindowWidthSetting;
            this.getWindowStateSetting = getWindowStateSetting;
            this.setWindowStateSetting = setWindowStateSetting;
            this.settings = settings;
            
            if(loadSavedState)
            {
                LoadState();
            }
            
            if(saveStateOnClose)
            {
                window.Closing += (sender, args) => SaveState();
            }
        }
        
        #endregion
        
        #region Events

        public event EventHandler<Exception> Error;

        #endregion
        
        #region Public Methods

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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
                var actualXAdjustment = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                window.Left -= actualXAdjustment;
                
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
                var actualYAdjustment = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                window.Top -= actualYAdjustment;
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

        public void FillPercentageOfScreen(double horizontalPercentage, double verticalPercentage)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));

                var screenWidth = (screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X);
                var screenHeight = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y);

                var distanceFromLeftBoundary = ((1d - (horizontalPercentage / 100)) / 2d) * screenWidth;
                var distanceFromTopBoundary = ((1d - (verticalPercentage / 100)) / 2d) * screenHeight;

                window.Left = screenTopLeftInWpfCoords.X + distanceFromLeftBoundary;
                window.Top = screenTopLeftInWpfCoords.Y + distanceFromTopBoundary;

                var width = (horizontalPercentage / 100) * screenWidth;
                var height = (verticalPercentage / 100) * screenHeight;

                window.Width = width;
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth

                window.Height = height;
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Height property out of sync with the ActualHeight
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinWHeight - not doing this leaves the Height property out of sync with the ActualHeight

            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }
        
        public void LoadState()
        {
            var windowTop = getWindowTopSetting();
            var windowLeft = getWindowLeftSetting();
            var windowHeight = getWindowHeightSetting();
            var windowWidth = getWindowWidthSetting();
            var windowState = getWindowStateSetting();

            //Coerce window height - cannot be taller than virtual screen height

            if (windowHeight > SystemParameters.VirtualScreenHeight) //N.B. Virtual screen height/width are already is DIP
            {
                windowHeight = SystemParameters.VirtualScreenHeight;
            }

            //Coerce window width - cannot be wider than virtual screen width
            if (windowWidth > SystemParameters.VirtualScreenWidth)
            {
                windowWidth = SystemParameters.VirtualScreenWidth;
            }
            
            //Coerce vertical position - cannot be outside virtual screen
            if (windowTop < SystemParameters.VirtualScreenTop)
            {
                windowTop = SystemParameters.VirtualScreenTop;
            }
            else if ((windowTop + windowHeight) > (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight))
            {
                windowTop = (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight) - windowHeight;
            }

            //Coerce horizontal position - cannot be outside virtual screen
            if (windowLeft < SystemParameters.VirtualScreenLeft)
            {
                windowLeft = SystemParameters.VirtualScreenLeft;
            }
            else if ((windowLeft + windowWidth) > (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth))
            {
                windowLeft = (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth) - windowWidth;
            }
            
            window.Height = windowHeight;
            window.Width = windowWidth;
            window.Top = windowTop;
            window.Left = windowLeft;
            window.WindowState = windowState;
        }

        public void Maximise()
        {
            window.WindowState = WindowState.Maximized;
            OnPropertyChanged("CanRestore");
            OnPropertyChanged("CanMaximise");
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
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void Restore()
        {
            window.WindowState = WindowState.Normal;
            OnPropertyChanged("CanRestore");
            OnPropertyChanged("CanMaximise");
        }
        
        public void SaveState()
        {
            if (window.WindowState != WindowState.Minimized)
            {
                setWindowTopSetting(window.Top);
                setWindowLeftSetting(window.Left);
                setWindowHeightSetting(window.Height);
                setWindowWidthSetting(window.Width);
                setWindowStateSetting(window.WindowState);

                settings.Save();
            }
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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
                var actualXAdjustment = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                window.Left -= actualXAdjustment;
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
                var actualYAdjustment = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                window.Top -= actualYAdjustment;
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

        public void IncreaseOpacity()
        {
            window.Opacity += 0.1;
            if (window.Opacity > 1)
            {
                window.Opacity = 1;
            }
        }

        public void DecreaseOpacity()
        {
            window.Opacity -= 0.1;
            if (window.Opacity < 0.1)
            {
                window.Opacity = 0.1;
            }
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
        
        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
