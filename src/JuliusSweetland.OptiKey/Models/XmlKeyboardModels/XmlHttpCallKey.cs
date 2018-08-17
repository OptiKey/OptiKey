using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlHttpCallKey : XmlKey
    {
        public string Url
        { get; set; }
    }
}