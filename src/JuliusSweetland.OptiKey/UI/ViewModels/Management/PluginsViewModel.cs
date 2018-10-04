using System;
using System.Collections.Generic;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services.PluginEngine;
using log4net;
using Prism.Mvvm;
using FontStretches = JuliusSweetland.OptiKey.Enums.FontStretches;
using FontWeights = JuliusSweetland.OptiKey.Enums.FontWeights;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class PluginsViewModel : BindableBase
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public PluginsViewModel()
        {
            Load();
        }
        
        #endregion
        
        #region Properties

        private string pluginsLocation;
        public string PluginsLocation
        {
            get { return pluginsLocation; }
            set { SetProperty(ref pluginsLocation, value); }
        }

        public List<Plugin> AvailablePlugins
        {
            get
            {
                return PluginEngine.GetAllAvailablePlugins();
            }
        }

        public bool ChangesRequireRestart
        {
            get
            {
                // TODO
                return false;
            }
        }

        #endregion

        #region Methods

        private void Load()
        {
            PluginsLocation = Settings.Default.PluginsLocation;
        }

        public void ApplyChanges()
        {
            Settings.Default.PluginsLocation = PluginsLocation;
        }

        #endregion
    }
}
