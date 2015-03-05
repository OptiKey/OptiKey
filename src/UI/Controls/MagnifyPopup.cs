using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.UI.ViewModels;
using log4net;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class MagnifyPopup : Popup
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
            Screen = window.GetScreen();
            var mainViewModel = DataContext as MainViewModel;

            //IsOpen
            Action<Point?> calculateIsOpen = magnifyAtPoint => IsOpen = magnifyAtPoint != null;
            mainViewModel.OnPropertyChanges(vm => vm.MagnifyAtPoint).Subscribe(calculateIsOpen);
            calculateIsOpen(mainViewModel.MagnifyAtPoint);
            
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
                }
            }
        }

        #endregion

        
    }
}
