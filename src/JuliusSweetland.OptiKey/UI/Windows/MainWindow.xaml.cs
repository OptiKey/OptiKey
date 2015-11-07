using System.Windows;
using System.Windows.Input;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Static;
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
        private readonly IAudioService audioService;
        private readonly IDictionaryService dictionaryService;
        private readonly IInputService inputService;
        private readonly IConfigurableCommandService configurableCommandService;
        private readonly InteractionRequest<NotificationWithServices> managementWindowRequest;

        public MainWindow(
            IAudioService audioService,
            IDictionaryService dictionaryService,
            IInputService inputService,
            IConfigurableCommandService configurableCommandService)
        {
            InitializeComponent();

            this.audioService = audioService;
            this.dictionaryService = dictionaryService;
            this.inputService = inputService;
            this.configurableCommandService = configurableCommandService;
            
            managementWindowRequest = new InteractionRequest<NotificationWithServices>();

            //Setup key binding (Alt-C and Shift-Alt-C) to open settings
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

            // TO REMOVE
            ContentRendered += (_1, _2) => { RequestManagementWindow(); };
        }

        public InteractionRequest<NotificationWithServices> ManagementWindowRequest { get { return managementWindowRequest; } }

        private void RequestManagementWindow()
        {
            inputService.RequestSuspend();
            ManagementWindowRequest.Raise(new NotificationWithServices 
                { AudioService = audioService, DictionaryService = dictionaryService, ConfigurableCommandService = configurableCommandService },
                _ => inputService.RequestResume());
        }
    }
}
