using System.Windows;
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// An extension to <see cref="RadioButton"/> which fixes an issue when binding 
/// <see cref="IsChecked"/> on multiple controls to a data context
/// </summary>
public class RadioButtonExtended : RadioButton
{
    #region Constructor

    public RadioButtonExtended()
    {
        Checked += RadioButtonExtended_Checked;
        Unchecked += RadioButtonExtended_Unchecked;
    }

    #endregion

    #region Event Handlers

    private void RadioButtonExtended_Unchecked(object sender, RoutedEventArgs e) => IsChecked = false;

    private void RadioButtonExtended_Checked(object sender, RoutedEventArgs e) => IsChecked = true;

    public static void IsCheckedRealChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (((RadioButtonExtended)d).IsChecked == (bool)e.NewValue)
            ((RadioButtonExtended)d).IsChecked = (bool)e.NewValue;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets whether the <see cref="RadioButton"/> is checked
    /// </summary>
    public new bool? IsChecked
    {
        get => (bool?)GetValue(IsCheckedRealProperty);
        set
        {
            SetValue(IsCheckedRealProperty, value);
            SetValue(IsCheckedProperty, value);
        }
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty IsCheckedRealProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(RadioButtonExtended),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                IsCheckedRealChanged));

    #endregion
}