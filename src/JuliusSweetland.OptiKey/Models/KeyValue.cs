using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;

namespace JuliusSweetland.OptiKey.Models
{
    [TypeConverter(typeof(KeyValueConverter))]
    public struct KeyValue : IEquatable<KeyValue>
    {
        private readonly FunctionKeys? functionKey;
        private string str;

        public KeyValue(FunctionKeys functionKey)
        {
            this.functionKey = functionKey;
            this.str = null;
        }

        public KeyValue(string str)
        {
            this.functionKey = null;
            this.str = str;
        }

        public KeyValue(FunctionKeys? functionKey, string str)
        {
            this.functionKey = functionKey;
            this.str = str;
        }

        public KeyValue(FunctionKeys functionKey, string str)
        {
            this.functionKey = functionKey;
            this.str = str;
        }

        public FunctionKeys? FunctionKey { get { return functionKey; } }
        public string String
        {
            get { return str; }
            set { str = value; }
        }

        public bool StringIsLetter
        {
            get { return String != null && String.Length == 1 && char.IsLetter(String, 0); }
        }
        
        #region IEquatable

        public bool Equals(KeyValue kv)
        {
            // Return true if the fields match:
            return (FunctionKey == kv.FunctionKey)
                && (String == kv.String);
        }

        public static bool operator ==(KeyValue kv1, KeyValue kv2)
        {
            return kv1.Equals(kv2);
        }

        public static bool operator !=(KeyValue x, KeyValue y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is KeyValue && Equals((KeyValue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 397) ^ (FunctionKey != null ? FunctionKey.GetHashCode() : 0);
                hash = (hash * 397) ^ (String != null ? String.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (FunctionKey != null)
            {
                stringBuilder.Append(FunctionKey);
            }

            if (String != null)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(",");
                }

                //Special chars such as '\n' have meaning in a string - convert to literal strings.
                //This is also required by the Key property as Key is used in dictionary indexes, for example.
                stringBuilder.Append(String.ToPrintableString());
            }
            
            return stringBuilder.ToString();
        }
    }

    public sealed class KeyValueConverter : TypeConverter
    {
        // The ITypeDescriptorContext interface provides the context for the
        // conversion. Typically, this interface is used at design time to 
        // provide information about the design-time container.
        public override bool CanConvertFrom(ITypeDescriptorContext context,
           Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
           CultureInfo culture, object value)
        {
            var text = value as string;
            if (text!=null)
            {
                return new KeyValue(text);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((KeyValue)value).String;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
