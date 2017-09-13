using System.Windows.Controls;
using System.Windows;
using Microsoft.Win32;
using System;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Properties;
using System.IO;

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
            folderBrowser.SelectedPath = txtKeyboardsLocation.Text;

            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                // This is hooked up to the DynamicKeyboardsLocation property
                txtKeyboardsLocation.Text = folderBrowser.SelectedPath; 
            }
        }

        private void FindStartupKeyboardFile(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XML keyboard definition (*.xml)|*.xml"
            };

            if (File.Exists(txtStartupKeyboardLocation.Text))
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(txtStartupKeyboardLocation.Text);
            }

            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.EndsWith(@".xml"))
                {
                    // This is hooked up to the DynamicKeyboardsLocation property
                    txtStartupKeyboardLocation.Text = openFileDialog.FileName;
                }                
            }
        }
    }
}
