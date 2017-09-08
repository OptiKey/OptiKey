using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlActionKey : XmlKey
    {

        [XmlIgnore]
        public FunctionKeys? Action
        { get; set; }

        [XmlElement("Action")]
        public string FunctionKeyAsString
        {
            get {
                return Action.ToString();
            }
            set
            {
                if (null != value)
                {
                    FunctionKeys fKey;
                    if (System.Enum.TryParse(value, out fKey))
                    {
                        Action = fKey;
                    }
                    else
                    {
                        Log.ErrorFormat("Could not parse {0} as function key", value);
                    }
                }
            }
        }
    }
}