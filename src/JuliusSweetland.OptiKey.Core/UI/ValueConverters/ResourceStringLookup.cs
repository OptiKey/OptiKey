// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Globalization;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class ResourceStringLookup : IValueConverter
    {
        public string Prefix { get; set; } = null;
        public string Suffix { get; set; } = null;

        public bool ConvertToUppercase { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string resourceKey = value.ToString();

            if (Prefix != null)
            {
                resourceKey = Prefix + resourceKey;
            }

            if (Suffix != null)
            {
                resourceKey += Suffix;
            }

            if (ConvertToUppercase)
            {
                resourceKey = resourceKey.ToUpperInvariant();
            }

            return Resources.ResourceManager.GetString(resourceKey, Resources.Culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
