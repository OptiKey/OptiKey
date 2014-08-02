using System.Windows;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;

namespace JuliusSweetland.ETTA.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WindowStatePersistenceService windowStatePersistenceService;

        public MainWindow()
        {
            InitializeComponent();

            //Instantiate window state persistence service and provide accessors to the appropriate settings for this window
            windowStatePersistenceService = new WindowStatePersistenceService(
                () => Settings.Default.MainWindowTop, d => Settings.Default.MainWindowTop = d,
                () => Settings.Default.MainWindowLeft, d => Settings.Default.MainWindowLeft = d,
                () => Settings.Default.MainWindowHeight, d => Settings.Default.MainWindowHeight = d,
                () => Settings.Default.MainWindowWidth, d => Settings.Default.MainWindowWidth = d,
                () => Settings.Default.MainWindowState, s => Settings.Default.MainWindowState = s,
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
