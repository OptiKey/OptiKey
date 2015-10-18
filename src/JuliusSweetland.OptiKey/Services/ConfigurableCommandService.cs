using System.Collections.Generic;
using System.ComponentModel;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class ConfigurableCommandService : BindableBase, IConfigurableCommandService
    {
        
        #region Fields

        private Dictionary<FunctionKeys, Dictionary<Languages, string>> commands;
        public Dictionary<FunctionKeys, Dictionary<Languages, string>> Commands
        {
            get { return commands; }
            set { SetProperty(ref commands, value); }
        }

        #endregion

        #region Ctor

        public ConfigurableCommandService()
        {
            commands = new Dictionary<FunctionKeys, Dictionary<Languages, string>>();
            commands.Add(FunctionKeys.MouseMoveAndLeftClick, new Dictionary<Languages, string>()
            {
                { Languages.Custom, "clic" }
            });
        }

        #endregion
    }
}
