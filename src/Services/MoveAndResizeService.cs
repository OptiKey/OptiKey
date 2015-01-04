using System;
using System.Windows;
using JuliusSweetland.ETTA.Extensions;
using log4net;

namespace JuliusSweetland.ETTA.Services
{
    public class MoveAndResizeService : IMoveAndResizeService
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;

        public MoveAndResizeService(Window window)
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
                var windowBottom = window.Top
                                   + window.ActualHeight;
                                   //+ (SystemParameters.WindowNonClientFrameThickness.Bottom / SystemParameters.BorderWidth); //BorderWidth is a multiplier based on DPI (e.g. scaling factor 100% = 1.0, scaling factor 125% = 0.8, etc)
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - windowBottom;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Height += yAdjustment;
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
                window.Left -= xAdjustment;
                window.Width += xAdjustment;
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
                var windowRight = window.Left
                                  + window.ActualWidth;
                                  //+ (SystemParameters.WindowNonClientFrameThickness.Right / SystemParameters.BorderWidth); //BorderWidth is a multiplier based on DPI (e.g. scaling factor 100% = 1.0, scaling factor 125% = 0.8, etc)
                var distanceToBoundary = screenTopRightInWpfCoords.X - windowRight;
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Width += xAdjustment;
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
                window.Top -= yAdjustment;
                window.Height += yAdjustment;
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

        public void MoveLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void MoveLeftToBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void MoveRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void MoveRightToBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void MoveUp(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void MoveUpToBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void MoveDown(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void MoveDownToBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void Restore()
        {
            window.WindowState = WindowState.Normal;
        }

        public void ShrinkFromBottom(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void ShrinkFromBottomAndLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void ShrinkFromBottomAndRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void ShrinkFromLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void ShrinkFromRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void ShrinkFromTop(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void ShrinkFromTopAndLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
        }

        public void ShrinkFromTopAndRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;
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
