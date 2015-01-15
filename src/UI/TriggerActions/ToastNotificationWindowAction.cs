using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.UI.Utilities;
using JuliusSweetland.ETTA.UI.Windows;
using log4net;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.ETTA.UI.TriggerActions
{
    public class ToastNotificationWindowAction : TriggerAction<FrameworkElement>
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Invoke

        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;

            if (args == null)
            {
                return;
            }

            var contentAsString = args.Context != null ? args.Context.Content as string : null;

            var displayTimeInSeconds = contentAsString != null
                ? (contentAsString.Length / Settings.Default.ToastNotificationCharactersPerLine) 
                    * Settings.Default.ToastNotificationSecondsPerLine
                : 0;

            displayTimeInSeconds += Settings.Default.ToastNotificationAdditionalSeconds;

            var childWindow = new ToastNotificationWindow
            {
                DataContext = args.Context,
                DisplayTime = new TimeSpan(0, 0, displayTimeInSeconds),
                ToastBorderBrush = this.ToastBorderBrush,
                ToastForegroundBrush = this.ToastForegroundBrush,
                ToastBackgroundBrush = this.ToastBackgroundBrush
            };

            var callback = args.Callback;
            EventHandler handler = null;
            handler =
                (o, e) =>
                {
                    childWindow.Closed -= handler;
                    callback();
                };
            childWindow.Closed += handler;

            Window parentWindow = null;
            if (AssociatedObject != null)
            {
                parentWindow = AssociatedObject as Window ?? VisualAndLogicalTreeHelper.FindVisualParent<Window>(AssociatedObject);
            }

            bool parentWindowHadFocus = false;
            bool parentWindowWasTopmost = false;
            if (parentWindow != null)
            {
                childWindow.Owner = parentWindow; //Setting the owner preserves the z-order of the parent and child windows when the focus is shifted back to the parent (otherwise the child popup will be hidden)
                parentWindowHadFocus = parentWindow.IsFocused;
                parentWindowWasTopmost = parentWindow.Topmost;
            }

            Log.Debug(string.Format("Showing ToastNotificationWindow with content '{0}' for {1} seconds", contentAsString, displayTimeInSeconds));

            childWindow.Show();

            if (parentWindow != null)
            {
                if(parentWindowHadFocus)
                {
                    Log.Debug("Parent Window was previously focussed - giving it focus again.");
                    parentWindow.Focus();
                }
                
                if(parentWindowWasTopmost)
                {
                    Log.Debug("Parent Window was previously top most - setting it back to top most window.");
                    parentWindow.Topmost = true;
                }
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ToastBorderBrushProperty =
            DependencyProperty.Register("ToastBorderBrush", typeof(Brush), typeof(ToastNotificationWindowAction), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush ToastBorderBrush
        {
            get { return (Brush)GetValue(ToastBorderBrushProperty); }
            set { SetValue(ToastBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty ToastForegroundBrushProperty =
            DependencyProperty.Register("ToastForegroundBrush", typeof(Brush), typeof(ToastNotificationWindowAction), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush ToastForegroundBrush
        {
            get { return (Brush)GetValue(ToastForegroundBrushProperty); }
            set { SetValue(ToastForegroundBrushProperty, value); }
        }

        public static readonly DependencyProperty ToastBackgroundBrushProperty =
            DependencyProperty.Register("ToastBackgroundBrush", typeof(Brush), typeof(ToastNotificationWindowAction), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush ToastBackgroundBrush
        {
            get { return (Brush)GetValue(ToastBackgroundBrushProperty); }
            set { SetValue(ToastBackgroundBrushProperty, value); }
        }

        #endregion
    }
}
