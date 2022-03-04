// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Globalization;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class IsCommuniKateMenuKey : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeyValue key = (KeyValue)value;
            if (string.IsNullOrEmpty(key.String))
                return false;
            return key.String.Contains(":action:board:")
                || key.String.Contains(":ext_letters")
                || key.String.Contains(":ext_numbers")
                || key.String.Contains(":ext_mouse");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
