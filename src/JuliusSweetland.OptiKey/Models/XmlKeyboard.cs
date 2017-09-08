using JuliusSweetland.OptiKey.Enums;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml;
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

        public XmlKeys Keys
        { get; set; }

        // The following are all optional
        public double? Height
        { get; set; }

        public string Name
        { get; set; }

        public string Symbol
        { get; set; }

        public double? SymbolMargin
        { get; set; }

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [XmlIgnore]
        public Thickness? BorderThickness
        { get; set; }

        [XmlElement("BorderThickness")]
        public string BorderThicknessAsString
        {
            get { return BorderThickness.ToString(); }
            set {
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
        
        public static XmlKeyboard ReadFromFile(string inputFilename)
        {
            XmlKeyboard keyboard;

            // If no extension given, try ".xml"
            string ext = Path.GetExtension(inputFilename);
            bool exists = File.Exists(inputFilename);
            if (!File.Exists(inputFilename) &&
                String.IsNullOrEmpty(Path.GetExtension(inputFilename)))
            {
                inputFilename += ".xml";
            } 

            // Read in XML file (may throw)
            XmlSerializer serializer = new XmlSerializer(typeof(XmlKeyboard));
            using (FileStream readStream = new FileStream(@inputFilename, FileMode.Open))
            {
                keyboard = (XmlKeyboard)serializer.Deserialize(readStream);
            }

            return keyboard;
        }
    }

    public class XmlGrid
    {
        public int Rows
        { get; set; }

        public int Cols
        { get; set; }

    }

    public class XmlKeys
    {
        [XmlElement(ElementName = "ActionKey")]
        public List<XmlActionKey> ActionKeys
        { get; set; }

        [XmlElement(ElementName = "TextKey")]
        public List<XmlTextKey> TextKeys
        { get; set; }

        [XmlElement(ElementName = "LinkKey")]
        public List<XmlLinkKey> LinkKeys
        { get; set; }

        [XmlIgnore]
        public int Count
        {
            get
            {
                return ActionKeys.Count + TextKeys.Count + LinkKeys.Count;
            }
        }
    }

    public class XmlLinkKey : XmlKey
    {

        public string Link
        { get; set; }

        [XmlIgnore]
        public bool ReturnToThisKeyboard
        { get; set; }
        
        [XmlElement("ReturnToThisKeyboard")]
        public string ReturnToThisKeyboardAsString
        {
            get { return this.ReturnToThisKeyboard ? "True" : "False"; }
            set { this.ReturnToThisKeyboard = XmlUtils.ConvertToBoolean(value); }            
        }
    }

    public class XmlActionKey : XmlKey
    {

        [XmlIgnore]
        public FunctionKeys? Action
        { get; set; }

        [XmlElement("Action")]
        public string FunctionKeyAsString
        {
            get {
                return Action.ToString();
            }
            set
            {
                if (null != value)
                {
                    FunctionKeys fKey;
                    if (System.Enum.TryParse(value, out fKey))
                    {
                        Action = fKey;
                    }
                    else
                    {
                        Log.ErrorFormat("Could not parse {0} as function key", value);
                    }
                }
            }
        }

    }

    public class XmlTextKey : XmlKey
    {
        public string Text
        { get; set; }
    }

    public class XmlKey
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public XmlKey()
        {
            Width = 1;
            Height = 1;
            Label = "";
        }

        public int Row
        { get; set; }

        public int Col
        { get; set; }

        public string Label
        { get; set; }

        public string Symbol
        { get; set; }

        public int Width
        { get; set; }

        public int Height
        { get; set; }

    }

    public static class XmlUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ConvertToBoolean(string value)
        {
            if (value.Trim().ToLower().Equals("true"))
                return true;
            else if (value.Trim().ToLower().Equals("false"))
                return false;
            else
            {
                bool bVal = false;
                try
                {
                    bVal = XmlConvert.ToBoolean(value);
                }
                catch (System.Exception)
                {
                    Log.ErrorFormat("Cannot convert string '{0}' to boolean", value);
                }
                return bVal;
            }
        }
    }
}