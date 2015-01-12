using System.Windows;
using System.Windows.Input;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.ETTA.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IAudioService audioService;
        private readonly IDictionaryService dictionaryService;
        private readonly IKeyboardService keyboardService;
        private readonly WindowStatePersistenceService windowStatePersistenceService;
        private readonly InteractionRequest<NotificationWithServices> managementWindowRequest;

        public MainWindow(
            IAudioService audioService,
            IDictionaryService dictionaryService,
            IKeyboardService keyboardService)
        {
            InitializeComponent();

            this.audioService = audioService;
            this.dictionaryService = dictionaryService;
            this.keyboardService = keyboardService;

            //Instantiate window state persistence service and provide accessors to the appropriate settings for this window
            windowStatePersistenceService = new WindowStatePersistenceService(
                () => Settings.Default.MainWindowTop, d => Settings.Default.MainWindowTop = d,
                () => Settings.Default.MainWindowLeft, d => Settings.Default.MainWindowLeft = d,
                () => Settings.Default.MainWindowHeight, d => Settings.Default.MainWindowHeight = d,
                () => Settings.Default.MainWindowWidth, d => Settings.Default.MainWindowWidth = d,
                () => Settings.Default.MainWindowState, s => Settings.Default.MainWindowState = s,
                Settings.Default);

            managementWindowRequest = new InteractionRequest<NotificationWithServices>();

            //Setup key binding (Alt-C) to open settings
            var openSettingsKeyBinding = new KeyBinding
            {
                Command = new DelegateCommand(RequestManagementWindow),
                Modifiers = ModifierKeys.Alt,
                Key = Key.M
            };
            InputBindings.Add(openSettingsKeyBinding);

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

        public InteractionRequest<NotificationWithServices> ManagementWindowRequest { get { return managementWindowRequest; } }

        private void RequestManagementWindow()
        {
            keyboardService.KeyEnabledStates.DisableAll = true;
            ManagementWindowRequest.Raise(new NotificationWithServices 
                { AudioService = audioService, DictionaryService = dictionaryService },
                _ => { keyboardService.KeyEnabledStates.DisableAll = false; });
        }
    }
}
