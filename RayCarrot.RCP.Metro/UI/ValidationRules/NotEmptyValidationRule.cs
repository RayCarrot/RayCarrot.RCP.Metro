using System;
using System.Globalization;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Validates if the value is not an empty <see cref="String"/> or just white space
    /// </summary>
    public class NotEmptyValidationRule : ValidationRule
    {
        /// <summary>
        /// Validates the value
        /// </summary>
        /// <param name="value">The value from the binding target to check</param>
        /// <param name="cultureInfo">The culture to use in this rule</param>
        /// <returns>The validation result</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // TODO: Localize
            return String.IsNullOrWhiteSpace(value?.ToString()) ? new ValidationResult(false, "Field is required") : ValidationResult.ValidResult;
        }
    }
}