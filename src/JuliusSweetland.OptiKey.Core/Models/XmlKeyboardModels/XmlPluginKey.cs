// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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

        [XmlElement("Arguments")]
        public PluginArguments Arguments
        { get; set; }
    }

    public class PluginArguments
    {
        [XmlElement("Argument")]
        public List<PluginArgument> Argument
        { get; set; }
    }

    public class PluginArgument
    {
        public string Name
        { get; set; }
        public string Value
        { get; set; }
    }
}