using RayCarrot.Common;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    public class DosBoxResolutionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string s))
                return new ValidationResult(false, value == null ? Resources.DosBoxResolutionValidation_NullOrEmpty : Resources.DosBoxResolutionValidation_InvalidFormat);

            if (s.Equals("original", StringComparison.CurrentCultureIgnoreCase) || s.Equals("desktop", StringComparison.CurrentCultureIgnoreCase))
                return ValidationResult.ValidResult;

            if (s.IsNullOrWhiteSpace())
                return new ValidationResult(false, Resources.DosBoxResolutionValidation_NullOrEmpty);

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
                    return new ValidationResult(false, String.Format(Resources.DosBoxResolutionValidation_InvalidCharacter, c));

                if (!first)
                    return new ValidationResult(false, Resources.DosBoxResolutionValidation_MultipleSeparators);

                first = false;
            }

            if (first)
                return new ValidationResult(false, Resources.DosBoxResolutionValidation_MissingSeparator);

            if (!secondValid)
                return new ValidationResult(false, Resources.DosBoxResolutionValidation_InvalidHeight);

            return ValidationResult.ValidResult;
        }
    }
}