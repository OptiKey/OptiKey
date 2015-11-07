using System.Collections.Generic;
using System.ComponentModel;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.Services
{
    public interface IConfigurableCommandService : INotifyPropertyChanged, INotifyErrors
    {
        /// <summary>
        /// Contains all supported commands.
        /// A command is defined for a given FunctionKeys, and contains a spoken patterns.
        /// </summary>
        Dictionary<FunctionKeys, string> Commands { get; set; }

        /// <summary>
        /// Loads the configrable commands for the specified language.
        /// </summary>
        /// <param name="language">Language to load commands for</param>
        void Load(Languages language);

        /// <summary>
        /// Save the current configrable commands into the specified language file.
        /// </summary>
        /// <param name="language">Language for which current commands are saved</param>
        void Save(Languages language);

        // TODO activation state
    }
}
