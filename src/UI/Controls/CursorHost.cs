using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class CursorHost : Popup
    {
        #region Private member vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window window;
        private Screen screen;
        private Point screenTopLeft;
        private Point screenBottomRight;

        #endregion

        #region Ctor

        public CursorHost()
        {
            Loaded += OnLoaded;
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

                    if (value != null)
                    {
                        screenTopLeft = new Point(screen.Bounds.Left, screen.Bounds.Top);
                        screenBottomRight = new Point(screen.Bounds.Right, screen.Bounds.Bottom);
                        CalculatePosition();
                    }
                }
            }
        }

        //SelectionMode DP bound to MVM.SelectionMode
        //    -> IsOpen = SelectionMode == SelectionModes.Point;

        //React to IsOpen changes - CalculatePoint();

        //CurrentPoint DP bound to MVM.CurrentPositionPoint
        //    -> if(PointSelectionProgress == null)
        //        {
        //            CalculatePosition();
        //        }

        //SelectionProgress DP bound to MVM.PointSelectionProgress
        //    -> CalculatePosition();

        //IsPivottedHorizontally
        //IsPivottedVertically

        #endregion

        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            window = Window.GetWindow(this);
            Screen = window.GetScreen();
            CalculatePosition();

            //Subscribe to window location changes and re-evaluate the current screen
            Observable.FromEventPattern<EventHandler, EventArgs>
                (h => window.LocationChanged += h,
                 h => window.LocationChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("Window's LocationChanged event detected.");
                    Screen = window.GetScreen();
                    CalculatePosition();
                });

            //Re-calculate cursor host position on size changes
            this.OnPropertyChanges<double>(ActualWidthProperty).Subscribe(_ => CalculatePosition());
            this.OnPropertyChanges<double>(ActualHeightProperty).Subscribe(_ => CalculatePosition());
        }

        #endregion

        #region Calculate Position

        //N.B. CurrentPoint and SelectionProgress.Point are in the device co-ordinate system
        //N.B. Offsets appear to be in DPI, i.e. WPF coordinate system
        private void CalculatePosition()
        {
            //if (IsOpen
            //    && Screen != null)
            //{
            //    Point point = SelectionProgress != null
            //        ? SelectionProgress.Point
            //        : CurrentPoint;

            //    if (point != null)
            //    {
            //        //Check if point is within screen bounds
            //        if (point.X >= screenTopLeft.X
            //            && point.X <= screenBottomRight.X
            //            && point.Y >= screenTopLeft.Y
            //            && point.Y <= screenBottomRight.Y)
            //        {
            //            //Convert point to DPI points - https://msdn.microsoft.com/en-us/library/windows/desktop/ff684173(v=vs.85).aspx
            //            //DIPs = pixels / (DPI/96.0) from https://msdn.microsoft.com/en-us/library/windows/desktop/dd371316(v=vs.85).aspx
            //            var dpiPoint = new Point((point.X / (Graphics.DpiX / 96)), (point.Y / (Graphics.DpiY / 96)));

                        //Adjust offsets for screen boundaries vs ActualWidth & ActualHeight
                        //Set HorizontalPosition and VerticalPosition

            //            HorizontalOffset = dpiPoint.X;
            //            VerticalOffset = dpiPoint.Y;
            //        }
            //    }
            //}
        }

        #endregion
    }
}
