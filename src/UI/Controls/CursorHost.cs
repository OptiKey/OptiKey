using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Static;
using JuliusSweetland.OptiKey.UI.ViewModels;
using log4net;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class CursorHost : Popup
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

        public CursorHost()
        {
            Loaded += OnLoaded;
        }

        #endregion
        
        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            window = Window.GetWindow(this);
            Screen = window.GetScreen();
            
            var mainViewModel = DataContext as MainViewModel;
            
            //IsOpen
            Action<SelectionModes> calculateIsOpen = selectionMode => IsOpen = selectionMode == SelectionModes.Point;
            mainViewModel.OnPropertyChanges(vm => vm.SelectionMode).Subscribe(calculateIsOpen);
            calculateIsOpen(mainViewModel.SelectionMode);
            
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

            //Re-calculate cursor host position on size changes
            this.OnPropertyChanges<double>(ActualWidthProperty).Subscribe(_ => CalculatePosition());
            this.OnPropertyChanges<double>(ActualHeightProperty).Subscribe(_ => CalculatePosition());
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
            DependencyProperty.Register("CursorPointPosition", typeof(CursorPointPositions), typeof(CursorHost), new PropertyMetadata(default(CursorPointPositions)));

        public CursorPointPositions CursorPointPosition
        {
            get { return (CursorPointPositions)GetValue(CursorPointPositionProperty); }
            set { SetValue(CursorPointPositionProperty, value); }
        }

        public static readonly DependencyProperty SelectionProgressProperty =
            DependencyProperty.Register("SelectionProgress", typeof(double), typeof(CursorHost), new PropertyMetadata(default(double)));

        public double SelectionProgress
        {
            get { return (double)GetValue(SelectionProgressProperty); }
            set { SetValue(SelectionProgressProperty, value); }
        }

        #endregion

        #region Calculate Position

        //N.B. Despite the MSDN documentation popup with Placement=AbsolutePosition aligns to the BOTTOM LEFT of the point, i.e. the offset point is at the TOP RIGHT
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
                //N.B. Offsets are in the WPF coordinate system, so we now need to compare everything in that system
                //DIPs = pixels / (DPI/96.0) from https://msdn.microsoft.com/en-us/library/windows/desktop/dd371316(v=vs.85).aspx / https://msdn.microsoft.com/en-us/library/windows/desktop/ff684173(v=vs.85).aspx
                var dpiPoint = new Point(((double)pointCopy.X / ((double)Graphics.DpiX / (double)96)), ((double)pointCopy.Y / ((double)Graphics.DpiY / (double)96)));

                bool cursorPointsToLeft;
                bool cursorPointsToTop;

                //Coerce horizontal offset
                if(dpiPoint.X + Width > screenBottomRightInWpfCoords.X) //Width is set explicitly on the Popup from the Setting value. Cannot use ActualWidth as it will be 0 (Popup itself is not part of the visual tree)
                {
                    //Do not adjust horizontal offset - default position of popup is to the left of the point
                    HorizontalOffset = dpiPoint.X;
                    cursorPointsToLeft = false;
                }
                else
                {
                    //Manually adjust popup to the right of the point (default position of popup is to the left of the point)
                    HorizontalOffset = dpiPoint.X + Width;
                    cursorPointsToLeft = true;
                }
                
                //Coerce vertical offset
                if (dpiPoint.Y + Height > screenBottomRightInWpfCoords.Y) //Width is set explicitly on the Popup from the Setting value. Cannot use ActualWidth as it will be 0 (Popup itself is not part of the visual tree)
                {
                    //Manually adjust popup to be above the point (default position of popup is below the point)
                    VerticalOffset = dpiPoint.Y - Height;
                    cursorPointsToTop = false;
                }
                else
                {
                    //Do not adjust vertical offset - default position of popup is below the point
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
