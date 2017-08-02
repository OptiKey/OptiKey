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
    [TypeConverter(typeof(KeyValueLinkConverter))]
    public class KeyValueLink : KeyValue, IEquatable<KeyValueLink>
    {

        // TODO: May hold ID or file path or something else?
        private readonly string keyboardLink;

        public KeyValueLink() : base()
        {
            this.keyboardLink = null;
        }

        public KeyValueLink(string keyboardLink)
        {
            this.keyboardLink = keyboardLink;
        }

        public override bool HasContent()
        {
            return (this.keyboardLink != null);
        }

        public string Keyboard { get { return keyboardLink; } }

        #region IEquatable

        public static bool Equals(KeyValueLink x, KeyValueLink y)
        {
            return x == y;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to KeyValueLink return false:
            KeyValueLink p = obj as KeyValueLink;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if objects match
            return (p == this);
        }

        public bool Equals(KeyValueLink kv)
        {
            if (ReferenceEquals(null, kv)) return false;
            else return (this == kv);   
        }

        public static bool operator ==(KeyValueLink x, KeyValueLink y)
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

            // Return true if the fields match:
            return (x.Keyboard == y.Keyboard);
        }

        public static bool operator !=(KeyValueLink x, KeyValueLink y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 389) ^ (Keyboard != null ? Keyboard.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (Keyboard != null)
            {
                stringBuilder.Append(Keyboard);
            }
            
            return stringBuilder.ToString();
        }
    }

    public sealed class KeyValueLinkConverter : TypeConverter
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
                return new KeyValueLink(text);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((KeyValueLink)value).String;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
