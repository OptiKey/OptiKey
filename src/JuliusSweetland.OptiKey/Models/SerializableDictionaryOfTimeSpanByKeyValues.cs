using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    [XmlRoot("dictionary")]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)] //This is not really necessary as this class will be serialised into the settings as XML, but I've included it to be explicit
    public class SerializableDictionaryOfTimeSpanByKeyValues : Dictionary<KeyValue, TimeSpan>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(KeyValue));
            
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                //Read key
                reader.ReadStartElement("key");
                var key = (KeyValue)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

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
            var keySerializer = new XmlSerializer(typeof(KeyValue));
            
            foreach (KeyValue key in this.Keys)
            {
                writer.WriteStartElement("item");
                
                //Write key
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
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
