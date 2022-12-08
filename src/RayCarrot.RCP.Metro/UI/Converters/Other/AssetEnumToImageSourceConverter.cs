using System;
using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an assets <see cref="Enum"/> to an <see cref="ImageSource"/>
/// </summary>
public class AssetEnumToImageSourceConverter : BaseValueConverter<AssetEnumToImageSourceConverter, Enum, ImageSource?>
{
    public override ImageSource? ConvertValue(Enum value, Type targetType, object parameter, CultureInfo culture)
    {
        return new ImageSourceConverter().ConvertFrom(value.GetAssetPath()) as ImageSource;
    }
}