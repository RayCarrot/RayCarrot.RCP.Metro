using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an <see cref="Object"/> to a <see cref="String"/>
/// </summary>
public class ObjToStringConverter : BaseValueConverter<ObjToStringConverter, object, string>
{
    public override string ConvertValue(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() ?? String.Empty;
    }
}