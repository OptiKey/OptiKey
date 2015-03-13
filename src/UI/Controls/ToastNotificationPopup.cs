using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.ViewModels;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class ToastNotificationPopup : Popup
    {
        private Storyboard storyboard;
        private DoubleAnimation closeAnimation;
        private Action onPopupClose;

        public ToastNotificationPopup()
        {
            Loaded += OnLoaded;
        }

        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            storyboard = this.Resources["ToastNotificationStoryboard"] as Storyboard;
            closeAnimation = storyboard.Children.First(c => c.Name == "CloseAnimation") as DoubleAnimation;
            
            var mainViewModel = DataContext as MainViewModel;

            //Handle ToastNotification event
            mainViewModel.ToastNotification += (o, args) =>
            {
                ClosePopup(); //Close the popup so it can be re-opened

                Title = args.Title;
                Content = args.Content;
                NotificationType = args.NotificationType;
                onPopupClose = args.OnPopupClose;

                var displayTimeInSeconds = Content != null
                ? Convert.ToInt32(Math.Ceiling((double)Content.Length / (double)Settings.Default.ToastNotificationCharactersPerLine))
                    * Settings.Default.ToastNotificationSecondsPerLine
                : 0;

                displayTimeInSeconds += Settings.Default.ToastNotificationAdditionalSeconds;
                
                closeAnimation.BeginTime = TimeSpan.FromSeconds(displayTimeInSeconds);

                EventHandler closeAnimationCompletedHander = null;
                closeAnimationCompletedHander = (sender2, e2) =>
                {
                    closeAnimation.Completed -= closeAnimationCompletedHander;
                    IsOpen = false;
                    if (onPopupClose != null)
                    {
                        onPopupClose();
                    }
                };

                closeAnimation.Completed += closeAnimationCompletedHander;

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

        #region Close Popup

        private void ClosePopup()
        {
            if (IsOpen)
            {
                IsOpen = false;
                if (onPopupClose != null)
                {
                    onPopupClose();
                }
            }
        }

        #endregion
    }
}
