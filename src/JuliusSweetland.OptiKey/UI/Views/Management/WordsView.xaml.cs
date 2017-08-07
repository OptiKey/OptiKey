using System.Windows.Controls;
using System.Windows;
using Microsoft.Win32;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.Views.Management
{
    /// <summary>
    /// Interaction logic for WordsView.xaml
    /// </summary>
    public partial class WordsView : UserControl
    {
        public WordsView()
        {
            InitializeComponent();
        }

        private void btnFindCommuniKateTopPageLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Open Board Format (*.obf)|*.obf"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.EndsWith(@".obf"))
                {
                    txtCommuniKateTopPageLocation.Text = openFileDialog.FileName;
                    Settings.Default.CommuniKateTopPageLocation = txtCommuniKateTopPageLocation.Text;
                }
                else
                {
                    txtCommuniKateTopPageLocation.Text = Properties.Resources.COMMUNIKATE_TOPPAGE_LOCATION_ERROR_LABEL;
                    Settings.Default.CommuniKateTopPageLocation = null;
                }
            }
        }
    }
}
