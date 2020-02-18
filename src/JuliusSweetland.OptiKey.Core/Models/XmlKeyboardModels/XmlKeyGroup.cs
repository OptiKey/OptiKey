// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - AllRights Reserved
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlKeyGroup
    {
        [XmlAttribute] public string Name { get; set; } //This is the name used to identify the set of characteristics
        [XmlAttribute] public string ForegroundColor { get; set; }
        [XmlAttribute] public string KeyDisabledForeground { get; set; }
        [XmlAttribute] public string BackgroundColor { get; set; }
        [XmlAttribute] public string KeyDisabledBackground { get; set; }
        [XmlAttribute] public string KeyDownBackground { get; set; }
        [XmlAttribute] public string Opacity { get; set; }
        [XmlAttribute] public string KeyDisabledOpacity { get; set; }
        [XmlAttribute] public string KeyDownOpacity { get; set; }
        [XmlAttribute] public bool AutoScaleToOneKeyWidth { get; set; } = true;
        [XmlAttribute] public bool AutoScaleToOneKeyHeight { get; set; } = true;
        [XmlAttribute] public string SharedSizeGroup { get; set; } //Optional - only required to break out a key, or set of keys, to size separately, otherwise size grouping is determined automatically
        [XmlAttribute] public bool UsePersianCompatibilityFont { get; set; }
        [XmlAttribute] public bool UseUnicodeCompatibilityFont { get; set; }
        [XmlAttribute] public bool UseUrduCompatibilityFont { get; set; }
        [XmlAttribute] public int LockOnTime { get; set; } = -1;
        [XmlAttribute] public int CompletionTime { get; set; } = -1;
        [XmlAttribute] public int RepeatDelay { get; set; } = -1;
        [XmlAttribute] public int RepeatRate { get; set; } = -1;
    }
}
