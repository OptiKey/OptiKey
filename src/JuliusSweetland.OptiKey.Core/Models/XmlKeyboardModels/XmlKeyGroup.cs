// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - AllRights Reserved
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlKeyGroup
    {
        [XmlAttribute] public string Name { get; set; } //This is the name used to identify the set of characteristics
        [XmlAttribute] public string ForegroundColor { get; set; }
        [XmlAttribute] public string KeyDisabledForeground { get; set; }
        [XmlAttribute] public string KeyDownForeground { get; set; }
        [XmlAttribute] public string BackgroundColor { get; set; }
        [XmlAttribute] public string KeyDisabledBackground { get; set; }
        [XmlAttribute] public string KeyDownBackground { get; set; }
        [XmlAttribute] public string BorderColor { get; set; }
        [XmlAttribute] public string BorderThickness { get; set; }
        [XmlAttribute] public string CornerRadius { get; set; }
        [XmlAttribute] public string Margin { get; set; }
        [XmlAttribute] public string Opacity { get; set; }
        [XmlAttribute] public string KeyDisabledOpacity { get; set; }
        [XmlAttribute] public string KeyDownOpacity { get; set; }
        [XmlIgnore] public bool? AutoScaleToOneKeyWidth;
        [XmlIgnore] public bool? AutoScaleToOneKeyHeight;
        [XmlAttribute("AutoScaleToOneKeyWidth")]
        public string AutoScaleToOneKeyWidthString
        {
            get { return AutoScaleToOneKeyWidth.HasValue ? AutoScaleToOneKeyWidth.Value.ToString() : null; }
            set
            {
                if (bool.TryParse(value, out bool v))
                    AutoScaleToOneKeyWidth = v;
            }
        }
        [XmlAttribute("AutoScaleToOneKeyHeight")]
        public string AutoScaleToOneKeyHeightString
        {
            get { return AutoScaleToOneKeyHeight.HasValue ? AutoScaleToOneKeyHeight.Value.ToString() : null; }
            set
            {
                if (bool.TryParse(value, out bool v))
                    AutoScaleToOneKeyHeight = v;
            }
        }
        [XmlAttribute] public string SharedSizeGroup { get; set; } //Optional - only required to break out a key, or set of keys, to size separately, otherwise size grouping is determined automatically
        [XmlAttribute] public bool UsePersianCompatibilityFont { get; set; }
        [XmlAttribute] public bool UseUnicodeCompatibilityFont { get; set; }
        [XmlAttribute] public bool UseUrduCompatibilityFont { get; set; }
        [XmlAttribute] public int LockOnTime { get; set; } = -1;
        //This is a comma separated list of times required to trigger keystrokes
        //When used the time to trigger the first keystroke is overridden and the time to trigger repetitive keystrokes can be shortened
        [XmlAttribute] public string CompletionTimes { get; set; }
        //The amount of time a continuous gaze is required to lock down a key
        //When used the key will be pressed down when triggerred and remain down as long as gaze is fixed on key
        [XmlAttribute] public int TimeRequiredToLockDown { get; set; } = -1;
        //The purpose of this is to allow the key to be quickly released if TimeRequiredToLockDown is used
        //It is the amount of time focus can be lost before resetting the key
        //When used this will override the keyFixationTimeout
        [XmlAttribute] public int LockDownAttemptTimeout { get; set; } = -1; 
    }
}
