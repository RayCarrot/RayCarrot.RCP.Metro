#nullable disable
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Attached properties for the <see cref="UnifiedMarginBehavior"/>
/// </summary>
public static class UnifiedMargin
{
    #region Ignore

    /// <summary>
    /// Gets the value indicating if the control should be ignored from a <see cref="DependencyObject"/>
    /// </summary>
    /// <param name="obj">The object to get the property from</param>
    /// <returns>The property</returns>
    public static bool GetIgnored(DependencyObject obj) => (bool)obj.GetValue(IgnoredProperty);

    /// <summary>
    /// Sets the value indicating if the control should be ignored for a <see cref="DependencyObject"/>
    /// </summary>
    /// <param name="obj">The object to set the property on</param>
    /// <param name="value">The property to set</param>
    public static void SetIgnored(DependencyObject obj, bool value) => obj.SetValue(IgnoredProperty, value);

    /// <summary>
    /// The value indicating if the control should be ignored
    /// </summary>
    public static readonly DependencyProperty IgnoredProperty = DependencyProperty.RegisterAttached("Ignored", typeof(bool), typeof(UnifiedMargin), new PropertyMetadata(false));

    #endregion
}