using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

public class InvertedEnumVisibilityConverter : BaseValueConverter<InvertedEnumVisibilityConverter, Enum, Visibility, Enum>
{
    public override Visibility ConvertValue(Enum value, Type targetType, Enum parameter, CultureInfo culture)
    {
        return value.Equals(parameter) ? Visibility.Collapsed : Visibility.Visible;
    }
}