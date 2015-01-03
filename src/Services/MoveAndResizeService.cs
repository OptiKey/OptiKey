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
                //Calculate adjustment in device (screen) co-ordinates
                var screenBottom = window.GetScreen().Bounds.Bottom;
                var windowBottom = window.PointToScreen(new Point(0, window.Top + window.ActualHeight)).Y;
                var distanceToBoundary = screenBottom - windowBottom;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                
                //Convert back into resolution independent co-ord system
                var wpfAdjustment = (Size) window.GetTransformFromDevice().Transform(new Vector(0, yAdjustment));

                if (yAdjustment > 0)
                {
                    window.Height += wpfAdjustment.Height;
                }
                else
                {
                    window.Height -= wpfAdjustment.Height;
                }
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
                var screenLeftInDeviceCoords = window.GetScreen().Bounds.Left;
                var screenLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screenLeftInDeviceCoords, 0)).X;
                
                var distanceToBoundary = window.Left - screenLeftInWpfCoords;
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
                var screenRightInDeviceCoords = window.GetScreen().Bounds.Right;
                var screenRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screenRightInDeviceCoords, 0)).X;
                var windowRight = window.Left + window.ActualWidth;
                var distanceToBoundary = screenRightInWpfCoords - windowRight;
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
                //Calculate adjustment in device (screen) co-ordinates
                var screenTop = window.GetScreen().Bounds.Top;
                var windowTop = window.PointToScreen(new Point(0, window.Top)).Y;
                var distanceToBoundary = windowTop - screenTop;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);

                //Convert back into resolution independent co-ord system
                var wpfAdjustment = (Size)window.GetTransformFromDevice().Transform(new Vector(0, yAdjustment));

                if (yAdjustment > 0)
                {
                    window.Top -= wpfAdjustment.Height;
                    window.Height += wpfAdjustment.Height;
                }
                else
                {
                    window.Top += wpfAdjustment.Height;
                    window.Height -= wpfAdjustment.Height;
                }
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
