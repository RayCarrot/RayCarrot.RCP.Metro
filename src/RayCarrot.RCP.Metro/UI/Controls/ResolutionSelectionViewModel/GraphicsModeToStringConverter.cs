#nullable disable
using System;
using System.Globalization;

namespace RayCarrot.RCP.Metro;

public class GraphicsModeToStringConverter : BaseValueConverter<GraphicsModeToStringConverter, GraphicsMode, string>
{
    public override string ConvertValue(GraphicsMode value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString();
    }

    public override GraphicsMode ConvertValueBack(string value, Type targetType, object parameter, CultureInfo culture)
    {
        return GraphicsMode.TryParse(value, out GraphicsMode g) ? g : null;
    }
}