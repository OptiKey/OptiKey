using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for Currencies1.xaml
    /// </summary>
    public partial class Currencies1 : KeyboardView
    {
        public Currencies1()
        {
            InitializeComponent();
            Loaded += (sender, args) => KeyboardSupportsCollapsedDock = false;
        }
    }
}
