using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Validates if a value for a <see cref="BrowseBox"/>
/// </summary>
public class BrowseBoxValidationRule : ValidationRule
{
    public BrowseBoxValidationRule()
    {
        ValidationData = new BrowseValidationRuleData();
    }

    /// <summary>
    /// Validates the value
    /// </summary>
    /// <param name="value">The value from the binding target to check</param>
    /// <param name="cultureInfo">The culture to use in this rule</param>
    /// <returns>The validation result</returns>
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        string input = (value ?? String.Empty).ToString();

        switch (ValidationData.ValidationRule)
        {
            default:
            case BrowseValidationRule.None:
                return ValidationResult.ValidResult;

            case BrowseValidationRule.NotEmpty:
                return input.IsNullOrWhiteSpace() ? new ValidationResult(false, Resources.BrowseBox_ValidationError_Empty) : ValidationResult.ValidResult;

            case BrowseValidationRule.FileExists:
                return input.IsNullOrWhiteSpace() || File.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.BrowseBox_ValidationError_FileExists);

            case BrowseValidationRule.FileExistsAndNotEmpty:
                return !input.IsNullOrWhiteSpace() && File.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.BrowseBox_ValidationError_FileExists);

            case BrowseValidationRule.DirectoryExists:
                return input.IsNullOrWhiteSpace() || Directory.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.BrowseBox_ValidationError_DirExists);

            case BrowseValidationRule.DirectoryExistsAndNotEmpty:
                return !input.IsNullOrWhiteSpace() && Directory.Exists(input) ? ValidationResult.ValidResult : new ValidationResult(false, Resources.BrowseBox_ValidationError_DirExists);
        }
    }

    /// <summary>
    /// The validation data
    /// </summary>
    public BrowseValidationRuleData ValidationData { get; set; }
}