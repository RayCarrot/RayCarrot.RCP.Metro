using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Brush"/> to a brush with an opacity
/// </summary>
public class BrushToBrushWithOpacityConverter : BaseValueConverter<BrushToBrushWithOpacityConverter, SolidColorBrush, SolidColorBrush>
{
    public override SolidColorBrush ConvertValue(SolidColorBrush value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new SolidColorBrush(value.Color)
        {
            Opacity = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture)
        };
    }
}