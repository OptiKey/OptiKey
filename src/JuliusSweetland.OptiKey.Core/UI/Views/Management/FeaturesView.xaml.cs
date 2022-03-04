// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Windows.Controls;
using System.Windows;
using Microsoft.Win32;
using System;
using System.Windows.Forms;
using JuliusSweetland.OptiKey.Properties;
using System.IO;
using JuliusSweetland.OptiKey.Services.PluginEngine;
using System.Windows.Data;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;

namespace JuliusSweetland.OptiKey.UI.Views.Management
{
    /// <summary>
    /// Interaction logic for FeaturesView.xaml
    /// </summary>
    public partial class FeaturesView : System.Windows.Controls.UserControl
    {
        public FeaturesView()
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

        private void btnFindCommuniKateTopPageLocation_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Open Board Format (*.OBF or *.OBZ)|*.obf; *.obz|Open Board Format file (*.OBF)|*.obf|Open Board Format archive (*.OBZ)|*.obz"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileLocation = null;

                if (openFileDialog.FileName.EndsWith(@".obf") || openFileDialog.FileName.EndsWith(@".obz"))
                {
                    txtCommuniKateTopPageLocation.Text = openFileDialog.FileName;
                    fileLocation = txtCommuniKateTopPageLocation.Text;
                }
                else
                {
                    txtCommuniKateTopPageLocation.Text = Properties.Resources.COMMUNIKATE_TOPPAGE_LOCATION_ERROR_LABEL;
                }

                // TODO: do this with binding only
                FeaturesViewModel viewModel = this.DataContext as FeaturesViewModel;
                if (viewModel != null && !string.IsNullOrWhiteSpace(fileLocation))
                {
                    viewModel.CommuniKatePagesetLocation = fileLocation;
                    viewModel.CommuniKateStagedForDeletion = true;
                }
            }
        }

        private void RefreshAvailablePlugins(object sender, System.Windows.RoutedEventArgs e)
        {            
            PluginEngine.RefreshAvailablePlugins(txtPluginsLocation.Text);

            // Force refresh by re-binding the CollectionView source
            // TODO: how should this be done with MVVM properly? we don't have property change notifications for the read-only list of plugins...
            ((CollectionViewSource)this.Resources["AvailablePluginsCollectionViewSource"]).Source = PluginEngine.GetAllAvailablePlugins(); 
        }
    }
}
