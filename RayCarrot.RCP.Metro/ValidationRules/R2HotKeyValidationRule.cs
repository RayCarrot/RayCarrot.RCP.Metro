using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace RayCarrot.RCP.Metro
{
    public class R2HotKeyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is HotKey key))
                return new ValidationResult(false, "Invalid format");

            if (key.ModifierKeys != ModifierKeys.None)
                return new ValidationResult(false, "Single key must be used");

            if (R2ButtonMappingManager.GetKeyCode(key.Key) == 0)
                return new ValidationResult(false, "Key is not valid");

            return ValidationResult.ValidResult;
        }
    }
}