using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an <see cref="Enum"/> to a <see cref="Visibility"/> which is <see cref="Visibility.Visible"/> if the value equals the parameter value.
/// </summary>
public class EnumVisibilityConverter : BaseValueConverter<EnumVisibilityConverter, Enum, Visibility, Enum>
{
    public override Visibility ConvertValue(Enum value, Type targetType, Enum parameter, CultureInfo culture)
    {
        return value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
    }

    public override Enum ConvertValueBack(Visibility value, Type targetType, Enum parameter, CultureInfo culture)
    {
        return parameter;
    }
}