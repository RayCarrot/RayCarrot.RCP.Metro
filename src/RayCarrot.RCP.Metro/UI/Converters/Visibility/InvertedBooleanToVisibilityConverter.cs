using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Boolean"/> to an inverted <see cref="Visibility"/>
/// </summary>
public class InvertedBooleanToVisibilityConverter : BaseValueConverter<InvertedBooleanToVisibilityConverter, bool, Visibility>
{
    public override Visibility ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture) => 
        value ? Visibility.Collapsed : Visibility.Visible;

    public override bool ConvertValueBack(Visibility value, Type targetType, object parameter, CultureInfo culture) =>
        value != Visibility.Visible;
}