using System;
using System.ComponentModel;
using System.Globalization;

namespace JuliusSweetland.OptiKey.Models
{
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