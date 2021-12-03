// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlKeys
    {
        [XmlElement(ElementName = "ActionKey")]
        public List<XmlActionKey> ActionKeys
        { get; set; }

        [XmlElement(ElementName = "TextKey")]
        public List<XmlTextKey> TextKeys
        { get; set; }

        [XmlElement(ElementName = "ChangeKeyboardKey")]
        public List<XmlChangeKeyboardKey> ChangeKeyboardKeys
        { get; set; }

        [XmlElement(ElementName = "PluginKey")]
        public List<XmlPluginKey> PluginKeys
        { get; set; }

        [XmlElement(ElementName = "DynamicKey")]
        public List<XmlDynamicKey> DynamicKeys
        { get; set; }

        [XmlIgnore]
        public int Count
        {
            get
            {
                return ActionKeys.Count + TextKeys.Count + ChangeKeyboardKeys.Count + PluginKeys.Count + DynamicKeys.Count;
            }
        }
    }
}