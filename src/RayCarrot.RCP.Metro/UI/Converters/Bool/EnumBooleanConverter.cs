using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an <see cref="Enum"/> to a <see cref="Boolean"/> which is true if the value equals the parameter value.
/// This is useful for a <see cref="System.Windows.Controls.RadioButton"/> bound to an <see cref="Enum"/>
/// </summary>
public class EnumBooleanConverter : BaseValueConverter<EnumBooleanConverter, Enum, bool, Enum>
{
    public override bool ConvertValue(Enum value, Type targetType, Enum parameter, CultureInfo culture)
    {
        return value.Equals(parameter);
    }

    public override Enum ConvertValueBack(bool value, Type targetType, Enum parameter, CultureInfo culture)
    {
        return parameter;
    }
}