using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

public class GraphicsModeValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string str)
            return new ValidationResult(false, Resources.InvalidResolution);

        if (GraphicsMode.TryParse(str, out _))
            return ValidationResult.ValidResult;
        else
            return new ValidationResult(false, Resources.InvalidResolution);
    }
}