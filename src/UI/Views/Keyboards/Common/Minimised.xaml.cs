using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for Minimised.xaml
    /// </summary>
    public partial class Minimised : KeyboardView
    {
        public Minimised()
        {
            InitializeComponent();
            Loaded += (sender, args) => KeyboardSupportsCollapsedDock = false;
        }
    }
}
