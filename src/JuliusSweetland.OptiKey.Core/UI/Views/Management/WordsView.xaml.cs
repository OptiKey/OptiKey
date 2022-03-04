// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

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

        private void btnFindPresageDatabaseLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Presage Database (*.db)|*.db"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileLocation = null;

                if (openFileDialog.FileName.EndsWith(@".db"))
                {
                    txtPresageDatabaseLocation.Text = openFileDialog.FileName;
                    fileLocation = txtPresageDatabaseLocation.Text;
                }
                else
                {
                    txtPresageDatabaseLocation.Text = Properties.Resources.COMMUNIKATE_TOPPAGE_LOCATION_ERROR_LABEL;
                }

                WordsViewModel viewModel = this.DataContext as WordsViewModel;
                if (viewModel != null && !string.IsNullOrWhiteSpace(fileLocation))
                {
                    viewModel.PresageDatabaseLocation = fileLocation;
                }
            }
        }
    }
}
