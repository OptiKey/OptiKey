using System.Windows.Controls;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Properties;
using System.IO;
using System;

namespace JuliusSweetland.OptiKey.UI.Views.Management
{
    /// <summary>
    /// Interaction logic for VisualsView.xaml
    /// </summary>
    public partial class VisualsView : System.Windows.Controls.UserControl
    {

        public VisualsView()
        {
            InitializeComponent();
        }

        private void FindKeyboardsFolder(object sender, System.Windows.RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select folder containing dynamic keyboards";
            folderBrowser.SelectedPath = Settings.Default.DynamicKeyboardsLocation;

            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.DynamicKeyboardsLocation = folderBrowser.SelectedPath;
                txtKeyboardsLocation.Text = Settings.Default.DynamicKeyboardsLocation;
            }
        }
    }
}
