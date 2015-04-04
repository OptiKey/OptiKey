using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.UI.ViewModels;
using log4net;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class CursorPopup : Popup
    {
        #region Private member vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window window;
        private Screen screen;
        private Point point = new Point(0,0);
        private Point screenTopLeft;
        private Point screenBottomRight;
        private Point screenBottomRightInWpfCoords;
        
        #endregion

        #region Ctor

        public CursorPopup()
        {
            Loaded += OnLoaded;
        }

        #endregion
        
        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Apply and subscribe to cursor height setting changes
            Action applyCursorHeight = () =>
            {
                MaxHeight = MinHeight = Height = Settings.Default.CursorHeight;
                CalculatePosition();
            };
            Settings.Default.OnPropertyChanges(s => s.CursorHeight).Subscribe(_ => applyCursorHeight());
            applyCursorHeight();

            //Apply and subscribe to cursor width setting changes
            Action applyCursorWidth = () =>
            {
                MaxWidth = MinWidth = Width = Settings.Default.CursorWidth;
                CalculatePosition();
            };
            Settings.Default.OnPropertyChanges(s => s.CursorWidth).Subscribe(_ => applyCursorWidth());
            applyCursorWidth();

            //Get references to window, screen and mainViewModel
            window = Window.GetWindow(this);
            Screen = window.GetScreen();
            var mainViewModel = DataContext as MainViewModel;

            //IsOpen
            Action<bool> calculateIsOpen = showCursor => IsOpen = showCursor;
            mainViewModel.OnPropertyChanges(vm => vm.ShowCursor).Subscribe(calculateIsOpen);
            calculateIsOpen(mainViewModel.ShowCursor);
            
            //Calculate position based on CurrentPositionPoint
            mainViewModel.OnPropertyChanges(vm => vm.CurrentPositionPoint)
                .Where(cpp => cpp != null && SelectionProgress == 0) //Only set current Point if we are not within a selection/fixation
                .Subscribe(cpp => Point = cpp.Value);
            
            //Calculate selection progress and position based on PointSelectionProgress
            mainViewModel.OnPropertyChanges(vm => vm.PointSelectionProgress)
                .Subscribe(psp =>
                {
                    if (psp == null)
                    {
                        //Selection/fixation not in progress
                        SelectionProgress = 0;
                    }
                    else
                    {
                        //Selection/fixation in progress
                        Point = psp.Item1;
                        SelectionProgress = psp.Item2;
                    }
                });

            SelectionProgress = mainViewModel.PointSelectionProgress != null
                ? mainViewModel.PointSelectionProgress.Item2
                : 0;
            
            //Subscribe to window location changes and re-evaluate the current screen and current position
            Observable.FromEventPattern<EventHandler, EventArgs>
                (h => window.LocationChanged += h,
                 h => window.LocationChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("Window's LocationChanged event detected.");
                    Screen = window.GetScreen();
                });
        }

        #endregion

        #region Properties

        private Screen Screen
        {
            get { return screen; }
            set
            {
                if (screen != value)
                {
                    screen = value;

                    screenTopLeft = new Point(screen.Bounds.Left, screen.Bounds.Top);
                    screenBottomRight = new Point(screen.Bounds.Right, screen.Bounds.Bottom);
                    screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(screenBottomRight);
                    
                    CalculatePosition();
                }
            }
        }
        
        //N.B. The sources 'CurrentPositionPoint' and 'PointSelectionProgress.Item1' are in the device co-ordinate system, so Point is too
        private Point Point
        {
            get { return point; }
            set
            {
                if(point != value)
                {
                    point = value;
                    CalculatePosition();
                }
            }
        }

        public static readonly DependencyProperty CursorPointPositionProperty =
            DependencyProperty.Register("CursorPointPosition", typeof(CursorPointPositions), typeof(CursorPopup), new PropertyMetadata(default(CursorPointPositions)));

        public CursorPointPositions CursorPointPosition
        {
            get { return (CursorPointPositions)GetValue(CursorPointPositionProperty); }
            set { SetValue(CursorPointPositionProperty, value); }
        }

        public static readonly DependencyProperty SelectionProgressProperty =
            DependencyProperty.Register("SelectionProgress", typeof(double), typeof(CursorPopup), new PropertyMetadata(default(double)));

        public double SelectionProgress
        {
            get { return (double)GetValue(SelectionProgressProperty); }
            set { SetValue(SelectionProgressProperty, value); }
        }

        #endregion

        #region Calculate Position

        private void CalculatePosition()
        {
            //Copy point locally as point is not threadsafe
            var pointCopy = Point;
            
            //Popup is open and point is within screen bounds
            if (IsOpen
                && pointCopy.X >= screenTopLeft.X
                && pointCopy.X <= screenBottomRight.X
                && pointCopy.Y >= screenTopLeft.Y
                && pointCopy.Y <= screenBottomRight.Y)
            {
                var dpiPoint = this.GetTransformFromDevice().Transform(pointCopy); //Offsets are in device independent pixels (DIP), but the incoming Point is in pixels

                bool cursorPointsToLeft;
                bool cursorPointsToTop;

                //Calculate horizontal offset
                if(dpiPoint.X + Width > screenBottomRightInWpfCoords.X) //Width is set explicitly on the Popup from the Setting value. Cannot use ActualWidth as it will be 0 (Popup itself is not part of the visual tree)
                {
                    //Move popup to the point, but manually adjust popup to the left of the point
                    HorizontalOffset = dpiPoint.X - Width;
                    cursorPointsToLeft = false;
                }
                else
                {
                    //Move popup to the point - it will align to the right of the point
                    HorizontalOffset = dpiPoint.X;
                    cursorPointsToLeft = true;
                }
                
                //Calculate vertical offset
                if (dpiPoint.Y + Height > screenBottomRightInWpfCoords.Y) //Width is set explicitly on the Popup from the Setting value. Cannot use ActualWidth as it will be 0 (Popup itself is not part of the visual tree)
                {
                    //Move popup to the point, but manually adjust popup to be above the point
                    VerticalOffset = dpiPoint.Y - Height;
                    cursorPointsToTop = false;
                }
                else
                {
                    //Move popup to the point - it will align to be below the point
                    VerticalOffset = dpiPoint.Y;
                    cursorPointsToTop = true;
                }

                CursorPointPosition = cursorPointsToTop && cursorPointsToLeft ? CursorPointPositions.ToTopLeft
                    : cursorPointsToTop && !cursorPointsToLeft ? CursorPointPositions.ToTopRight
                        : !cursorPointsToTop && cursorPointsToLeft ? CursorPointPositions.ToBottomLeft
                            : CursorPointPositions.ToBottomRight;
            }
        }

        #endregion
    }
}
