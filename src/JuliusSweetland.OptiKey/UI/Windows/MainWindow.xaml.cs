using System.Windows;
using System.Windows.Input;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Static;
using log4net;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAudioService audioService;
        private readonly IDictionaryService dictionaryService;
        private readonly IInputService inputService;
        private readonly IKeyStateService keyStateService;
        private readonly InteractionRequest<NotificationWithServices> managementWindowRequest;

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

            managementWindowRequest = new InteractionRequest<NotificationWithServices>();

            //Setup key binding (Alt-M and Shift-Alt-M) to open settings
            InputBindings.Add(new KeyBinding
            {
                Command = new DelegateCommand(RequestManagementWindow),
                Modifiers = ModifierKeys.Alt,
                Key = Key.M
            });
            InputBindings.Add(new KeyBinding
            {
                Command = new DelegateCommand(RequestManagementWindow),
                Modifiers = ModifierKeys.Shift | ModifierKeys.Alt,
                Key = Key.M
            });

            Title = string.Format(Properties.Resources.WINDOW_TITLE, DiagnosticInfo.AssemblyVersion);
        }

        public InteractionRequest<NotificationWithServices> ManagementWindowRequest { get { return managementWindowRequest; } }

        private void RequestManagementWindow()
        {
            inputService.RequestSuspend();
            var restoreModifierStates = keyStateService.ReleaseModifiers(Log);
            ManagementWindowRequest.Raise(new NotificationWithServices 
                { AudioService = audioService, DictionaryService = dictionaryService },
                _ =>
                {
                    inputService.RequestResume();
                    restoreModifierStates();
                });
        }
    }
}
