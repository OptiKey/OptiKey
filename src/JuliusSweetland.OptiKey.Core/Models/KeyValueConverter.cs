// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.ComponentModel;
using System.Globalization;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    public sealed class KeyValueConverter : TypeConverter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            if (value == null)
            {
                Log.Warn("Unable to convert from string (to KeyValue) as value was null. This can occur if a selection begins before the keyboard is fully initialised.");
                return null;
            }

            var text = value as string;
            if (text != null)
            {
                return new KeyValue(text);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                Log.Warn("Unable to convert to string (from KeyValue) as value was null. This can occur if a selection begins before the keyboard is fully initialised.");
                return null;
            }

            if (destinationType == typeof(string))
            {
                return ((KeyValue)value).String;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}