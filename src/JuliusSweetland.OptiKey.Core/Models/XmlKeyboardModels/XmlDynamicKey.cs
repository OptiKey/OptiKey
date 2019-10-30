// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - AllRights Reserved
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlDynamicKey : XmlDynamicItem
    {
        public XmlDynamicKey() { }

        [XmlElement("Action", typeof(DynamicAction))]
        [XmlElement("DestinationKeyboard", typeof(DynamicLink))]
        //ignore for now. future funtionality
        //[XmlElement("KeyDown", typeof(DynamicKeyDown))]
        //[XmlElement("KeyUp", typeof(DynamicKeyUp))]
        //[XmlElement("KeyToggle", typeof(DynamicKeyToggle))]
        //[XmlElement("Loop", typeof(XmlDynamicKey))]
        [XmlElement("Text", typeof(DynamicText))]
        [XmlElement("Wait", typeof(DynamicWait))]
        public List<XmlDynamicKey> Steps { get; } = new List<XmlDynamicKey>();

        [XmlIgnore]
        public bool ReturnToThisKeyboard
        { get; set; }

        [XmlElement("ReturnToThisKeyboard")]
        public string ReturnToThisKeyboardAsString
        {
            get { return this.ReturnToThisKeyboard ? "True" : "False"; }
            set { this.ReturnToThisKeyboard = XmlUtils.ConvertToBoolean(value); }
        }

        //ignore for now. future funtionality
        //public int Count { get; set; } //If the item in the list is a Loop, then Count is the number of repetitions
        public string Label { get; set; } //Either set this, the Symbol, or both. This value become the Text value on the created Key.
        public string ShiftDownLabel { get; set; } //Optional - only required to display an alternate Text value when the shift key is down.
        public string Symbol { get; set; }
        public string SharedSizeGroup { get; set; } //Optional - only required to break out a key, or set of keys, to size separately, otherwise size grouping is determined automatically
        public bool AutoScaleToOneKeyWidth { get; set; } = true;
        public bool AutoScaleToOneKeyHeight { get; set; } = true;
        public bool UsePersianCompatibilityFont { get; set; }
        public bool UseUnicodeCompatibilityFont { get; set; }
        public bool UseUrduCompatibilityFont { get; set; }
    }

    public class DynamicAction : XmlDynamicKey
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicLink : XmlDynamicKey
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicKeyDown : XmlDynamicKey
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicKeyUp : XmlDynamicKey
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicKeyToggle : XmlDynamicKey
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicText : XmlDynamicKey
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicWait : XmlDynamicKey
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicPlugin : XmlDynamicKey
    {
        public string Plugin
        { get; set; }

        public string Method
        { get; set; }

        [XmlElement("Argument")]
        public List<DynamicArgument> Argument
        { get; set; }
    }

    public class DynamicArgument
    {
        public string Name
        { get; set; }
        public string Value
        { get; set; }
    }
}
