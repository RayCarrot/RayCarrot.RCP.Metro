#nullable disable
using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an <see cref="Object"/> to a <see cref="Visibility"/> which is <see cref="Visibility.Visible"/> if the <see cref="Type"/> specified in the parameter matches that of the value
/// </summary>
public class IsTypeToVisibilityConverter : BaseValueConverter<IsTypeToVisibilityConverter, object, Visibility, Type>
{
    public override Visibility ConvertValue(object value, Type targetType, Type parameter, CultureInfo culture) => 
        value.GetType().IsEquivalentTo(parameter) ? Visibility.Visible : Visibility.Collapsed;
}