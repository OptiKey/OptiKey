using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.English
{
    /// <summary>
    /// Interaction logic for CommuniKate.xaml
    /// </summary>
    public partial class CommuniKate : KeyboardView
    {
        public CommuniKate() : base(shiftAware: false)
        {
            InitializeComponent();
        }

        private void Key_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {

        }
    }
}
