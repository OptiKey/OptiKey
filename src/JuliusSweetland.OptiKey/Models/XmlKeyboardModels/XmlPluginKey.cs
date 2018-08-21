using System.Collections.Generic;
using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlPluginKey : XmlKey
    {
        public string Plugin
        { get; set; }

        public string Method
        { get; set; }

        [XmlElement(ElementName = "Arguments")]
        public List<PluginArgument> Arguments
        { get; set; }
    }

    public class PluginArgument
    {
        public string Arg
        { get; set; }
    }
}