using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.ViewModels;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class ToastNotificationPopup : Popup
    {
        #region Private Member Variables

        private Window window;
        private Screen screen;
        private ToastNotification toastNotification;

        #endregion

        public ToastNotificationPopup()
        {
            Loaded += OnLoaded;
        }

        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Get references to window, screen, toastNotification and mainViewModel
            window = Window.GetWindow(this);
            screen = window.GetScreen();
            toastNotification = VisualAndLogicalTreeHelper.FindLogicalChildren<ToastNotification>(this).First();
            var mainViewModel = DataContext as MainViewModel;

            //Handle ToastNotification event
            mainViewModel.SubscribeToToastNotification((o, args) =>
            {
                SetSizeAndPosition();

                Title = args.Title;
                Content = args.Content;
                NotificationType = args.NotificationType;

                Action closePopup = () =>
                {
                    if (IsOpen)
                    {
                        IsOpen = false;
                        if (args.Callback != null)
                        {
                            args.Callback();
                        }
                    }
                };

                AnimateTarget(args.Content, toastNotification, closePopup);
                IsOpen = true;
            });
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ToastNotificationPopup), new PropertyMetadata(default(string)));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(ToastNotificationPopup), new PropertyMetadata(default(string)));

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty NotificationTypeProperty =
            DependencyProperty.Register("NotificationType", typeof(NotificationTypes), typeof(ToastNotificationPopup), new PropertyMetadata(default(NotificationTypes)));

        public NotificationTypes NotificationType
        {
            get { return (NotificationTypes)GetValue(NotificationTypeProperty); }
            set { SetValue(NotificationTypeProperty, value); }
        }

        #endregion

        #region Set Size And Position

        private void SetSizeAndPosition()
        {
            var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
            var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));

            var screenWidth = (screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X);
            var screenHeight = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y);

            var toastPopupWidthAsPercentage = Settings.Default.ToastNotificationHorizontalFillPercentage / 100d;
            var toastPopupHeightAsPercentage = Settings.Default.ToastNotificationVerticalFillPercentage / 100d;

            var distanceFromLeftBoundary = ((1d - toastPopupWidthAsPercentage) / 2d) * screenWidth;
            var distanceFromTopBoundary = ((1d - toastPopupHeightAsPercentage) / 2d) * screenHeight;

            HorizontalOffset = screenTopLeftInWpfCoords.X + distanceFromLeftBoundary;
            VerticalOffset = screenTopLeftInWpfCoords.Y + distanceFromTopBoundary;

            var width = toastPopupWidthAsPercentage * screenWidth;
            var height = toastPopupHeightAsPercentage * screenHeight;

            MaxWidth = MinWidth = Width = width;
            MaxHeight = MinHeight = Height = height;
        }

        #endregion

        #region Animate Target

        private static void AnimateTarget(string text, FrameworkElement target, Action onPopupClose)
        {
            var storyboard = new Storyboard();

            var introAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.Stop);
            Storyboard.SetTarget(introAnimation, target);
            Storyboard.SetTargetProperty(introAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(introAnimation);

            var displayTimeInSeconds = Convert.ToInt32(Math.Ceiling((double)(text != null ? text.Length : 0) 
                * (double)Settings.Default.ToastNotificationSecondsPerCharacter));

            var outroAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.Stop)
            {
                BeginTime = TimeSpan.FromSeconds(displayTimeInSeconds)
            };
            outroAnimation.Completed += (_, __) => onPopupClose();
            Storyboard.SetTarget(outroAnimation, target);
            Storyboard.SetTargetProperty(outroAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(outroAnimation);

            storyboard.Begin(target);
        }

        #endregion
    }
}
