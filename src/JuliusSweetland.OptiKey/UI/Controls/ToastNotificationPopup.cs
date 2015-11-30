using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
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
        public ToastNotificationPopup()
        {
            Loaded += OnLoaded;
        }

        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = VisualAndLogicalTreeHelper.FindLogicalParent<Window>(this);
            var toastNotification = VisualAndLogicalTreeHelper.FindLogicalChildren<ToastNotification>(this).First();
            var mainViewModel = DataContext as MainViewModel;

            //Handle ToastNotification event
            mainViewModel.ToastNotification += (o, args) =>
            {
                //Set size and position
                SetSize(toastNotification, window);
                SetPosition(window);

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
            };
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

        #region Set Size

        private static void SetSize(FrameworkElement target, Window parent)
        {
            var screen = parent.GetScreen();

            target.MaxHeight = target.MinHeight = target.Height =
                screen.Bounds.Height * Settings.Default.ToastNotificationVerticalFillPercentage / 100.0;

            target.MaxWidth = target.MinWidth = target.Width =
                screen.Bounds.Width * Settings.Default.ToastNotificationHorizontalFillPercentage / 100.0;
        }

        #endregion

        #region Set Position

        private void SetPosition(FrameworkElement parent)
        {
            //VerticalOffset is used to align the center of this popup with a point on the parent window so calculate the window's center line
            VerticalOffset = (parent.ActualHeight / 2d) * ((double)Settings.Default.ToastNotificationVerticalFillPercentage / 100d);
        }

        #endregion

        #region Animate Target

        private void AnimateTarget(string text, FrameworkElement target, Action onPopupClose)
        {
            var storyboard = new Storyboard();

            var introAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.Stop);
            Storyboard.SetTarget(introAnimation, target);
            Storyboard.SetTargetProperty(introAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
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
