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

        [XmlAttribute] public int Row { get; set; } = -1;
        [XmlAttribute] public int Col { get; set; } = -1;
        [XmlAttribute] public int Width { get; set; } = 1;
        [XmlAttribute] public int Height { get; set; } = 1;
        [XmlAttribute] public string BackgroundColor { get; set; }
        [XmlAttribute] public string Opacity { get; set; }
        [XmlAttribute] public string KeyDownBackground { get; set; }
        [XmlAttribute] public string KeyDownOpacity { get; set; }
    }

    public class XmlDynamicScratchpad : XmlDynamicItem { }
    public class XmlDynamicSuggestionRow : XmlDynamicItem { }
    public class XmlDynamicSuggestionCol : XmlDynamicItem { }
}
