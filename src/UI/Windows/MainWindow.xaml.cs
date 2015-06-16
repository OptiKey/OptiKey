using System.Windows;
using System.Windows.Input;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Static;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

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
        private readonly InteractionRequest<NotificationWithServices> managementWindowRequest;

        public MainWindow(
            IAudioService audioService,
            IDictionaryService dictionaryService,
            IInputService inputService)
        {
            InitializeComponent();

            this.audioService = audioService;
            this.dictionaryService = dictionaryService;
            this.inputService = inputService;
            
            managementWindowRequest = new InteractionRequest<NotificationWithServices>();

            //Setup key binding (Alt-C) to open settings
            var openSettingsKeyBinding = new KeyBinding
            {
                Command = new DelegateCommand(RequestManagementWindow),
                Modifiers = ModifierKeys.Alt,
                Key = Key.M
            };
            InputBindings.Add(openSettingsKeyBinding);

            Title = string.Format("OptiKey v{0}", DiagnosticInfo.AssemblyVersion);
        }

        public InteractionRequest<NotificationWithServices> ManagementWindowRequest { get { return managementWindowRequest; } }

        private void RequestManagementWindow()
        {
            inputService.State = RunningStates.Paused;
            ManagementWindowRequest.Raise(new NotificationWithServices 
                { AudioService = audioService, DictionaryService = dictionaryService },
                _ => { inputService.State = RunningStates.Running; });
        }
    }
}
