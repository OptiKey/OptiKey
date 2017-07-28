using System.Windows.Controls;
using System.Windows;
using Microsoft.Win32;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.Views.Management
{
    /// <summary>
    /// Interaction logic for SoundsView.xaml
    /// </summary>
    public partial class SoundsView : UserControl
    {
        public SoundsView()
        {
            InitializeComponent();
        }

        private void btnFindFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.EndsWith(@"\bin\marytts-server.bat"))
                {
                    txtEditor.Text = openFileDialog.FileName;
                    Settings.Default.MaryTTSLocation = txtEditor.Text;
                }
                else
                {
                    txtEditor.Text = Properties.Resources.MARYTTS_LOCATION_ERROR_LABEL;
                    Settings.Default.MaryTTSLocation = null;
                }
            }
        }
    }
}
