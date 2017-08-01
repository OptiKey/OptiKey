using System.Windows;
using System.Windows.Controls;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    /// <summary>
    /// Interaction logic for CKKey.xaml
    /// </summary>
    public partial class CKKey : UserControl
    {
        public CKKey()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CKMenuProperty =
            DependencyProperty.Register("CKMenu", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKMenu
        {
            get { return (string)GetValue(CKMenuProperty); }
            set { SetValue(CKMenuProperty, value); }
        }

        public static readonly DependencyProperty CKTextProperty =
            DependencyProperty.Register("CKText", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText
        {
            get { return (string)GetValue(CKTextProperty); }
            set { SetValue(CKTextProperty, value); }
        }

        public static readonly DependencyProperty CKMenuKeyProperty =
            DependencyProperty.Register("CKMenuKey", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey { get { return CKMenu !=null && CKMenu != ""; } }

        public static readonly DependencyProperty CKBaCoProperty =
            DependencyProperty.Register("CKBaCo", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo
        {
            get { return (string)GetValue(CKBaCoProperty); }
            set { SetValue(CKBaCoProperty, value); }
        }

        public static readonly DependencyProperty CKImSoProperty =
            DependencyProperty.Register("CKImSo", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo
        {
            get { return (string)GetValue(CKImSoProperty); }
            set { SetValue(CKImSoProperty, "/Resources/CommuniKateImages/image_" + value + ".png"); }
        }
    }
}
