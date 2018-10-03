using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
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
            var applicationDataPath = Settings.Default.PluginsLocation;

            foreach (string file in Directory.GetFiles(applicationDataPath, "*.dll"))
            {
                List<Plugin> plugins = ValidateAndCreatePlugins(file);
                foreach (Plugin plugin in plugins)
                {
                    AvailablePlugins.Add(plugin.Id, plugin);
                }
            }
        }

        public static void RunPlugin(Dictionary<String, String> context, XmlPluginKey key)
        {
            Plugin plugin = AvailablePlugins[key.Plugin];
            List<string> methodArgs = null;
            if (key.Arguments?.Argument?.Count > 0)
            {
                // FIXME: This logic does not support two methods with the same name and different arguments
                methodArgs = new List<String>();
                foreach (MethodInfo pluginMethod in plugin.Type.GetMethods())
                {
                    if (pluginMethod.Name.Equals(key.Method))
                    {
                        foreach (ParameterInfo pluginMethodParam in pluginMethod.GetParameters())
                        {
                            string argValue = null;
                            foreach (PluginArgument arg in key.Arguments.Argument)
                            {
                                if (arg.Name.Equals(pluginMethodParam.Name))
                                {
                                    argValue = GetArgumentValue(context, arg.Value);
                                    break;
                                }
                            }
                            methodArgs.Add(argValue);
                        }
                        break;
                    }
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

        private static string GetArgumentValue(Dictionary<String, String> context, string value)
        {
            string result = value;
            if (value.StartsWith("$"))
            {
                string contextValue = context[value.Substring(1)];
                if (contextValue != null)
                {
                    result = contextValue;
                }
            }
            return result;
        }

        #endregion

    }
}
