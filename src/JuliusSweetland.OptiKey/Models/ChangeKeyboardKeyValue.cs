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
    public class ChangeKeyboardKeyValue : KeyValue, IEquatable<ChangeKeyboardKeyValue>
    {

        private readonly string keyboardLink;

        // replace current keyboard instead of pushing on top?
        private readonly bool replaceKeyboard;

        public ChangeKeyboardKeyValue() : base()
        {
            this.keyboardLink = null;
        }

        public ChangeKeyboardKeyValue(string keyboardLink, bool replacePreviousKeyboard=false)
        {
            this.keyboardLink = keyboardLink;
            this.replaceKeyboard = replacePreviousKeyboard;
        }

        public override bool HasContent()
        {
            return (this.keyboardLink != null);
        }

        public string Keyboard { get { return keyboardLink; } }
        public bool Replace { get { return replaceKeyboard; } }

        #region IEquatable

        public static bool Equals(ChangeKeyboardKeyValue x, ChangeKeyboardKeyValue y)
        {
            return x == y;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to KeyValueLink return false:
            ChangeKeyboardKeyValue p = obj as ChangeKeyboardKeyValue;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if objects match
            return (p == this);
        }

        public bool Equals(ChangeKeyboardKeyValue kv)
        {
            if (ReferenceEquals(null, kv)) return false;
            else return (this == kv);   
        }

        public static bool operator ==(ChangeKeyboardKeyValue x, ChangeKeyboardKeyValue y)
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
            return (x.Keyboard == y.Keyboard) &&
                   (x.Replace == y.Replace);
        }

        public static bool operator !=(ChangeKeyboardKeyValue x, ChangeKeyboardKeyValue y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 389) ^ (Keyboard != null ? Keyboard.GetHashCode() : 0);
                hash = (hash * 13) ^ Replace.GetHashCode();
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
            stringBuilder.Append(" replace = ");
            stringBuilder.Append(this.replaceKeyboard);
            
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
                return new ChangeKeyboardKeyValue(text);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((ChangeKeyboardKeyValue)value).String;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
