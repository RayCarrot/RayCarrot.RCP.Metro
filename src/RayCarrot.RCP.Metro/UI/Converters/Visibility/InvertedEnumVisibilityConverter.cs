#nullable disable
using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

public class InvertedEnumVisibilityConverter : BaseValueConverter<InvertedEnumVisibilityConverter, Enum, Visibility, string>
{
    public override Visibility ConvertValue(Enum value, Type targetType, string parameter, CultureInfo culture)
    {
        object parameterValue = Enum.Parse(value.GetType(), parameter);

        return parameterValue.Equals(value) ? Visibility.Collapsed : Visibility.Visible;
    }
}