#nullable disable
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Validation rule data for <see cref="BrowseBoxValidationRule"/>
/// </summary>
public class BrowseValidationRuleData : DependencyObject
{
    /// <summary>
    /// The validation rule to use
    /// </summary>
    public BrowseValidationRule ValidationRule
    {
        get => (BrowseValidationRule)GetValue(ValidationRuleProperty);
        set => SetValue(ValidationRuleProperty, value);
    }

    /// <summary>
    /// The dependency Validation for <see cref="ValidationRule"/>
    /// </summary>
    public static readonly DependencyProperty ValidationRuleProperty = DependencyProperty.Register(nameof(ValidationRule), typeof(BrowseValidationRule), typeof(BrowseValidationRuleData));
}