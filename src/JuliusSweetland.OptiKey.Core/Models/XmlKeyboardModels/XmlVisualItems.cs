// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlVisualItem
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public XmlVisualItem() { }

        [XmlElement("DynamicKey", typeof(XmlDynamicItem))]
        [XmlElement("Scratchpad", typeof(XmlScratchpadItem))]
        [XmlElement("SuggestionRow", typeof(XmlSuggestionRowItem))]
        [XmlElement("SuggestionCol", typeof(XmlSuggestionColItem))]
        public List<XmlVisualItem> Items { get; } = new List<XmlVisualItem>();

        public int Row { get; set; } = -1;
        public int Col { get; set; } = -1;
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public string BackgroundColor { get; set; }
    }

    public class XmlScratchpadItem : XmlVisualItem
    {
        [XmlElement]
        public bool Value { get; set; } = false;
    }

    public class XmlSuggestionRowItem : XmlVisualItem
    {
        [XmlElement]
        public bool Value { get; set; } = false;
    }

    public class XmlSuggestionColItem : XmlVisualItem
    {
        [XmlElement]
        public bool Value { get; set; } = false;
    }
}