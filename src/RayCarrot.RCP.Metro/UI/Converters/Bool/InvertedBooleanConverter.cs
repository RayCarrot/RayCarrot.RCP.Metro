#nullable disable
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Boolean"/> to its inverted value
/// </summary>
public class InvertedBooleanConverter : BaseValueConverter<InvertedBooleanConverter, bool, bool>
{
    public override bool ConvertValue(bool value, Type targetType, object parameter, CultureInfo culture) =>
        !(value);

    public override bool ConvertValueBack(bool value, Type targetType, object parameter, CultureInfo culture) =>
        !(value);
}