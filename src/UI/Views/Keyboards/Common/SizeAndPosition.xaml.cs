using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for SizeAndPosition.xaml
    /// </summary>
    public partial class SizeAndPosition : KeyboardView
    {
        public SizeAndPosition()
        {
            InitializeComponent();
            Loaded += (sender, args) => KeyboardSupportsCollapsedDock = false;
        }
    }
}
