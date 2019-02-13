using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlChangeKeyboardKey : XmlKey
    {

        public string DestinationKeyboard
        { get; set; }

        [XmlIgnore]
        public bool ReturnToThisKeyboard
        { get; set; }
        
        [XmlElement("ReturnToThisKeyboard")]
        public string ReturnToThisKeyboardAsString
        {
            get { return this.ReturnToThisKeyboard ? "True" : "False"; }
            set { this.ReturnToThisKeyboard = XmlUtils.ConvertToBoolean(value); }            
        }
    }
}