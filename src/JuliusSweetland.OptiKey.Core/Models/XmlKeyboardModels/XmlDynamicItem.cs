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

        public int Row { get; set; } = -1;
        public int Col { get; set; } = -1;
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public string BackgroundColor { get; set; }
        public string Opacity { get; set; }
        public string KeyDownBackground { get; set; }
        public string KeyDownOpacity { get; set; }
    }

    public class XmlDynamicScratchpad : XmlDynamicItem { }
    public class XmlDynamicSuggestionRow : XmlDynamicItem { }
    public class XmlDynamicSuggestionCol : XmlDynamicItem { }
}
