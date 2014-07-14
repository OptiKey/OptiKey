using System;
using System.Windows;
using System.Windows.Media;

namespace JuliusSweetland.ETTA.UI.Windows
{
    /// <summary>
    /// Interaction logic for ToastNotificationWindow.xaml
    /// </summary>
    public partial class ToastNotificationWindow : Window
    {
        #region Ctor

        public ToastNotificationWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public TimeSpan DisplayTime { set { OpacityAnimation.BeginTime = value; } }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ToastBorderBrushProperty =
            DependencyProperty.Register("ToastBorderBrush", typeof (Brush), typeof (ToastNotificationWindow), new PropertyMetadata(default(Brush)));

        public Brush ToastBorderBrush
        {
            get { return (Brush) GetValue(ToastBorderBrushProperty); }
            set { SetValue(ToastBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty ToastForegroundBrushProperty =
            DependencyProperty.Register("ToastForegroundBrush", typeof (Brush), typeof (ToastNotificationWindow), new PropertyMetadata(default(Brush)));

        public Brush ToastForegroundBrush
        {
            get { return (Brush) GetValue(ToastForegroundBrushProperty); }
            set { SetValue(ToastForegroundBrushProperty, value); }
        }
        
        public static readonly DependencyProperty ToastBackgroundBrushProperty =
            DependencyProperty.Register("ToastBackgroundBrush", typeof(Brush), typeof(ToastNotificationWindow), new PropertyMetadata(default(Brush)));

        
        public Brush ToastBackgroundBrush
        {
            get { return (Brush)GetValue(ToastBackgroundBrushProperty); }
            set { SetValue(ToastBackgroundBrushProperty, value); }
        }

        #endregion

        #region Fadeout Animation - On Completed Event

        private void FadeoutAnimation_OnCompleted(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
