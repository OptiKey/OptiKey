// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    [XmlRoot(ElementName = "Keyboard")]
    public class XmlKeyboard
    {
        public XmlKeyboard()
        {
            Name = "";
        }

        public XmlGrid Grid
        { get; set; }

        [XmlElement("Keys")]
        public XmlKeys Keys
        { get; set; }

        [XmlElement("Content")]
        public XmlDynamicItem Content
        { get; set; }

        // The following are all optional
        [XmlElement("KeyGroup")]
        public List<XmlKeyGroup> KeyGroups
        { get; set; }

        [XmlIgnore]
        public bool ShowOutputPanel
        { get; set; }

        [XmlElement("ShowOutputPanel")]
        public string ShowOutputPanelBoolAsString
        {
            get { return this.ShowOutputPanel ? "True" : "False"; }
            set { this.ShowOutputPanel = XmlUtils.ConvertToBoolean(value); }
        }

        public XmlKeyStates InitialKeyStates
        { get; set; }

        [XmlIgnore]
        public bool PersistNewState
        { get; set; }

        [XmlElement("PersistNewState")]
        public string PersistNewStateBoolAsString
        {
            get { return this.PersistNewState ? "True" : "False"; }
            set { this.PersistNewState = XmlUtils.ConvertToBoolean(value); }
        }

        public string WindowState
        { get; set; }

        public string Position
        { get; set; }

        public string DockSize
        { get; set; }

        public string Width
        { get; set; }

        public string Height
        { get; set; }

        public string HorizontalOffset
        { get; set; }

        public string VerticalOffset
        { get; set; }

        public string Name
        { get; set; }

        public string Symbol
        { get; set; }

        public double? SymbolMargin
        { get; set; }

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string BackgroundColor
        { get; set; }

        public string BorderColor
        { get; set; }

        [XmlIgnore]
        public Thickness? BorderThickness
        { get; set; }

        [XmlElement("BorderThickness")]
        public string BorderThicknessAsString
        {
            get { return BorderThickness.ToString(); }
            set
            {
                try
                {
                    ThicknessConverter thicknessConverter = new ThicknessConverter();
                    BorderThickness = (Thickness)thicknessConverter.ConvertFromString(value);
                }
                catch (System.FormatException)
                {
                    Log.ErrorFormat("Cannot interpret \"{0}\" as thickness", value);
                }
            }
        }

        [XmlIgnore]
        public bool Hidden
        { get; set; }

        [XmlElement("HideFromKeyboardMenu")]
        public string HiddenBoolAsString
        {
            get { return this.Hidden ? "True" : "False"; }
            set { this.Hidden = XmlUtils.ConvertToBoolean(value); }
        }

        [XmlIgnore]
        public bool IsShiftAware
        { get; set; }

        [XmlElement("IsShiftAware")]
        public string IsShiftAwareAsString
        {
            get { return this.IsShiftAware ? "True" : "False"; }
            set { this.IsShiftAware = XmlUtils.ConvertToBoolean(value); }
        }

        public static XmlKeyboard ReadFromFile(string inputFilename)
        {
            XmlKeyboard keyboard;

            // If no extension given, try ".xml"
            string ext = Path.GetExtension(inputFilename);
            bool exists = File.Exists(inputFilename);
            if (!File.Exists(inputFilename) &&
                string.IsNullOrEmpty(Path.GetExtension(inputFilename)))
            {
                inputFilename += ".xml";
            }

            // Read in XML file (may throw)
            XmlSerializer serializer = new XmlSerializer(typeof(XmlKeyboard));
            using (FileStream readStream = new FileStream(@inputFilename, FileMode.Open, FileAccess.Read))
            {
                keyboard = (XmlKeyboard)serializer.Deserialize(readStream);
            }

            return keyboard;
        }
    }
}