using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.English
{
    /// <summary>
    /// Interaction logic for Alpha.xaml
    /// </summary>
    public partial class Alpha : KeyboardView
    {
        public Alpha()
        {
            InitializeComponent();
            Loaded += (sender, args) => KeyboardSupportsCollapsedDock = false;
        }
    }
}
