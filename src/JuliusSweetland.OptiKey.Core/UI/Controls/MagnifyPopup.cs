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

        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Get references to window, screen and mainViewModel
            window = Window.GetWindow(this);
            screen = window.GetScreen();
            var mainViewModel = DataContext as MainViewModel;

            //Listen for MagnifyPoint changes
            mainViewModel.OnPropertyChanges(vm => vm.MagnifyAtPoint).Subscribe(sourcePoint =>
            {
                if (sourcePoint != null)
                {
                    SetSizeAndPosition(sourcePoint.Value);

                    try
                    {
                        DisplayScaledScreenshot(sourcePoint.Value);
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        mainViewModel.RaiseToastNotification(OptiKey.Properties.Resources.ERROR_TITLE,
                            OptiKey.Properties.Resources.ERROR_MAGNIFYING,
                            NotificationTypes.Error, () => {});

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

            //Subscribe to window location changes and re-evaluate the current screen and current position
            Observable.FromEventPattern<EventHandler, EventArgs>
                (h => window.LocationChanged += h,
                 h => window.LocationChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("Window's LocationChanged event detected.");
                    screen = window.GetScreen();
                });
        }

        #endregion

        #region Set Size And Position

        private void SetSizeAndPosition(Point point)
        {
            var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));

            var screenWidth = (screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X);
            var screenHeight = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y);

            var destinationPercentage = Settings.Default.MagnifyDestinationPercentageOfScreen / 100d;
            var centerOnScreen = Settings.Default.MagnifierCenterOnScreen;

            var width = destinationPercentage * screenWidth;
            var height = destinationPercentage * screenHeight;

            MaxWidth = MinWidth = Width = width;
            MaxHeight = MinHeight = Height = height;

            // Screen-centered version (TODO: use Popup Placement="Centre" instead?)
            var distanceFromLeftBoundary = screenTopLeftInWpfCoords.X + ((1d - destinationPercentage) / 2d) * screenWidth;
            var distanceFromTopBoundary = screenTopLeftInWpfCoords.Y + ((1d - destinationPercentage) / 2d) * screenHeight;
            var pointInWpfCoords = window.GetTransformFromDevice().Transform(new Point(point.X, point.Y));

            // Point-centered version 
            if (!centerOnScreen)
            {
                distanceFromLeftBoundary = (pointInWpfCoords.X - (width / 2d)).CoerceToLowerLimit(0);
                distanceFromLeftBoundary.CoerceToUpperLimit(screenWidth - width);

                distanceFromTopBoundary = (pointInWpfCoords.Y - (height / 2d)).CoerceToLowerLimit(0);
                distanceFromTopBoundary.CoerceToUpperLimit(screenHeight - height);
            }

            HorizontalOffset = distanceFromLeftBoundary;
            VerticalOffset = distanceFromTopBoundary;
        }

        #endregion

        #region Capture & Display Scaled Screenshot

        private void DisplayScaledScreenshot(Point point)
        {
            var bitmap = CaptureScreenshot(point);
            Child = new Image { Source = bitmap.ToBitmapImage() };
        }

        private Bitmap CaptureScreenshot(Point point)
        {
            var magnifySourcePercentage = Settings.Default.MagnifySourcePercentageOfScreen / 100d;

            var captureWidth = magnifySourcePercentage * screen.Bounds.Width;
            captureWidth = captureWidth.CoerceToUpperLimit(screen.Bounds.Width);

            var captureHeight = magnifySourcePercentage * screen.Bounds.Height;
            captureHeight = captureHeight.CoerceToUpperLimit(screen.Bounds.Height);

            var captureX = point.X - (captureWidth / 2d);
            captureX = captureX.CoerceToLowerLimit(screen.Bounds.Left);
            if (captureX + captureWidth > screen.Bounds.Right)
            {
                captureX = screen.Bounds.Right - captureWidth;
            }

            var captureY = point.Y - (captureHeight / 2d);
            captureY = captureY.CoerceToLowerLimit(screen.Bounds.Top);
            if (captureY + captureHeight > screen.Bounds.Bottom)
            {
                captureY = screen.Bounds.Bottom - captureHeight;
            }

            //Calculate source area
            sourceArea = new Rect(captureX, captureY, captureWidth, captureHeight);

            //Create the bitmap to copy the screen shot into
            var bitmap = new Bitmap(Convert.ToInt32(captureWidth), Convert.ToInt32(captureHeight));

            //Copy the screen image to the bitmap
            using (var graphic = Graphics.FromImage(bitmap))
            {
                graphic.CopyFromScreen(
                    new System.Drawing.Point(Convert.ToInt32(sourceArea.X), Convert.ToInt32(sourceArea.Y)),
                    System.Drawing.Point.Empty,
                    new System.Drawing.Size(Convert.ToInt32(sourceArea.Width), Convert.ToInt32(sourceArea.Height)));
            }

            return bitmap;
        }

        #endregion

        #region Translate Magnified Selection Point

        private Point? TranslateMagnifiedSelectionPoint(Point point)
        {
            Point? translatedPoint = null;

            var image = VisualAndLogicalTreeHelper.FindLogicalChildren<Image>(this).First();

            var imagePoint = image.PointFromScreen(point); //Convert screen to point on image co-ord system
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
