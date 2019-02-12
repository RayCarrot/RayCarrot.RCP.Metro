using System;
using System.Globalization;
using System.Windows.Controls;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    public class DosBoxResolutionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string s))
                return new ValidationResult(false, value == null ? "The resolution can not be empty" : "Invalid format");

            if (s.Equals("original", StringComparison.CurrentCultureIgnoreCase))
                return ValidationResult.ValidResult;

            if (s.IsNullOrWhiteSpace())
                return new ValidationResult(false, "The resolution can not be empty");

            bool first = true;
            bool secondValid = false;

            foreach (char c in s)
            {
                if (Char.IsDigit(c))
                {
                    if (!first)
                        secondValid = true;

                    continue;
                }

                if (c != 'x')
                    return new ValidationResult(false, $"The character '{c}' is not valid");

                if (!first)
                    return new ValidationResult(false, "The resolution separator 'x' can only be used once");

                first = false;
            }

            if (first)
                return new ValidationResult(false, "The resolution separator 'x' must be used");

            if (!secondValid)
                return new ValidationResult(false, "A valid height must be specified");

            return ValidationResult.ValidResult;
        }
    }
}