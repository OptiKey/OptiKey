using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for NumericAndSymbols1.xaml
    /// </summary>
    public partial class NumericAndSymbols1 : KeyboardView
    {
        public NumericAndSymbols1()
        {
            InitializeComponent();
            Loaded += (sender, args) => KeyboardSupportsCollapsedDock = false;
        }
    }
}
