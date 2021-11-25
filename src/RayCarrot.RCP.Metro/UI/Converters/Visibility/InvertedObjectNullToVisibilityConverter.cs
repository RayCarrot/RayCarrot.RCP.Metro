#nullable disable
using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Object"/> to a <see cref="Visibility"/> which is set to <see cref="Visibility.Collapsed"/> when the value is null
/// </summary>
public class InvertedObjectNullToVisibilityConverter : BaseValueConverter<InvertedObjectNullToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null ? Visibility.Collapsed : Visibility.Visible;
}