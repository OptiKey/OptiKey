using System.Collections.Generic;
using System.ComponentModel;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IConfigurableCommandService : INotifyPropertyChanged
    {
        /// <summary>
        /// Contains all supported commands.
        /// A command is defined for a given FunctionKeys, and contains a dictionnary of patterns.
        /// For each supported language, a pattern may be defined (english will be used as default value), 
        /// and user can customize command by providing pattern for "Custom" language.
        /// </summary>
        Dictionary<FunctionKeys, Dictionary<Languages, string>> Commands { get; set; }
    }
}
