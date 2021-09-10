using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Validates if the value is an existing directory and is not an empty <see cref="String"/>
    /// </summary>
    public class DirectoryExistsAndNotEmptyValidationRule : ValidationRule
    {
        /// <summary>
        /// Validates the value
        /// </summary>
        /// <param name="value">The value from the binding target to check</param>
        /// <param name="cultureInfo">The culture to use in this rule</param>
        /// <returns>The validation result</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (value ?? String.Empty).ToString();

            return !String.IsNullOrEmpty(input) && Directory.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.BrowseBox_ValidationError_DirExists);
        }
    }
}