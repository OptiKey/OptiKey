using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class CompletionTimesValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue))
                return new ValidationResult(false, $"Invalid entry");

            else if (Regex.IsMatch(value.ToString(), "^[1-9][0-9][0-9]+(,[1-9][0-9][0-9]+)*$"))
                return new ValidationResult(true, null);

            else if (Regex.IsMatch(value.ToString(), "^[1-9][0-9][0-9]+,[1-9][0-9][0-9]+(,[1-9][0-9][0-9]+)+(,[1-9][0-9]+)$"))
                return new ValidationResult(true, null);

            else if (Regex.IsMatch(value.ToString(), "^0(,[1-9][0-9][0-9]+)+$"))
                return new ValidationResult(true, null);

            else if (Regex.IsMatch(value.ToString(), "^0,[1-9][0-9][0-9],[1-9][0-9][0-9]+(,[1-9][0-9][0-9]+)*(,[1-9][0-9]+)*$"))
                return new ValidationResult(true, null);

            return new ValidationResult(false, $"Invalid entry");
        }
    }
}