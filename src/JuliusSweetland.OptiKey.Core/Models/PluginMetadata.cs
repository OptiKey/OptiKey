// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Xml.Serialization;
using System.Collections.Generic;
using System;

namespace JuliusSweetland.OptiKey.Models
{
    [XmlRoot(ElementName = "Argument")]
    public class Argument
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "Sample")]
        public string Sample { get; set; }
        [XmlElement(ElementName = "Required")]
        public string Required { get; set; }
        [XmlElement(ElementName = "DefaultValue")]
        public string DefaultValue { get; set; }

        public string ArgumentDescriptionSuffix
        {
            get
            {
                string description = (Description != null && Description.EndsWith(".")) ? " " : ". ";
                if (Required != null)
                {
                    description += "Required: " + Required + ". ";
                }
                if (DefaultValue != null)
                {
                    description += "Default value: " + DefaultValue + ". ";
                }
                if (Sample != null)
                {
                    description += "Sample: " + Sample + ". ";
                }
                return description;
            }
        }
    }

    [XmlRoot(ElementName = "Arguments")]
    public class Arguments
    {
        [XmlElement(ElementName = "Argument")]
        public List<Argument> Argument { get; set; }
    }

    [XmlRoot(ElementName = "Method")]
    public class Method
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "Arguments")]
        public Arguments Arguments { get; set; }
        [XmlElement(ElementName = "SampleConfig")]
        public string SampleConfig { get; set; }
    }

    [XmlRoot(ElementName = "Methods")]
    public class Methods
    {
        [XmlElement(ElementName = "Method")]
        public List<Method> Method { get; set; }
    }

    [XmlRoot(ElementName = "Plugin")]
    public class Plugin
    {
        [XmlElement(ElementName = "Id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "Impl")]
        public string Impl { get; set; }
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "Methods")]
        public Methods Methods { get; set; }
        [XmlIgnore]
        public Type Type { get; set; }
        [XmlIgnore]
        public object Instance{ get; set; }
    }

    [XmlRoot(ElementName = "Plugins")]
    public class Plugins
    {
        [XmlElement(ElementName = "Plugin")]
        public List<Plugin> Plugin { get; set; }
    }
}
