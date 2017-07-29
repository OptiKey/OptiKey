using System.Windows;
using System.Windows.Controls;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    /// <summary>
    /// Interaction logic for Output.xaml
    /// </summary>
    public partial class CKKey : UserControl
    {
        public CKKey()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Key), new PropertyMetadata(default(bool)));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Key), new PropertyMetadata(default(bool)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ShiftUpTextProperty =
            DependencyProperty.Register("ShiftUpText", typeof(string), typeof(Key), new PropertyMetadata(default(bool)));

        public string ShiftUpText
        {
            get { return (string)GetValue(ShiftUpTextProperty); }
            set { SetValue(ShiftUpTextProperty, value); }
        }

        public static readonly DependencyProperty ShiftDownTextProperty =
            DependencyProperty.Register("ShiftDownText", typeof(string), typeof(Key), new PropertyMetadata(default(bool)));

        public string ShiftDownText
        {
            get { return (string)GetValue(ShiftDownTextProperty); }
            set { SetValue(ShiftDownTextProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColourProperty =
            DependencyProperty.Register("BackgroundColour", typeof(string), typeof(Key), new PropertyMetadata(default(bool)));

        public string BackgroundColour
        {
            get { return (string)GetValue(BackgroundColourProperty); }
            set { SetValue(BackgroundColourProperty, value); }
        }

        public static readonly DependencyProperty BImageSourceProperty =
            DependencyProperty.Register("BackgroundColour", typeof(string), typeof(Key), new PropertyMetadata(default(bool)));

        public string ImageSource
        {
            get { return (string)GetValue(BImageSourceProperty); }
            set { SetValue(BImageSourceProperty, "/Resources/CommuniKateImages/image_" + value + ".png"); }
        }
    }
}
