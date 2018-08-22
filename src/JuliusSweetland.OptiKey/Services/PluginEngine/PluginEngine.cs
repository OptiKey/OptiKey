using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Models;
using log4net;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    public class PluginEngine
    {

        #region Private Member Vars

        private static Dictionary<string, Plugin> AvailablePlugins = new Dictionary<string, Plugin>();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public methods

        public static void LoadAvailablePlugins()
        {
            const string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\Plugins\";

            var applicationDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ApplicationDataSubPath);

            foreach (string file in Directory.GetFiles(applicationDataPath, "*.dll"))
            {
                List<Plugin> plugins = ValidateAndCreatePlugins(file);
                foreach (Plugin plugin in plugins)
                {
                    AvailablePlugins.Add(plugin.Id, plugin);
                }
            }
        }

        public static void RunPlugin(XmlPluginKey key)
        {
            Plugin plugin = AvailablePlugins[key.Plugin];
            List<string> methodArgs = null;
            if (key.Arguments?.Arg?.Count > 0)
            {
                methodArgs = new List<String>();
                foreach (string arg in key.Arguments.Arg)
                {
                    methodArgs.Add(arg);
                }
            }
            plugin.Type.InvokeMember(key.Method, BindingFlags.InvokeMethod, null, plugin.Instance, methodArgs?.ToArray());
        }

        public static bool PluginExists(string PluginId)
        {
            return AvailablePlugins.ContainsKey(PluginId);
        }

        public static void RefreshAvailablePlugins()
        {
            AvailablePlugins = new Dictionary<string, Plugin>();
            LoadAvailablePlugins();
        }

        public static List<Plugin> GetAllAvailablePlugins()
        {
            return AvailablePlugins.Values.ToList();
        }

        #endregion

        #region Private methods

        private static List<Plugin> ValidateAndCreatePlugins(string file)
        {
            List<Plugin> plugins = new List<Plugin>();

            try
            {
                Assembly DLL = Assembly.LoadFile(file);
                string metadataResName = Array.Find(DLL.GetManifestResourceNames(), new Predicate<String>(FindMetadataResource));
                if (null != metadataResName)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Plugins));
                    Plugins pluginMetadata = (Plugins)serializer.Deserialize(new StringReader(new StreamReader(DLL.GetManifestResourceStream(metadataResName)).ReadToEnd()));
                    foreach (Plugin plugin in pluginMetadata.Plugin)
                    {
                        Type pluginType = DLL.GetType(plugin.Impl);
                        if (null != pluginType)
                        {
                            plugin.Instance = Activator.CreateInstance(pluginType);
                            plugin.Type = pluginType;
                            plugins.Add(plugin);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Error handling library: [{0}].", file, e);
                // Ensure that all plugins from this DLL are discarded.
                plugins = new List<Plugin>();
            }
            return plugins;
        }

        private static bool FindMetadataResource(string resource)
        {
            return resource.StartsWith("JuliusSweetland.OptiKey.") && resource.EndsWith("metadata.xml");
        }
        #endregion

    }
}
