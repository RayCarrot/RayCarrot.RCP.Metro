using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

public class R1GbcPasswordGeneratorUtilityPasswordValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not string s)
            return new ValidationResult(false, Resources.R1Passwords_Invalid);

        if (s.Length != 10)
            return new ValidationResult(false, Resources.R1Passwords_InvalidLength);

        char[] supportedChars = RaymanGbcPassword.GetSupportedCharacters();

        char? invalidChar = s.Select(x => (char?)x).FirstOrDefault(x => !supportedChars.Any(c => c == x));

        if (invalidChar != null)
            return new ValidationResult(false, String.Format(Resources.R1Passwords_InvalidChar, invalidChar));

        return ValidationResult.ValidResult;
    }
}