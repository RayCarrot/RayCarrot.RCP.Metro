using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Games.Clients.DosBox;

public class CyclesValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not string s)
            return new ValidationResult(false, value == null 
                ? Resources.DosBoxCyclesValidation_NullOrEmpty 
                : Resources.DosBoxCyclesValidation_InvalidFormat);

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