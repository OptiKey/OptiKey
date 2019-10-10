// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlDynamicKey : XmlKey
    {
        [XmlElement("Steps")]
        public List<DynamicStep> Steps
        { get; set; }
    }

    public class DynamicStep
    {
        public string Action
        { get; set; }

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

        public string Text
        { get; set; }

        public string Wait
        { get; set; }

        public string Plugin
        { get; set; }

        public string Method
        { get; set; }

        [XmlElement("Argument")]
        public List<DynamicPluginArgument> Argument
        { get; set; }
    }

    public class DynamicPluginArgument
    {
        public string Name
        { get; set; }
        public string Value
        { get; set; }
    }
}
