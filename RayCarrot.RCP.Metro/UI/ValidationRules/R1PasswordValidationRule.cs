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
            var supportedChars = R1_PS1_Password.GetSupportedCharacters();

            if (value is string s && s.Length == 10 && s.All(x => supportedChars.Any(c => c.ToString().Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase))))
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, Resources.R1Passwords_Invalid);
        }
    }
}