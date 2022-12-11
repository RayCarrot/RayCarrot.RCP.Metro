#nullable disable
using System.Globalization;

namespace RayCarrot.RCP.Metro;

public class EqualsToConverter : BaseValueConverter<EqualsToConverter, object, bool, object>
{
    public override bool ConvertValue(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter.Equals(value);
    }

    public override object ConvertValueBack(bool value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter;
    }
}