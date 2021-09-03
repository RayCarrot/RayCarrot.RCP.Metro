using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Validates if the value is an existing file or is an empty <see cref="String"/>
    /// </summary>
    public class FileExistsValidationRule : ValidationRule
    {
        public bool AllowEmpty { get; set; } = true;

        /// <summary>
        /// Validates the value
        /// </summary>
        /// <param name="value">The value from the binding target to check</param>
        /// <param name="cultureInfo">The culture to use in this rule</param>
        /// <returns>The validation result</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (value ?? String.Empty).ToString();
            // TODO: Localize
            return (String.IsNullOrEmpty(input) && AllowEmpty) || File.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, "The file does not exist");
        }
    }
}