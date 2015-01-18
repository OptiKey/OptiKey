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
        private readonly WindowStatePersistenceService windowStatePersistenceService;

        public ManagementWindow(
            IAudioService audioService,
            IDictionaryService dictionaryService)
        {
            InitializeComponent();

            //Instantiate ManagementViewModel and set as DataContext of ManagementView
            var managementViewModel = new ManagementViewModel(audioService, dictionaryService);
            this.ManagementView.DataContext = managementViewModel;

            //Instantiate window state persistence service and provide accessors to the appropriate settings for this window
            windowStatePersistenceService = new WindowStatePersistenceService(
                () => Settings.Default.ManagementConsoleWindowTop, d => Settings.Default.ManagementConsoleWindowTop = d,
                () => Settings.Default.ManagementConsoleWindowLeft, d => Settings.Default.ManagementConsoleWindowLeft = d,
                () => Settings.Default.ManagementConsoleWindowHeight, d => Settings.Default.ManagementConsoleWindowHeight = d,
                () => Settings.Default.ManagementConsoleWindowWidth, d => Settings.Default.ManagementConsoleWindowWidth = d,
                () => Settings.Default.ManagementConsoleWindowState, s => Settings.Default.ManagementConsoleWindowState = s,
                Settings.Default);

            //Apply window settings from window state persistence service
            Height = windowStatePersistenceService.WindowHeight;
            Width = windowStatePersistenceService.WindowWidth;
            Top = windowStatePersistenceService.WindowTop;
            Left = windowStatePersistenceService.WindowLeft;
            WindowState = windowStatePersistenceService.WindowState;

            //Store current window settings on closing
            Closing += (sender, args) =>
            {
                if (WindowState != WindowState.Minimized)
                {
                    windowStatePersistenceService.WindowHeight = Height;
                    windowStatePersistenceService.WindowWidth = Width;
                    windowStatePersistenceService.WindowTop = Top;
                    windowStatePersistenceService.WindowLeft = Left;
                    windowStatePersistenceService.WindowState = WindowState;

                    windowStatePersistenceService.Save();
                }
            };
        }
    }
}
