#nullable disable
using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Enum"/> to a <see cref="Int32"/> with the type specified in the parameter
/// </summary>
public class EnumToIntConverter : BaseValueConverter<EnumToIntConverter, object, int>
{
    public override int ConvertValue(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)Enum.Parse(value.GetType(), value.ToString());
    }

    public override object ConvertValueBack(int value, Type targetType, object parameter, CultureInfo culture)
    {
        return Enum.Parse(targetType, value.ToString());
    }
}