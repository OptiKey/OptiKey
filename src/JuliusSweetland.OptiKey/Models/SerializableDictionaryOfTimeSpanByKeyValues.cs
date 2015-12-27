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
    public class SerializableDictionaryOfTimeSpanByKeyValues : Dictionary<KeyValue, TimeSpan>, IXmlSerializable
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
                FunctionKeys? fk = null;
                if (reader.IsStartElement("functionKey"))
                {
                    reader.ReadStartElement("functionKey");
                    var functionKeyAsString = reader.ReadString();
                    FunctionKeys localFk;
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
                var key = new KeyValue(fk, s);

                //Read value
                reader.ReadStartElement("value");
                reader.ReadStartElement("ticks");
                var valueAsString = reader.ReadString();
                var value = string.IsNullOrEmpty(valueAsString) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(valueAsString);
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
                if (key.FunctionKey != null)
                {
                    writer.WriteStartElement("functionKey");
                    writer.WriteString(key.FunctionKey.Value.ToString());
                    writer.WriteEndElement();
                }
                if (key.String != null)
                {
                    writer.WriteStartElement("str");
                    writer.WriteString(key.String);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                
                //Write value (as ticks because TimeSpan is not XML serialisable)
                TimeSpan value = this[key];
                writer.WriteStartElement("value");
                writer.WriteStartElement("ticks");
                writer.WriteString(XmlConvert.ToString(value));
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
