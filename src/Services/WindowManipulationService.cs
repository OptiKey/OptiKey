using System;
using System.Windows;
using JuliusSweetland.OptiKey.Extensions;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;

        public WindowManipulationService(Window window)
        {
            this.window = window;
        }

        public event EventHandler<Exception> Error;

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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth
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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight
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

        public void Maximise()
        {
            window.WindowState = WindowState.Maximized;
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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth
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
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth
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
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight
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
