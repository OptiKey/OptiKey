// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - AllRights Reserved
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlDynamicKey : XmlDynamicItem , IXmlKey
    {
        public XmlDynamicKey() { }

        [XmlElement("Action", typeof(DynamicAction))]
        [XmlElement("ChangeKeyboard", typeof(DynamicLink))]
        [XmlElement("KeyDown", typeof(DynamicKeyDown))]
        [XmlElement("KeyUp", typeof(DynamicKeyUp))]
        [XmlElement("KeyToggle", typeof(DynamicKeyToggle))]
        [XmlElement("Loop", typeof(DynamicLoop))]
        [XmlElement("Plugin", typeof(DynamicPlugin))]
        [XmlElement("MoveWindow", typeof(DynamicMove))]
        [XmlElement("Text", typeof(DynamicText))]
        [XmlElement("Wait", typeof(DynamicWait))]
        public List<XmlDynamicKey> Commands { get; } = new List<XmlDynamicKey>();
        
        public string Label { get; set; } //Either set this, the Symbol, or both. This value become the Text value on the created Key.
        public string ShiftDownLabel { get; set; } //Optional - only required to display an alternate Text value when the shift key is down.
        public string Symbol { get; set; }
        [XmlIgnore] public bool? AutoScaleToOneKeyWidth;
        [XmlIgnore] public bool? AutoScaleToOneKeyHeight;
        [XmlAttribute("AutoScaleToOneKeyWidth")]
        public string AutoScaleToOneKeyWidthString
        {
            get { return AutoScaleToOneKeyWidth.HasValue ? AutoScaleToOneKeyWidth.Value.ToString() : null; }
            set
            {
                if (bool.TryParse(value, out bool v))
                    AutoScaleToOneKeyWidth = v;
            }
        }
        [XmlAttribute("AutoScaleToOneKeyHeight")]
        public string AutoScaleToOneKeyHeightString
        {
            get { return AutoScaleToOneKeyHeight.HasValue ? AutoScaleToOneKeyHeight.Value.ToString() : null; }
            set
            {
                if (bool.TryParse(value, out bool v))
                    AutoScaleToOneKeyHeight = v;
            }
        }
        [XmlAttribute] public string SharedSizeGroup { get; set; } //Optional - only required to break out a key, or set of keys, to size separately, otherwise size grouping is determined automatically
        [XmlAttribute] public bool UsePersianCompatibilityFont { get; set; }
        [XmlAttribute] public bool UseUnicodeCompatibilityFont { get; set; }
        [XmlAttribute] public bool UseUrduCompatibilityFont { get; set; }
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

        [XmlIgnore]
        public bool BackReturnsHere
        { get; set; } = true;

        [XmlAttribute("BackReturnsHere")]
        public string BackReturnsHereAsString
        {
            get { return this.BackReturnsHere ? "True" : "False"; }
            set { this.BackReturnsHere = XmlUtils.ConvertToBoolean(value); }
        }
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

    public class DynamicLoop : XmlDynamicKey
    {
        public DynamicLoop() { }

        [XmlElement("Action", typeof(DynamicAction))]
        [XmlElement("ChangeKeyboard", typeof(DynamicLink))]
        [XmlElement("KeyDown", typeof(DynamicKeyDown))]
        [XmlElement("KeyUp", typeof(DynamicKeyUp))]
        [XmlElement("KeyToggle", typeof(DynamicKeyToggle))]
        [XmlElement("Loop", typeof(DynamicLoop))]
        [XmlElement("Plugin", typeof(DynamicPlugin))]
        [XmlElement("MoveWindow", typeof(DynamicMove))]
        [XmlElement("Text", typeof(DynamicText))]
        [XmlElement("Wait", typeof(DynamicWait))]
        public List<XmlDynamicKey> LoopCommands { get; } = new List<XmlDynamicKey>();

        [XmlAttribute] public int Count { get; set; } = 1; //The number of loop repetitions
    }

    public class DynamicMove : XmlDynamicKey
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
        public string Name
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
