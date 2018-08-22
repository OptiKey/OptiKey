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

        public PluginArguments Arguments
        { get; set; }
    }

    public class PluginArguments
    {
        [XmlElement(ElementName = "Arg")]
        public List<string> Arg
        { get; set; }
    }
}