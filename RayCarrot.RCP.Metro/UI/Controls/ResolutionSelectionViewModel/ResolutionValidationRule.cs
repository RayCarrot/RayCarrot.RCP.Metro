using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    public class ResolutionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // TODO-UPDATE Localize

            if (value is not string str)
                return new ValidationResult(false, "Invalid resolution. Must be in the format of 'Width x Height'");

            string[] values = str.Split('x').Select(x => x.Trim()).ToArray();

            if (values.Length != 2)
                return new ValidationResult(false, "Invalid resolution. Must be in the format of 'Width x Height'");

            if (!Int32.TryParse(values[0], out int _))
                return new ValidationResult(false, "Invalid resolution. Must be in the format of 'Width x Height'");

            if (!Int32.TryParse(values[1], out int _))
                return new ValidationResult(false, "Invalid resolution. Must be in the format of 'Width x Height'");

            return ValidationResult.ValidResult;
        }
    }
}