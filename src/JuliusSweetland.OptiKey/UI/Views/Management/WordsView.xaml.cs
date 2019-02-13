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

        private void btnFindCommuniKateTopPageLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
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

                WordsViewModel viewModel = this.DataContext as WordsViewModel;
                if (viewModel != null && !string.IsNullOrWhiteSpace(fileLocation))
                {
                    viewModel.CommuniKatePagesetLocation = fileLocation;
                    viewModel.CommuniKateStagedForDeletion = true;
                }
            }
        }

        private void btnFindPresageDatabaseLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Presaage Database (*.db)|*.db"
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
