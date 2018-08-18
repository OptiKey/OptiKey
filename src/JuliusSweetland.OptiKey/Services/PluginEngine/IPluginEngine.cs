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
        bool PluginExists(string PluginId);
        void RunPlugin(string PluginId, string MethodName);
        List<Plugin> GetAllAvailablePlugins();
    }
}
