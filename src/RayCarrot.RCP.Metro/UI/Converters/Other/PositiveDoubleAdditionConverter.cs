#nullable disable
using System.Globalization;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="double"/> to its value plus that defined by the parameter and forces it to be a positive value
/// </summary>
public class PositiveDoubleAdditionConverter : BaseValueConverter<PositiveDoubleAdditionConverter, double, double>
{
    public override double ConvertValue(double value, Type targetType, object parameter, CultureInfo culture)
    {
        double add = parameter switch
        {
            double d => d,
            int i => i,
            _ => Double.Parse(parameter.ToString())
        };

        double calculatedValue = value + add;

        if (calculatedValue < 0)
            return 0;

        return calculatedValue;
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