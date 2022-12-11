#nullable disable
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="double"/> to its value plus that defined by the parameter
/// </summary>
public class DoubleAdditionConverter : BaseValueConverter<DoubleAdditionConverter, double, double>
{
    public override double ConvertValue(double value, Type targetType, object parameter, CultureInfo culture)
    {
        double add = parameter switch
        {
            double d => d,
            int i => i,
            _ => Double.Parse(parameter.ToString())
        };

        return value + add;
    }

    public override double ConvertValueBack(double value, Type targetType, object parameter, CultureInfo culture)
    {
        double add = parameter switch
        {
            double d => d,
            int i => i,
            _ => Double.Parse(parameter.ToString())
        };

        return value - add;
    }
}