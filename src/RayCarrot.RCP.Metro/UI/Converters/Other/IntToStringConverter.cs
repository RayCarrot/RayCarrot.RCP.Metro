using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an <see cref="Int32"/> to a <see cref="String"/>
/// </summary>
public class IntToStringConverter : BaseValueConverter<IntToStringConverter, int, string>
{
    public override string ConvertValue(int value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }

    public override int ConvertValueBack(string value, Type targetType, object parameter, CultureInfo culture)
    {
        return Int32.Parse(value);
    }
}