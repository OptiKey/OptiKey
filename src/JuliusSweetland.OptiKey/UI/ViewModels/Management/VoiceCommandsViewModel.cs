using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{

    // TODO doc +
    // TODO pattern can be null, meaning disabled
    public class ConfigurableCommand: BindableBase
    {
        private FunctionKeys function;
        public FunctionKeys Function
        {
            get { return function; }
            set { SetProperty(ref function, value); }
        }
        
        private string pattern;
        public string Pattern
        {
            get { return pattern; }
            set { SetProperty(ref pattern, value); }
        }

        private string label;
        public string Label
        {
            get { return label; }
            set { label = value; }
        }
    }

    public class VoiceCommandsViewModel : BindableBase
    {
        private readonly IConfigurableCommandService configurableCommandService;
        private readonly WordsViewModel wordsViewModel;

        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public VoiceCommandsViewModel(IConfigurableCommandService configurableCommandService, WordsViewModel wordsViewModel)
        {
            this.configurableCommandService = configurableCommandService;
            this.wordsViewModel = wordsViewModel;
            commands = new ObservableCollection<ConfigurableCommand>();

            Load(); 
            //Reloads commands on language changes
            wordsViewModel.OnPropertyChanges(view => view.ResourceLanguage).Subscribe(_ => Load());
        }

        #endregion
        
        #region Properties
        
        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set { SetProperty(ref enabled, value); }
        }

        private bool feedback;
        public bool Feedback
        {
            get { return feedback; }
            set { SetProperty(ref feedback, value); }
        }

        private string prefix;
        public string Prefix
        {
            get { return prefix; }
            set { SetProperty(ref prefix, value); }
        }

        private ObservableCollection<ConfigurableCommand> commands;
        public ObservableCollection<ConfigurableCommand> Commands
        {
            get { return commands; }
            set { SetProperty(ref commands, value); }
        }

        public bool ChangesRequireRestart
        {
            get { return false; }
        }
        
        #endregion
        
        #region Methods

        private string ComputeLabel(FunctionKeys function)
        {
            var snakeCase = String.Concat(function.ToString().Select((x, i) => i > 0 && char.IsUpper(x) || char.IsDigit(x) ? "_" + x.ToString() : x.ToString()));
            return "VC_" + snakeCase.ToUpper();
        }

        private void Load()
        {
            Enabled = Settings.Default.VoiceCommandsEnabled;
            Prefix = Settings.Default.VoiceCommandsPrefix;
            Feedback = Settings.Default.VoiceCommandsFeedback;

            commands.Clear();
            //Loads the currently edited language, even if it was not changed.
            //It avoids editing English commands while user has change language from English to German and not applied yet.
            configurableCommandService.Load(wordsViewModel.ResourceLanguage);
            
            if (configurableCommandService.Commands != null)
            {
                //Loads all possible functions
                foreach (FunctionKeys function in Enum.GetValues(typeof(FunctionKeys)))
                {
                    //Get pattern for current function. If nothing found, use an empty string.
                    string pattern = "";
                    configurableCommandService.Commands.TryGetValue(function, out pattern);
                    var property = typeof(Resources).GetProperty(ComputeLabel(function));
                    if (property != null)
                    {
                        commands.Add(new ConfigurableCommand { Function = function, Pattern = pattern, Label = (string)property.GetValue(null) });
                    }
                    else 
                    {
                        Log.Debug(string.Format("Unsupported voice command for function {0}", function));
                    }
                }
            }
        }

        public void ApplyChanges()
        {
            Settings.Default.VoiceCommandsEnabled = Enabled;
            Settings.Default.VoiceCommandsPrefix = Prefix;
            Settings.Default.VoiceCommandsFeedback = Feedback;

            //Update commands in configurable commands service, for the current language pattern
            foreach (var command in commands)
            {
                if (command.Pattern  == null || command.Pattern.Trim().Length == 0)
                {
                    //If not provided, remove from service
                    configurableCommandService.Commands.Remove(command.Function);
                }
                else
                {
                    //Updates or add pattern for this function to supported commands
                    configurableCommandService.Commands[command.Function] = command.Pattern.Trim();
                }
            }
            //Save everything, using the last selected to language because we don't know if Settings.Default has been updated yet
            configurableCommandService.Save(wordsViewModel.ResourceLanguage);
        }

        #endregion
    }
}
