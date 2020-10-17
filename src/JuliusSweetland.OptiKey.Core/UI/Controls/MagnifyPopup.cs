// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.ViewModels;
using log4net;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class MagnifyPopup : Popup
    {
        #region Private member vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window window;
        private Screen screen;
        private Rect sourceArea;

        #endregion

        #region Ctor

        public MagnifyPopup()
        {
            Loaded += OnLoaded;
        }

        #endregion

        #region OnLoaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Get references to window, screen and mainViewModel
            window = Window.GetWindow(this);
            screen = window.GetScreen();
            var mainViewModel = DataContext as MainViewModel;

            Log.Info($"Screen details resolved following an OnLoaded event. Bounds={screen.Bounds}, WorkingArea={screen.WorkingArea} and IsPrimary={screen.Primary}");

            //Subscribe to window location changes and re-evaluate the current screen and current position
            Observable.FromEventPattern<EventHandler, EventArgs>
                (h => window.LocationChanged += h,
                    h => window.LocationChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    screen = window.GetScreen();
                    Log.Info($"Screen details resolved following a Window.LocationChanged event. Bounds={screen.Bounds}, WorkingArea={screen.WorkingArea} and IsPrimary={screen.Primary}");
                });

            //Listen for MagnifyPoint changes
            mainViewModel.OnPropertyChanges(vm => vm.MagnifyAtPoint)
                .Subscribe(sourcePoint =>
                {
                    if (sourcePoint != null)
                    {
                        SetSizeAndPosition(sourcePoint.Value);

                        try
                        {
                            DisplayScaledScreenshot();
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.ERROR_TITLE,
                                OptiKey.Properties.Resources.ERROR_MAGNIFYING,
                                NotificationTypes.Error, () => { });

                            Log.ErrorFormat("Caught exception: {0}", ex);

                            //Reset as much as possible
                            mainViewModel.SelectionMode = SelectionModes.Key;
                            mainViewModel.MagnifiedPointSelectionAction = null;
                            mainViewModel.MagnifyAtPoint = null;
                            mainViewModel.MagnifiedPointSelectionAction = null;
                            mainViewModel.ShowCursor = false;

                            //Return so the rest of the workflow is avoided
                            return;
                        }

                        EventHandler<Point> pointSelectionHandler = null;
                        pointSelectionHandler = (pointSelectionSender, point) =>
                        {
                            mainViewModel.PointSelection -= pointSelectionHandler; //Only react to one PointSelection event

                            Point? destinationPoint = TranslateMagnifiedSelectionPoint(point);

                            IsOpen = false; //Close popup before clicking - destination point may be under the magnified image

                            if (mainViewModel.MagnifiedPointSelectionAction != null)
                            {
                                mainViewModel.MagnifiedPointSelectionAction(destinationPoint);
                            }
                        };
                        mainViewModel.PointSelection += pointSelectionHandler;

                        IsOpen = true;
                    }
                });

            Loaded -= OnLoaded;
        }

        #endregion

        #region Set Size And Position

        private void SetSizeAndPosition(Point point)
        {
            point = new Point(point.X.Clamp(screen.Bounds.Left, screen.Bounds.Right), point.Y.Clamp(screen.Bounds.Top, screen.Bounds.Bottom));

            var centerOnScreen = Settings.Default.MagnifierCenterOnScreen;
            var magnifySourcePercentage = Settings.Default.MagnifySourcePercentageOfScreen / 100d;
            var destinationPercentage = Settings.Default.MagnifyDestinationPercentageOfScreen / 100d;

            var captureWidth = magnifySourcePercentage * screen.Bounds.Width;
            var captureHeight = magnifySourcePercentage * screen.Bounds.Height;
            var captureX = (point.X - (captureWidth / 2d)).Clamp(screen.Bounds.Left, screen.Bounds.Right - captureWidth);
            var captureY = (point.Y - (captureHeight / 2d)).Clamp(screen.Bounds.Top, screen.Bounds.Bottom - captureHeight);

            //Calculate source area
            sourceArea = new Rect(captureX, captureY, captureWidth, captureHeight);

            //Get WPF coords in order to calculate the magnified size and position
            var pointInWpfCoords = window.GetTransformFromDevice().Transform(new Point(point.X, point.Y));
            var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));

            var screenWidth = (screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X);
            var screenHeight = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y);
            var width = destinationPercentage * screenWidth;
            var height = destinationPercentage * screenHeight;

            // Screen-centered version
            var distanceFromLeftBoundary = screenTopLeftInWpfCoords.X + ((1d - destinationPercentage) / 2d) * screenWidth;
            var distanceFromTopBoundary = screenTopLeftInWpfCoords.Y + ((1d - destinationPercentage) / 2d) * screenHeight;

            // Point-centered version 
            if (!centerOnScreen)
            {
                var widthCropped = Math.Min(pointInWpfCoords.X - screenTopLeftInWpfCoords.X, screenBottomRightInWpfCoords.X - pointInWpfCoords.X)
                    .CoerceToUpperLimit(width / 2d) + width / 2d;
                var heightCropped = Math.Min(pointInWpfCoords.Y - screenTopLeftInWpfCoords.Y, screenBottomRightInWpfCoords.Y - pointInWpfCoords.Y)
                    .CoerceToUpperLimit(height / 2d) + height / 2d;

                var minSize = destinationPercentage - .6 * Math.Pow(destinationPercentage, 2);
                destinationPercentage = (widthCropped / screenWidth).Clamp(minSize, destinationPercentage);
                destinationPercentage = (heightCropped / screenHeight).Clamp(minSize, destinationPercentage);

                width = destinationPercentage * screenWidth;
                height = destinationPercentage * screenHeight;

                distanceFromLeftBoundary = (pointInWpfCoords.X - (width / 2d) - screenTopLeftInWpfCoords.X).CoerceToLowerLimit(0);
                distanceFromTopBoundary = (pointInWpfCoords.Y - (height / 2d) - screenTopLeftInWpfCoords.Y).CoerceToLowerLimit(0);
            }

            MaxWidth = MinWidth = Width = width;
            MaxHeight = MinHeight = Height = height;
            HorizontalOffset = distanceFromLeftBoundary;
            VerticalOffset = distanceFromTopBoundary;

            Log.Info($"SetSizeAndPosition set Width={width}, Height={height}, HorizontalOffset={HorizontalOffset}, VerticalOffset={VerticalOffset}");
        }

        #endregion

        #region Capture & Display Scaled Screenshot

        private void DisplayScaledScreenshot()
        {
            var bitmap = CaptureScreenshot();
            Child = new Image { Source = bitmap.ToBitmapImage() };
        }

        private Bitmap CaptureScreenshot()
        {
            if (sourceArea.Width <= 0)
                throw new ArgumentOutOfRangeException(nameof(sourceArea.Width), $"SourceArea.Width was {sourceArea.Width}, which is invalid");

            if (sourceArea.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(sourceArea.Height), $"SourceArea.Height was {sourceArea.Height}, which is invalid");

            //Create the bitmap to copy the screen shot into
            var bitmap = new Bitmap(Convert.ToInt32(sourceArea.Width), Convert.ToInt32(sourceArea.Height));

            //Copy the screen image to the bitmap
            using (var graphic = Graphics.FromImage(bitmap))
            {
                graphic.CopyFromScreen(
                    new System.Drawing.Point(Convert.ToInt32(sourceArea.X), Convert.ToInt32(sourceArea.Y)),
                    System.Drawing.Point.Empty,
                    new System.Drawing.Size(Convert.ToInt32(sourceArea.Width), Convert.ToInt32(sourceArea.Height)));
            }

            Log.Info($"CaptureScreenshot sourceArea calculated as {sourceArea}");

            return bitmap;
        }

        #endregion

        #region Translate Magnified Selection Point

        private Point? TranslateMagnifiedSelectionPoint(Point point)
        {
            Point? translatedPoint = null;

            var image = VisualAndLogicalTreeHelper.FindLogicalChildren<Image>(this).First();
            
            //Convert screen to point on image co-ord system
            var imagePoint = image.PointFromScreen(new Point(point.X.Clamp(screen.Bounds.Left, screen.Bounds.Right), point.Y.Clamp(screen.Bounds.Top, screen.Bounds.Bottom)));
            var imageWidth = image.ActualWidth;
            var imageHeight = image.ActualHeight;

            if (imagePoint.X >= 0 && imagePoint.X < imageWidth
                && imagePoint.Y >= 0 && imagePoint.Y < imageHeight)
            {
                //Point is within the magnified image
                var sourceXRatio = imagePoint.X / imageWidth;
                var sourceYRatio = imagePoint.Y / imageHeight;

                var destX = (sourceXRatio * sourceArea.Width) + sourceArea.X;
                var destY = (sourceYRatio * sourceArea.Height) + sourceArea.Y;

                translatedPoint = new Point(destX, destY);
            }

            return translatedPoint;
        }

        #endregion
    }
}
