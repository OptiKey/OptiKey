// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    [XmlRoot("dictionary")]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)] //This is not really necessary as this class will be serialised into the settings as XML, but I've included it to be explicit
    public class SerializableDictionaryOfTimeSpanByKeyValues : Dictionary<KeyValue, string>, IXmlSerializable
    {
        #region IXmlSerializable

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                //Read key
                reader.ReadStartElement("key");
                reader.ReadStartElement("keyValue");
                FunctionKeys? fk = null;
                if (reader.IsStartElement("functionKey"))
                {
                    reader.ReadStartElement("functionKey");
                    var functionKeyAsString = reader.ReadString();
                    FunctionKeys localFk;
                    //Remap function keys which have changed names over time
                    if (functionKeyAsString == "AlphaKeyboard")
                    {
                        functionKeyAsString = "Alpha1Keyboard";
                    }
                    else if (functionKeyAsString == "ConversationAlphaKeyboard")
                    {
                        functionKeyAsString = "ConversationAlpha1Keyboard";
                    }
                    if (Enum.TryParse(functionKeyAsString, out localFk))
                    {
                        fk = localFk;
                    }
                    reader.ReadEndElement();
                }
                string s = null;
                if (reader.IsStartElement("str"))
                {
                    reader.ReadStartElement("str");
                    s = reader.ReadString();
                    reader.ReadEndElement();
                }
                reader.ReadEndElement();
                reader.ReadEndElement();
                var key = new KeyValue(fk, s);

                //Read value
                reader.ReadStartElement("value");
                reader.ReadStartElement("ticks");
                var valueAsString = reader.ReadString();
                var value = string.IsNullOrEmpty(valueAsString) ? "0" : valueAsString;
                if (valueAsString.StartsWith("P"))
                {
                    value = XmlConvert.ToTimeSpan(valueAsString).TotalMilliseconds.ToString();
                }
                reader.ReadEndElement();
                reader.ReadEndElement();

                this.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (KeyValue key in this.Keys)
            {
                writer.WriteStartElement("item");
                
                //Write key
                writer.WriteStartElement("key");
                writer.WriteStartElement("keyValue");
                if (key.FunctionKey != null)
                {
                    writer.WriteStartElement("functionKey");
                    writer.WriteString(key.FunctionKey.Value.ToString());
                    writer.WriteEndElement();
                }
                if (key.String != null)
                {
                    writer.WriteStartElement("str");
                    writer.WriteCData(key.String);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                writer.WriteStartElement("ticks");
                writer.WriteString(this[key]);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
