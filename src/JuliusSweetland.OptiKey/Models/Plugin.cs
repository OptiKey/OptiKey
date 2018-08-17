using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Models
{
    // TODO: Checar classe SynamicKeyboardFolder nesse mesmo pacote. 
    public class Plugin
    {
        private string id;
        private string pluginName;
        private string pluginDescription;
        private object instance;
        private Type type;

        public Plugin(string id, string pluginName, string pluginDescription, object instance, Type type)
        {
            this.id = id;
            this.pluginName = pluginName;
            this.pluginDescription = pluginDescription;
            this.instance = instance;
            this.type = type;
        }

        public string Id { get => id; set => id = value; }
        public string PluginName { get => pluginName; set => pluginName = value; }
        public string PluginDescription { get => pluginDescription; set => pluginDescription = value; }
        public object Instance { get => instance; set => instance = value; }
        public Type Type { get => type; set => type = value; }
    }
}
