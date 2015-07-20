using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService, INotifyPropertyChanged
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;
        private readonly Func<double> getWindowTopSetting;
        private readonly Func<double> getWindowLeftSetting;
        private readonly Func<double> getWindowHeightSetting;
        private readonly Func<double> getWindowWidthSetting;
        private readonly Func<WindowState> getWindowStateSetting;

        private bool otherWindowsShouldBeMaximisedOnExit;
        private bool windowIsMinimised;
        private Size? windowSizeBeforeMinimise;
        private Point? windowPositionBeforeMinimise;

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
            this.getWindowLeftSetting = getWindowLeftSetting;
            this.getWindowHeightSetting = getWindowHeightSetting;
            this.getWindowWidthSetting = getWindowWidthSetting;
            this.getWindowStateSetting = getWindowStateSetting;
            
            if(loadSavedState)
            {
                LoadState();
            }

            window.Closing += (sender, args) =>
            {
                if (saveStateOnClose)
                {
                    //Check window is not in minimised state (either as a ssmall icon on the screen, or the window is actually minimised)
                    if (!windowIsMinimised 
                        && window.WindowState != WindowState.Minimized
                        && window.WindowState != WindowState.Maximized)
                    {
                        setWindowTopSetting(window.Top);
                        setWindowLeftSetting(window.Left);
                        setWindowHeightSetting(window.Height);
                        setWindowWidthSetting(window.Width);
                        setWindowStateSetting(window.WindowState);

                        settings.Save();
                    }
                }

                if (otherWindowsShouldBeMaximisedOnExit)
                {
                    //Maximise all windows if they were arranged at some point
                    var thisHwnd = new WindowInteropHelper(window).Handle;
                    var otherWindows = Windows.GetVisibleOverlappedWindows(thisHwnd);
                    foreach (var otherWindow in otherWindows)
                    {
                        Log.DebugFormat("Maximising window '{0}' before we exit", otherWindow.Item1);
                        PInvoke.ShowWindow(otherWindow.Item2, (int)WindowShowStyle.Maximize);
                    }
                }
            };
        }
        
        #endregion
        
        #region Events

        public event EventHandler<Exception> Error;

        #endregion
        
        #region Public Methods

        public void ArrangeWindowsHorizontally()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var windowTopLeftInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left, window.Top));
                var distanceToTopBoundaryInPixels = Convert.ToInt32(windowTopLeftInScreenCoords.Y - screen.Bounds.Top);
                var windowBottomRightInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left + window.ActualWidth, window.Top + window.ActualHeight));
                var distanceToBottomBoundaryInPixels = Convert.ToInt32(screen.Bounds.Bottom - windowBottomRightInScreenCoords.Y);

                var taskBar = new Taskbar();
                var taskBarOnCurrentScreen = taskBar.Bounds.IntersectsWith(screen.Bounds);

                int x = screen.Bounds.Left;
                int width = screen.Bounds.Width;

                //Compensate for left/right aligned taskbar
                if (taskBarOnCurrentScreen
                    && !taskBar.AutoHide
                    && taskBar.Position == TaskbarPosition.Left)
                {
                    x += taskBar.Size.Width;
                    width -= taskBar.Size.Width;
                }
                else if (taskBarOnCurrentScreen
                    && !taskBar.AutoHide 
                    && taskBar.Position == TaskbarPosition.Right)
                {
                    width -= taskBar.Size.Width;
                }

                if (distanceToTopBoundaryInPixels > 0 && distanceToTopBoundaryInPixels > distanceToBottomBoundaryInPixels)
                {
                    //Arrange windows above OptiKey
                    int y = screen.Bounds.Top;
                    int height = distanceToTopBoundaryInPixels;

                    //Compensate for top aligned taskbar
                    if (taskBarOnCurrentScreen
                        && !taskBar.AutoHide 
                        && taskBar.Position == TaskbarPosition.Top)
                    {
                        y += taskBar.Size.Height;
                        height -= taskBar.Size.Height;
                    }

                    ArrangeOtherWindows(x, y, width, height);
                }
                else if (distanceToBottomBoundaryInPixels > 0 && distanceToBottomBoundaryInPixels > distanceToTopBoundaryInPixels)
                {
                    //Arrange windows below OptiKey
                    int y = Convert.ToInt32(windowBottomRightInScreenCoords.Y);
                    int height = distanceToBottomBoundaryInPixels;
                    
                    //Compensate for bottom aligned taskbar
                    if (taskBarOnCurrentScreen
                        && !taskBar.AutoHide 
                        && taskBar.Position == TaskbarPosition.Bottom)
                    {
                        height -= taskBar.Size.Height;
                    }
                    
                    ArrangeOtherWindows(x, y, width, height);
                }
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ArrangeWindowsMaximised()
        {
            try
            {
                var thisHwnd = new WindowInteropHelper(window).Handle;
                var otherWindows = Windows.GetVisibleOverlappedWindows(thisHwnd);
                foreach (var otherWindow in otherWindows)
                {
                    Log.DebugFormat("Maximising window '{0}'", otherWindow.Item1);
                    PInvoke.ShowWindow(otherWindow.Item2, (int) WindowShowStyle.ShowMaximized);
                }
                otherWindowsShouldBeMaximisedOnExit = false;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ArrangeWindowsVertically()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var windowTopLeftInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left, window.Top));
                var distanceToLeftBoundaryInPixels = Convert.ToInt32(windowTopLeftInScreenCoords.X - screen.Bounds.Left);
                var windowBottomRightInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left + window.ActualWidth, window.Top + window.ActualHeight));
                var distanceToRightBoundaryInPixels = Convert.ToInt32(screen.Bounds.Right - windowBottomRightInScreenCoords.X);

                var taskBar = new Taskbar();
                var taskBarOnCurrentScreen = taskBar.Bounds.IntersectsWith(screen.Bounds);

                int y = screen.Bounds.Top;
                int height = screen.Bounds.Height;

                //Compensate for top/bottom aligned taskbar
                if (taskBarOnCurrentScreen
                    && !taskBar.AutoHide 
                    && taskBar.Position == TaskbarPosition.Top)
                {
                    y += taskBar.Size.Height;
                    height -= taskBar.Size.Height;
                }
                else if (taskBarOnCurrentScreen
                    && !taskBar.AutoHide 
                    && taskBar.Position == TaskbarPosition.Bottom)
                {
                    height -= taskBar.Size.Height;
                }

                if (distanceToLeftBoundaryInPixels > 0 && distanceToLeftBoundaryInPixels > distanceToRightBoundaryInPixels)
                {
                    //Arrange windows to left of OptiKey
                    int x = screen.Bounds.Left;
                    int width = distanceToLeftBoundaryInPixels;

                    //Compensate for left aligned taskbar
                    if (taskBarOnCurrentScreen
                        && !taskBar.AutoHide 
                        && taskBar.Position == TaskbarPosition.Left)
                    {
                        x += taskBar.Size.Width;
                        width -= taskBar.Size.Width;
                    }

                    ArrangeOtherWindows(x, y, width, height);
                }
                else if (distanceToRightBoundaryInPixels > 0 && distanceToRightBoundaryInPixels > distanceToLeftBoundaryInPixels)
                {
                    //Arrange windows to right of OptiKey
                    int x = Convert.ToInt32(windowBottomRightInScreenCoords.X);
                    int width = distanceToRightBoundaryInPixels;
                    
                    //Compensate for right aligned taskbar
                    if (taskBarOnCurrentScreen
                        && !taskBar.AutoHide 
                        && taskBar.Position == TaskbarPosition.Right)
                    {
                        width -= taskBar.Size.Width;
                    }

                    ArrangeOtherWindows(x, y, width, height);
                }
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
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

        public void IncreaseOpacity()
        {
            window.Opacity += 0.1;
            if (window.Opacity > 1)
            {
                window.Opacity = 1;
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
        }

        public void MinimiseToBottomAndLeftBoundaries()
        {
            Minimise();
            MoveToBottomAndLeftBoundaries();
        }

        public void MinimiseToBottomAndRightBoundaries()
        {
            Minimise();
            MoveToBottomAndRightBoundaries();
        }

        public void MinimiseToMiddleOfBottomBoundary()
        {
            Minimise();
            MoveToBottomBoundary();
            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var screenDimensionsInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Width, screen.Bounds.Height));
                window.Left = screenTopLeftInWpfCoords.X + (screenDimensionsInWpfCoords.X/2) - (Settings.Default.MinimisedWidthInPixels / 2);
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MinimiseToMiddleOfLeftBoundary()
        {
            Minimise(invertDimensions:true);
            MoveToLeftBoundary();
            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var screenDimensionsInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Width, screen.Bounds.Height));
                window.Top = screenTopLeftInWpfCoords.Y + (screenDimensionsInWpfCoords.Y / 2) - (Settings.Default.MinimisedHeightInPixels / 2);
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MinimiseToMiddleOfRightBoundary()
        {
            Minimise(invertDimensions: true);
            MoveToRightBoundary();
            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var screenDimensionsInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Width, screen.Bounds.Height));
                window.Top = screenTopLeftInWpfCoords.Y + (screenDimensionsInWpfCoords.Y / 2) - (Settings.Default.MinimisedHeightInPixels / 2);
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MinimiseToMiddleOfTopBoundary()
        {
            Minimise();
            MoveToTopBoundary();
            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var screenDimensionsInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Width, screen.Bounds.Height));
                window.Left = screenTopLeftInWpfCoords.X + (screenDimensionsInWpfCoords.X / 2) - (Settings.Default.MinimisedWidthInPixels / 2);
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MinimiseToTopAndLeftBoundaries()
        {
            Minimise();
            MoveToTopAndLeftBoundaries();
        }

        public void MinimiseToTopAndRightBoundaries()
        {
            Minimise();
            MoveToTopAndRightBoundaries();
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

        public void RestoreFromMaximised()
        {
            window.WindowState = WindowState.Normal;
        }

        public void RestoreFromMinimised()
        {
            if (windowPositionBeforeMinimise != null)
            {
                window.Top = windowPositionBeforeMinimise.Value.Y;
                window.Left = windowPositionBeforeMinimise.Value.X;
            }
            if (windowSizeBeforeMinimise != null)
            {
                window.Width = windowSizeBeforeMinimise.Value.Width;
                window.Height = windowSizeBeforeMinimise.Value.Height;
            }
            ClearStateBeforeMinimise();
            windowIsMinimised = false;
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

        #endregion

        #region Private Methods

        private void ArrangeOtherWindows(int x, int y, int width, int height)
        {
            otherWindowsShouldBeMaximisedOnExit = true;
            var thisHwnd = new WindowInteropHelper(window).Handle;
            var otherWindows = Windows.GetVisibleOverlappedWindows(thisHwnd);
            foreach (var otherWindow in otherWindows)
            {
                Log.DebugFormat("Restoring and arranging window '{0}' to position ({1},{2}) and size ({3},{4})", otherWindow.Item1, x, y, width, height);
                var otherWindowHandle = otherWindow.Item2;
                PInvoke.ShowWindow(otherWindowHandle, (int)WindowShowStyle.ShowNormal); //Restore windows as resizing windows that are in a minimised/maximised state can break the minimise/maximise button
                PInvoke.SetWindowPos(otherWindowHandle, IntPtr.Zero, x, y, width, height,
                    SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOZORDER);
            }
        }

        private void ClearStateBeforeMinimise()
        {
            windowPositionBeforeMinimise = null;
            windowSizeBeforeMinimise = null;
        }

        private void Minimise(bool invertDimensions = false)
        {
            StoreStateBeforeMinimise();
            window.Width = invertDimensions
                ? Settings.Default.MinimisedHeightInPixels / Graphics.DipScalingFactorY
                : Settings.Default.MinimisedWidthInPixels / Graphics.DipScalingFactorX;
            window.Height = invertDimensions
                ? Settings.Default.MinimisedWidthInPixels / Graphics.DipScalingFactorX
                : Settings.Default.MinimisedHeightInPixels / Graphics.DipScalingFactorY;
            windowIsMinimised = true;
        }

        private void StoreStateBeforeMinimise()
        {
            windowPositionBeforeMinimise = new Point(window.Left, window.Top);
            windowSizeBeforeMinimise = new Size(window.ActualWidth, window.ActualHeight);
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
