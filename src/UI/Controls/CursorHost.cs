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
        private bool selectionInProgress;
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
                .Where(cpp => cpp != null && !selectionInProgress) //Only set current Point if we are not within a selection/fixation
                .Subscribe(cpp => Point = cpp.Value);
            
            //Calculate position based on PointSelectionProgress
            mainViewModel.OnPropertyChanges(vm => vm.PointSelectionProgress)
                .Subscribe(psp => 
                {
                    if(psp == null)
                    {
                        //Selection/fixation not in progress
                        selectionInProgress = false;
                    }
                    else
                    {
                        //Selection/fixation in progress
                        selectionInProgress = true;
                        Point = psp.Item1;
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

        public static readonly DependencyProperty IsPivottedHorizontallyProperty =
            DependencyProperty.Register("IsPivottedHorizontally", typeof(bool), typeof(CursorHost), new PropertyMetadata(default(bool)));

        public bool IsPivottedHorizontally
        {
            get { return (bool)GetValue(IsPivottedHorizontallyProperty); }
            set { SetValue(IsPivottedHorizontallyProperty, value); }
        }

        public static readonly DependencyProperty IsPivottedVerticallyProperty =
            DependencyProperty.Register("IsPivottedVertically", typeof(bool), typeof(CursorHost), new PropertyMetadata(default(bool)));

        public bool IsPivottedVertically
        {
            get { return (bool)GetValue(IsPivottedVerticallyProperty); }
            set { SetValue(IsPivottedVerticallyProperty, value); }
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
                //N.B. Offsets are in the WPF coordinate system, so we now need to compare everything in that system
                //DIPs = pixels / (DPI/96.0) from https://msdn.microsoft.com/en-us/library/windows/desktop/dd371316(v=vs.85).aspx / https://msdn.microsoft.com/en-us/library/windows/desktop/ff684173(v=vs.85).aspx
                var dpiPoint = new Point(((double)pointCopy.X / ((double)Graphics.DpiX / (double)96)), ((double)pointCopy.Y / ((double)Graphics.DpiY / (double)96)));

                //Coerce horizontal offset
                var horizontalAdjustmentAmount = 0d;
                if(dpiPoint.X + ActualWidth > screenBottomRightInWpfCoords.X)
                {
                    horizontalAdjustmentAmount = 0 - ActualWidth;
                    IsPivottedHorizontally = true;
                }
                else
                {
                    IsPivottedHorizontally = false;
                }
                HorizontalOffset = dpiPoint.X + horizontalAdjustmentAmount;
                
                //Coerce vertical offset
                var verticalAdjustmentAmount = 0d;
                if(dpiPoint.Y + ActualHeight > screenBottomRightInWpfCoords.Y)
                {
                    verticalAdjustmentAmount = 0 - ActualHeight;
                    IsPivottedVertically = true;
                }
                else
                {
                    IsPivottedVertically = false;
                }
                VerticalOffset = dpiPoint.Y + verticalAdjustmentAmount;
            }
        }

        #endregion
    }
}
