using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="String"/> to a <see cref="Visibility"/> which is set to <see cref="Visibility.Collapsed"/> when the value is not empty
/// </summary>
public class InvertedStringEmptyToVisibilityConverter : BaseValueConverter<InvertedStringEmptyToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return String.IsNullOrEmpty(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
    }
}