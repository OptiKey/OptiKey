// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class EyeGesture
    {
        public EyeGesture() { }

        public string Name { get; set; } = "New Gesture";

        public double Cooldown { get; set; } = 2000;

        private bool enabled;

        [XmlIgnore] public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        [XmlElement("Enabled")] public string EnabledAsString
        {
            get { return enabled.ToString(); }
            set { enabled = !bool.TryParse(value, out var result) || result; }
        }

        [XmlElement("Step")] public ObservableCollection<EyeGestureStep> Steps { get; set; }

        [XmlIgnore] public Point FixationPoint { get; set; }
        [XmlIgnore] public int StepIndex { get; set; } = 0;
        [XmlIgnore] public Point PointStamp { get; set; }
        [XmlIgnore] public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.Now;
    }
}