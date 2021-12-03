// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

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

        private void btnFindMaryTts_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                FileName = "marytts-server.bat",
                Filter = "marytts-server|marytts-server.bat"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileLocation = null;

                if (openFileDialog.FileName.EndsWith(@"\bin\marytts-server.bat"))
                {
                    txtMaryTtsLocation.Text = openFileDialog.FileName;
                    fileLocation = txtMaryTtsLocation.Text;
                }
                else
                {
                    txtMaryTtsLocation.Text = Properties.Resources.MARYTTS_LOCATION_ERROR_LABEL;
                }

                SoundsViewModel viewModel = this.DataContext as SoundsViewModel;
                if (viewModel != null && !string.IsNullOrWhiteSpace(fileLocation))
                {
                    viewModel.MaryTTSLocation = fileLocation;
                }
            }
        }
    }
}
