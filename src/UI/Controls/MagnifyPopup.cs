using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.ViewModels;
using log4net;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class MagnifyPopup : Popup
    {
        #region Private member vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window window;
        private Screen screen;
        private Canvas canvas;
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
            canvas = VisualAndLogicalTreeHelper.FindLogicalChildren<Canvas>(this).First();
            var mainViewModel = DataContext as MainViewModel;

            //Listen for MagnifyPoint changes
            mainViewModel.OnPropertyChanges(vm => vm.MagnifyAtPoint).Subscribe(sourcePoint =>
            {
                if (sourcePoint != null)
                {
                    SetSizeAndPosition();

                    DisplayScaledScreenshot(sourcePoint.Value);

                    EventHandler<Point> pointSelectionHandler = null;
                    pointSelectionHandler = (pointSelectionSender, point) =>
                    {
                        mainViewModel.PointSelection -= pointSelectionHandler; //Only react to one PointSelection event

                        Point? destinationPoint = sourcePoint; //TODO - calculate destination point from sourceArea and point
                        
                        if (mainViewModel.MagnifiedPointSelectionAction != null)
                        {
                            mainViewModel.MagnifiedPointSelectionAction(destinationPoint);
                        }

                        IsOpen = false;
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

        private void SetSizeAndPosition()
        {
            var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));

            var screenWidth = (screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X);
            var screenHeight = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y);

            var horizontalFillPercentage = Settings.Default.MagnifyWindowFillHorizontalPercentageOfScreen;
            var verticalFillPercentage = Settings.Default.MagnifyWindowFillVerticalPercentageOfScreen;

            var distanceFromLeftBoundary = ((1d - (horizontalFillPercentage / 100)) / 2d) * screenWidth;
            var distanceFromTopBoundary = ((1d - (verticalFillPercentage / 100)) / 2d) * screenHeight;

            HorizontalOffset = screenTopLeftInWpfCoords.X + distanceFromLeftBoundary;
            VerticalOffset = screenTopLeftInWpfCoords.Y + distanceFromTopBoundary;

            var width = (horizontalFillPercentage / 100) * screenWidth;
            var height = (verticalFillPercentage / 100) * screenHeight;

            MaxWidth = MinWidth = Width = width;
            MaxHeight = MinHeight = Height = height;
        }

        #endregion

        #region Take Screenshot At Point

        private void DisplayScaledScreenshot(Point point)
        {
            var bitmap = CaptureScreenshot(point);
            Child = new Image {Source = bitmap.ToBitmapImage()};
        }

        private Bitmap CaptureScreenshot(Point point)
        {
            var captureWidth = (Settings.Default.MagnifySourceAreaHorizontalPercentageOfScreen / 100d) * screen.Bounds.Width;
            captureWidth.CoerceToUpperLimit(screen.Bounds.Width);

            var captureHeight = (Settings.Default.MagnifySourceAreaVerticalPercentageOfScreen / 100d) * screen.Bounds.Height;
            captureHeight.CoerceToUpperLimit(screen.Bounds.Height);

            var captureX = point.X - (captureWidth/2d);
            captureX.CoerceToLowerLimit(screen.Bounds.Left);
            if (captureX + captureWidth > screen.Bounds.Right)
            {
                captureX = screen.Bounds.Right - captureWidth;
            }

            var captureY = point.Y - (captureHeight / 2d);
            captureY.CoerceToLowerLimit(screen.Bounds.Top);
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
    }
}
