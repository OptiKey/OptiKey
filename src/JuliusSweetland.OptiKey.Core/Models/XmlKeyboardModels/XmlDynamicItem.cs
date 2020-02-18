// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlDynamicItem
    {
        public XmlDynamicItem() { }

        [XmlElement("DynamicKey", typeof(XmlDynamicKey))]
        [XmlElement("Scratchpad", typeof(XmlDynamicScratchpad))]
        [XmlElement("SuggestionRow", typeof(XmlDynamicSuggestionRow))]
        [XmlElement("SuggestionCol", typeof(XmlDynamicSuggestionCol))]
        public List<XmlDynamicItem> Items { get; } = new List<XmlDynamicItem>();
        
        [XmlElement("KeyGroup")]
        public List<KeyGroup> KeyGroups
        { get; set; }

        [XmlAttribute] public int Row { get; set; } = -1;
        [XmlAttribute] public int Col { get; set; } = -1;
        [XmlAttribute] public int Width { get; set; } = 1;
        [XmlAttribute] public int Height { get; set; } = 1;
        [XmlAttribute] public string ForegroundColor { get; set; }
        [XmlAttribute] public string KeyDisabledForeground { get; set; }
        [XmlAttribute] public string BackgroundColor { get; set; }
        [XmlAttribute] public string KeyDisabledBackground { get; set; }
        [XmlAttribute] public string KeyDownBackground { get; set; }
        [XmlAttribute] public string Opacity { get; set; }
        [XmlAttribute] public string KeyDisabledOpacity { get; set; }
        [XmlAttribute] public string KeyDownOpacity { get; set; }
        [XmlAttribute] public int LockOnTime { get; set; } = -1;
        [XmlAttribute] public int CompletionTime { get; set; } = -1;
        [XmlAttribute] public int RepeatDelay { get; set; } = -1;
        [XmlAttribute] public int RepeatRate { get; set; } = -1;
    }

    public class XmlDynamicScratchpad : XmlDynamicItem { }
    public class XmlDynamicSuggestionRow : XmlDynamicItem { }
    public class XmlDynamicSuggestionCol : XmlDynamicItem { }
    public class KeyGroup
    {
        [XmlText]
        public string Value { get; set; }
    }
}
