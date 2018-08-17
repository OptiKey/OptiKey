using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Models;
using log4net;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    public class PluginEngine : IPluginEngine
    {

        #region Private Member Vars

        private static Dictionary<string, Plugin> AvailablePlugins = new Dictionary<string, Plugin>();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public methods

        public void LoadAvailablePlugins()
        {
            const string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\Plugins\";

            var applicationDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ApplicationDataSubPath);

            foreach (string file in Directory.GetFiles(applicationDataPath, "*.dll"))
            {
                Plugin plugin = ValidatePlugin(file);
                if (plugin != null)
                {
                    AvailablePlugins.Add(plugin.Id, plugin);
                }
            }
        }

        public void CallPlugin(string PluginId, string MethodName)
        {
            Plugin plugin = AvailablePlugins[PluginId];
            plugin.Type.InvokeMember(MethodName, BindingFlags.InvokeMethod, null, plugin.Instance, null);
        }

        public bool PluginExists(string PluginName)
        {
            throw new NotImplementedException();
        }

        public List<Plugin> GetAllAvailablePlugins()
        {
            throw new NotImplementedException();
        }

        public void RefreshAvailablePlugins()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private Plugin ValidatePlugin(string file)
        {
            Plugin plugin = null;

            try
            {
                var DLL = Assembly.LoadFile(file);
                Type[] types = DLL.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Namespace.StartsWith("OptiKey"))
                    {
                        var instance = Activator.CreateInstance(type);
                        string pluginId = (string)type.InvokeMember("GetPluginId", BindingFlags.InvokeMethod, null, instance, null);
                        string pluginName = (string)type.InvokeMember("GetPluginName", BindingFlags.InvokeMethod, null, instance, null);
                        string pluginDescription = (string)type.InvokeMember("GetPluginDescription", BindingFlags.InvokeMethod, null, instance, null);

                        if (pluginName != null && pluginDescription != null)
                        {
                            plugin = new Plugin(pluginId, pluginName, pluginDescription, instance, type);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Error creating plugin instance: [{0}].", file, e);
            }
            return plugin;
        }

        #endregion

    }
}
