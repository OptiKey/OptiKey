using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Static;
using JuliusSweetland.OptiKey.UI.ViewModels;
using log4net;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAudioService audioService;
        private readonly IDictionaryService dictionaryService;
        private readonly IInputService inputService;
        private readonly IKeyStateService keyStateService;
        private readonly IPointSource defaultPointSource;
        private readonly IPointSource manualModePointSource;
        private readonly InteractionRequest<NotificationWithServicesAndState> managementWindowRequest;
        private readonly ICommand managementWindowRequestCommand;
        private readonly ICommand toggleManualModeCommand;
        private readonly ICommand backCommand;
        private readonly ICommand quitCommand;
        private readonly ICommand restartCommand;

        public MainWindow(
            IAudioService audioService,
            IDictionaryService dictionaryService,
            IInputService inputService,
            IKeyStateService keyStateService)
        {
            InitializeComponent();

            this.audioService = audioService;
            this.dictionaryService = dictionaryService;
            this.inputService = inputService;
            this.keyStateService = keyStateService;

            defaultPointSource = inputService.PointSource;
            manualModePointSource = new MousePositionSource(Settings.Default.PointTtl) { State = RunningStates.Paused };

            managementWindowRequest = new InteractionRequest<NotificationWithServicesAndState>();
            managementWindowRequestCommand = new DelegateCommand(RequestManagementWindow);
            toggleManualModeCommand = new DelegateCommand(ToggleManualMode, () => !(defaultPointSource is MousePositionSource));
            quitCommand = new DelegateCommand(Quit);
            backCommand = new DelegateCommand(Back);
            restartCommand = new DelegateCommand(Restart);

            //Setup key binding (Alt+M and Shift+Alt+M) to open settings
            InputBindings.Add(new KeyBinding
            {
                Command = managementWindowRequestCommand,
                Modifiers = ModifierKeys.Alt,
                Key = Key.M
            });
            InputBindings.Add(new KeyBinding
            {
                Command = managementWindowRequestCommand,
                Modifiers = ModifierKeys.Shift | ModifierKeys.Alt,
                Key = Key.M
            });

            //Setup key binding (Alt+Enter and Shift+Alt+Enter) to open settings
            InputBindings.Add(new KeyBinding
            {
                Command = toggleManualModeCommand,
                Modifiers = ModifierKeys.Alt,
                Key = Key.Enter
            });
            InputBindings.Add(new KeyBinding
            {
                Command = toggleManualModeCommand,
                Modifiers = ModifierKeys.Shift | ModifierKeys.Alt,
                Key = Key.Enter
            });

            Title = string.Format(Properties.Resources.WINDOW_TITLE, DiagnosticInfo.AssemblyVersion);

            //Set the window size to 0x0 as this prevents a flicker where OptiKey would be displayed in the default position and then repositioned
            Width = 0;
            Height = 0;
        }

       

        public IWindowManipulationService WindowManipulationService { get; set; }

        public InteractionRequest<NotificationWithServicesAndState> ManagementWindowRequest { get { return managementWindowRequest; } }
        public ICommand ManagementWindowRequestCommand { get { return managementWindowRequestCommand; } }
        public ICommand ToggleManualModeCommand { get { return toggleManualModeCommand; } }
        public ICommand QuitCommand { get { return quitCommand; } }
        public ICommand BackCommand { get { return backCommand; } }
        public ICommand RestartCommand { get { return restartCommand; } }

        private void RequestManagementWindow()
        {
            Log.Info("RequestManagementWindow called.");

            var modalManagementWindow = WindowManipulationService != null &&
                                        WindowManipulationService.WindowState == WindowStates.Maximised;

            if (modalManagementWindow)
            {
                inputService.RequestSuspend();
            }
            var restoreModifierStates = keyStateService.ReleaseModifiers(Log);
            ManagementWindowRequest.Raise(
                new NotificationWithServicesAndState
                {
                    ModalWindow = modalManagementWindow,
                    AudioService = audioService,
                    DictionaryService = dictionaryService
                },
                _ =>
                {
                    if (modalManagementWindow)
                    {
                        inputService.RequestResume();
                    }
                    restoreModifierStates();
                });

            Log.Info("RequestManagementWindow complete.");
        }

        private void ToggleManualMode()
        {
            Log.Info("ToggleManualMode called.");

            if (MessageBox.Show(Properties.Resources.MANUAL_MODE_MESSAGE, Properties.Resources.MANUAL_MODE, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var mainViewModel = MainView.DataContext as MainViewModel;
                if (mainViewModel != null)
                {
                    inputService.RequestSuspend();
                    mainViewModel.DetachInputServiceEventHandlers();
                    var changingToManualMode = inputService.PointSource == defaultPointSource;
                    inputService.PointSource = changingToManualMode ? manualModePointSource : defaultPointSource;
                    mainViewModel.AttachInputServiceEventHandlers();
                    mainViewModel.RaiseToastNotification(Properties.Resources.MANUAL_MODE_CHANGED,
                        changingToManualMode ? Properties.Resources.MANUAL_MODE_ENABLED : Properties.Resources.MANUAL_MODE_DISABLED,
                        NotificationTypes.Normal, () => inputService.RequestResume());
                    mainViewModel.ManualModeEnabled = changingToManualMode;
                    keyStateService.ClearKeyHighlightStates(); //Clear any in-progress multi-key selection highlighting
                }
            }

            Log.Info("ToggleManualMode complete.");
        }

        private void Quit()
        {
            if (MessageBox.Show(Properties.Resources.QUIT_MESSAGE, Properties.Resources.QUIT, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Back()
        {
            var mainViewModel = MainView.DataContext as MainViewModel;
            if (null != mainViewModel)
            {
                mainViewModel.BackFromKeyboard();   
            }            
        }

        private void Restart()
        {
            if (MessageBox.Show(Properties.Resources.REFRESH_MESSAGE, Properties.Resources.RESTART, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
            }
        }
    }
}
