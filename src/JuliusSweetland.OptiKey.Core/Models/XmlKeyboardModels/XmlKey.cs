// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Reflection;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlKey
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public XmlKey()
        {
            Width = 1;
            Height = 1;
        }

        public int Row
        { get; set; }

        public int Col
        { get; set; }

        public string Label
        { get; set; }

        public string ShiftUpLabel
        { get; set; }

        public string ShiftDownLabel
        { get; set; }

        public string Symbol
        { get; set; }

        public int Width
        { get; set; }

        public int Height
        { get; set; }

    }
}