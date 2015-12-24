using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;

namespace JuliusSweetland.OptiKey.Models
{
    [TypeConverter(typeof(KeyValueConverter))]
    public struct KeyValue : IEquatable<KeyValue>
    {
        public KeyValue(FunctionKeys functionKey)
        {
            FunctionKey = functionKey;
            String = null;
        }
        public KeyValue(string text)
        {
            FunctionKey = null;
            String = text;
        }
        public KeyValue(FunctionKeys? functionKey, string text)
        {
            FunctionKey = functionKey;
            String = text;
        }

        public FunctionKeys? FunctionKey { get; }
        public string String { get; }
        
        public bool StringIsLetter
        {
            get { return String != null && String.Length == 1 && char.IsLetter(String, 0); }
        }
        
        #region Equality 

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
                return (FunctionKey.GetHashCode()*397) ^ (String != null ? String.GetHashCode() : 0);
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
                stringBuilder.Append(String.ConvertEscapedCharsToLiterals());
            }
            
            return stringBuilder.ToString();
        }
    }

    public sealed class KeyValueConverter : TypeConverter
    {
        // Overrides the CanConvertFrom method of TypeConverter.
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
        // Overrides the ConvertFrom method of TypeConverter.
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
        // Overrides the ConvertTo method of TypeConverter.
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
