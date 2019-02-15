using RayCarrot.CarrotFramework;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    public class DosBoxCyclesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string s))
                return new ValidationResult(false, value == null ? "The cycles can not be empty" : "Invalid format");

            if (s.IsNullOrWhiteSpace())
                return new ValidationResult(false, "The cycles value can not be empty");

            if (s.Equals("default", StringComparison.CurrentCultureIgnoreCase) ||
                s.Equals("auto", StringComparison.CurrentCultureIgnoreCase) ||
                s.Equals("max", StringComparison.CurrentCultureIgnoreCase))
                return ValidationResult.ValidResult;

            foreach (char c in s)
            {
                if (!Char.IsDigit(c))
                    return new ValidationResult(false, "Only digits are allowed when using a specified value");
            }

            return ValidationResult.ValidResult;
        }
    }
}