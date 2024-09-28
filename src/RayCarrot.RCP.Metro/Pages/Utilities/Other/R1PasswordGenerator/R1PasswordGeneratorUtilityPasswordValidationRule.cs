using System.Globalization;
using System.Windows.Controls;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro.Pages.Utilities;

public class R1PasswordGeneratorUtilityPasswordValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var supportedChars = PS1Password.GetSupportedCharacters();

        if (value is not string s)
            return new ValidationResult(false, Resources.R1Passwords_Invalid);

        if (s.Length != 10)
            return new ValidationResult(false, Resources.R1Passwords_InvalidLength);

        char? invalidChar = s.Select(x => (char?)x).
            FirstOrDefault(x => !supportedChars.Any(c => c.ToString().Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase)));

        if (invalidChar != null)
            return new ValidationResult(false, String.Format(Resources.R1Passwords_InvalidChar, invalidChar));

        return ValidationResult.ValidResult;
    }
}