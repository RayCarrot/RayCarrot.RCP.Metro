#nullable disable
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="String"/> to a <see cref="Visibility"/> which is set to <see cref="Visibility.Visible"/> when the value is not empty
/// </summary>
public class StringEmptyToVisibilityConverter : BaseValueConverter<StringEmptyToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return String.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    }
}