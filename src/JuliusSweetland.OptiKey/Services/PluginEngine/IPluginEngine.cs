using JuliusSweetland.OptiKey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    interface IPluginEngine
    {
        void LoadAvailablePlugins();
        void RefreshAvailablePlugins();
        List<Plugin> GetAllAvailablePlugins();
        bool PluginExists(string PluginName);
        void CallPlugin(string PluginId, string MethodName);
    }
}
