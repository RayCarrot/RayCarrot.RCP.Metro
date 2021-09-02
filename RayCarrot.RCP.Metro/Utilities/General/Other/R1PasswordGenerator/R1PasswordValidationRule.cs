using RayCarrot.Rayman.Ray1;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    public class R1PasswordValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var supportedChars = Rayman1PS1Password.GetSupportedCharacters();

            if (!(value is string s))
                return new ValidationResult(false, Resources.R1Passwords_Invalid);

            if (s.Length != 10)
                return new ValidationResult(false, Resources.R1Passwords_InvalidLength);

            var invalidChar = s.Select(x => (char?)x).FirstOrDefault(x => !supportedChars.Any(c => c.ToString().Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase)));

            if (invalidChar != null)
                return new ValidationResult(false, String.Format(Resources.R1Passwords_InvalidChar, invalidChar));

            return ValidationResult.ValidResult;

        }
    }
}