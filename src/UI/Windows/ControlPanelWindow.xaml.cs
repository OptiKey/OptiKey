using System.Windows;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;

namespace JuliusSweetland.ETTA.UI.Windows
{
    /// <summary>
    /// Interaction logic for ControlPanelWindow.xaml
    /// </summary>
    public partial class ControlPanelWindow : Window
    {
        private readonly IDictionaryService dictionaryService;
        private readonly WindowStatePersistenceService windowStatePersistenceService;

        public ControlPanelWindow(IDictionaryService dictionaryService)
        {
            InitializeComponent();

            this.dictionaryService = dictionaryService;

            //Instantiate window state persistence service and provide accessors to the appropriate settings for this window
            windowStatePersistenceService = new WindowStatePersistenceService(
                () => Settings.Default.ControlPanelWindowTop, d => Settings.Default.ControlPanelWindowTop = d,
                () => Settings.Default.ControlPanelWindowLeft, d => Settings.Default.ControlPanelWindowLeft = d,
                () => Settings.Default.ControlPanelWindowHeight, d => Settings.Default.ControlPanelWindowHeight = d,
                () => Settings.Default.ControlPanelWindowWidth, d => Settings.Default.ControlPanelWindowWidth = d,
                () => Settings.Default.ControlPanelWindowState, s => Settings.Default.ControlPanelWindowState = s,
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
