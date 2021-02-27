// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    [XmlRoot(ElementName = "EyeGestures")]
    public class XmlEyeGestures
    {
        [XmlNamespaceDeclarations] public XmlSerializerNamespaces xmlns;

        [XmlElement("Gesture")]
        public ObservableCollection<EyeGesture> GestureList { get; set; }

        public static XmlEyeGestures ReadFromFile(string inputFilename)
        {
            if (!File.Exists(inputFilename) && string.IsNullOrEmpty(Path.GetExtension(inputFilename)))
                inputFilename += ".xml";

            var gestures = new XmlEyeGestures();
            var serializer = new XmlSerializer(typeof(XmlEyeGestures));
            try
            {
                using (var reader = new FileStream(@inputFilename, FileMode.Open))
                {
                    gestures = (XmlEyeGestures)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch { }

            return gestures;
        }

        public static XmlEyeGestures ReadFromString(string xmlString)
        {
            var gestures = new XmlEyeGestures();
            var serializer = new XmlSerializer(typeof(XmlEyeGestures));
            try
            {
                gestures = (XmlEyeGestures)serializer.Deserialize(new StringReader(xmlString));
            }
            catch { }

            return gestures;
        }

        public void WriteToFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return;

            var serializer = new XmlSerializer(typeof(XmlEyeGestures));
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var sw = new StreamWriter(filename);
            var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true });

            serializer.Serialize(xmlWriter, this, ns);
            sw.Close();
        }

        public string WriteToString()
        {
            var serializer = new XmlSerializer(typeof(XmlEyeGestures));

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var sw = new StringWriter();
            var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true });

            serializer.Serialize(xmlWriter, this, ns);
            return sw.ToString();
        }
    }
}