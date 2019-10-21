// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlDynamicItem : XmlVisualItem
    {
        public XmlDynamicItem() { }

        [XmlElement("Action", typeof(DynamicAction))]
        [XmlElement("DestinationKeyboard", typeof(DynamicLink))]
        [XmlElement("Text", typeof(DynamicText))]
        [XmlElement("Wait", typeof(DynamicWait))]
        public List<XmlDynamicItem> Steps { get; } = new List<XmlDynamicItem>();

        [XmlIgnore]
        public bool ReturnToThisKeyboard
        { get; set; }

        [XmlElement("ReturnToThisKeyboard")]
        public string ReturnToThisKeyboardAsString
        {
            get { return this.ReturnToThisKeyboard ? "True" : "False"; }
            set { this.ReturnToThisKeyboard = XmlUtils.ConvertToBoolean(value); }
        }

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

    public class DynamicAction : XmlDynamicItem
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicLink : XmlDynamicItem
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicText : XmlDynamicItem
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicWait : XmlDynamicItem
    {
        [XmlText]
        public string Value { get; set; }
    }

    public class DynamicPlugin : XmlDynamicItem
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
