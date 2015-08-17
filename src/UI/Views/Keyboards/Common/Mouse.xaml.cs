using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for Mouse.xaml
    /// </summary>
    public partial class Mouse : KeyboardView
    {
        public Mouse()
        {
            InitializeComponent();
            Loaded += (sender, args) => KeyboardSupportsCollapsedDock = true;
        }
    }
}
