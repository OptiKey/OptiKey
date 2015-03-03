using System.Windows;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ViewModels;
using MahApps.Metro.Controls;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for ManagementWindow.xaml
    /// </summary>
    public partial class ManagementWindow : MetroWindow
    {
        private readonly WindowStateService windowStateService;

        public ManagementWindow(
            IAudioService audioService,
            IDictionaryService dictionaryService)
        {
            InitializeComponent();

            //Instantiate ManagementViewModel and set as DataContext of ManagementView
            var managementViewModel = new ManagementViewModel(audioService, dictionaryService);
            this.ManagementView.DataContext = managementViewModel;
        }
    }
}
