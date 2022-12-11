using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro;

public class DoubleToPointYConverter : BaseValueConverter<DoubleToPointYConverter, double, Point>
{
    public override Point ConvertValue(double value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new Point(0, value);
    }

    public override double ConvertValueBack(Point value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value.Y;
    }
}