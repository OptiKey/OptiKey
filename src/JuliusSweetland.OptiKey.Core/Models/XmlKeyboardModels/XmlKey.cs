// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Reflection;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlKey : IXmlKey
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public XmlKey()
        {
            Width = 1;
            Height = 1;
        }

        public int Row { get; set; }
        public int Col { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Label { get; set; } //Either set this, or the ShiftUpLabel and ShiftDownLabel, and/or the Symbol. These values become the Text value on the created Key.
        public string ShiftUpLabel { get; set; }
        public string ShiftDownLabel { get; set; }
        public string Symbol { get; set; }
        public string SharedSizeGroup { get; set; } //Optional - only required to break out a key, or set of keys, to size separately, otherwise size grouping is determined automatically
        public bool AutoScaleToOneKeyWidth { get; set; } = true;
        public bool AutoScaleToOneKeyHeight { get; set; } = true;
        public bool UsePersianCompatibilityFont { get; set; }
        public bool UseUnicodeCompatibilityFont { get; set; }
        public bool UseUrduCompatibilityFont { get; set; }
        public string BackgroundColor { get; set; }
    }
}