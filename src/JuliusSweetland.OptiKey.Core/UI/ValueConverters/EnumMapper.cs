// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Globalization;
using System.Windows.Data;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    /// <summary>
    /// Maps each member of an enum type to an arbitrary value.
    /// </summary>
    public class EnumMapper : IValueConverter
    {
        /// <summary>
        /// The enum type whose members are to be mapped to values.
        /// </summary>
        public Type EnumType { get; set; }

        /// <summary>
        /// The mapped values for the enum members, in the same order those members would be returned from
        /// <see cref="Enum.GetValues(Type)"/>. If fewer values are provided than there are members, the
        /// remaining members will map to null. If more values are provided than there are members, those
        /// values will simply never be used.
        /// </summary>
        public object[] MappedValues { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || EnumType == null || !EnumType.IsEnum || MappedValues == null)
            {
                return null;
            }

            var members = Enum.GetValues(EnumType);

            for (int i = 0; i < members.Length && i < MappedValues.Length; i++)
            {
                if (Equals(value, members.GetValue(i)))
                {
                    return MappedValues[i];
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
