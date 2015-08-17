using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for NumericAndSymbols2.xaml
    /// </summary>
    public partial class NumericAndSymbols2 : KeyboardView
    {
        public NumericAndSymbols2()
        {
            InitializeComponent();
            Loaded += (sender, args) => KeyboardSupportsCollapsedDock = false;
        }
    }
}
