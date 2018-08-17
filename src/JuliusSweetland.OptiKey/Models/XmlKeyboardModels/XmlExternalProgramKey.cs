using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlExternalProgramKey : XmlKey
    {
        public string CommandLine
        { get; set; }
    }
}