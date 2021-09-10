using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    public class Emulator_DOSBox_CyclesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string s))
                return new ValidationResult(false, value == null ? Resources.DosBoxCyclesValidation_NullOrEmpty : Resources.DosBoxCyclesValidation_InvalidFormat);

            if (s.IsNullOrWhiteSpace())
                return new ValidationResult(false, Resources.DosBoxCyclesValidation_NullOrEmpty);

            if (s.Equals("default", StringComparison.CurrentCultureIgnoreCase) ||
                s.Equals("auto", StringComparison.CurrentCultureIgnoreCase) ||
                s.Equals("max", StringComparison.CurrentCultureIgnoreCase))
                return ValidationResult.ValidResult;

            if (s.Any(c => !Char.IsDigit(c)))
                return new ValidationResult(false, Resources.DosBoxCyclesValidation_NonDigit);

            return ValidationResult.ValidResult;
        }
    }
}