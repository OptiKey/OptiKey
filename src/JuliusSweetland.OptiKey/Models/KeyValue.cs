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
    public class KeyValue : IEquatable<KeyValue>
    {

        private readonly FunctionKeys? functionKey;
        private string str;

        public KeyValue()
        {
            this.functionKey = null;
            this.str = null;
        }

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

        public static bool Equals(KeyValue x, KeyValue y)
        {
            return x == y;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to KeyValue return false:
            KeyValue p = obj as KeyValue;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if objects match
            return (p == this);
        }

        public bool Equals(KeyValue kv)
        {
            if (ReferenceEquals(null, kv)) return false;
            else return (this == kv);   
        }

        public static bool operator ==(KeyValue x, KeyValue y)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            // Return true if the fields and hash codes match
            // Hash code check allows us to account for differences in 
            // subclasses, even if the (KeyValue) parts are equal.
            return (x.FunctionKey == y.FunctionKey)
                && (x.String == y.String)
                && x.GetHashCode() == y.GetHashCode();
        }

        public static bool operator !=(KeyValue x, KeyValue y)
        {
            return !(x == y);
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

        public virtual bool HasContent() {            
            return (FunctionKey != null) ||
                   (String != null); 
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
