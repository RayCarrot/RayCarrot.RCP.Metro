using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an <see cref="Enum"/> with underlying type <see cref="Byte"/> to a <see cref="Int32"/> with the type specified in the parameter
/// </summary>
public class ByteEnumToIntConverter : BaseValueConverter<ByteEnumToIntConverter, object, int>
{
    public override int ConvertValue(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (byte)Enum.Parse(value.GetType(), value.ToString());
    }

    public override object ConvertValueBack(int value, Type targetType, object parameter, CultureInfo culture)
    {
        return Enum.Parse(targetType, value.ToString());
    }
}