using System.Windows.Controls;
using System.Windows;
using Microsoft.Win32;
using System;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Properties;
using System.IO;
using JuliusSweetland.OptiKey.Services.PluginEngine;

namespace JuliusSweetland.OptiKey.UI.Views.Management
{
    /// <summary>
    /// Interaction logic for PluginsView.xaml
    /// </summary>
    public partial class PluginsView : System.Windows.Controls.UserControl
    {
        public PluginsView()
        {
            InitializeComponent();
        }

        private void FindPluginsFolder(object sender, System.Windows.RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog
            {
                Description = "Select folder containing plugins",
                SelectedPath = txtPluginsLocation.Text
            };

            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                // This is hooked up to the PluginsLocation property
                txtPluginsLocation.Text = folderBrowser.SelectedPath;
            }
        }

        private void RefreshAvailablePlugins(object sender, System.Windows.RoutedEventArgs e)
        {
            PluginEngine.RefreshAvailablePlugins();
        }
    }
}
