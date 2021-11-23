using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts a <see cref="Bitmap"/> to a <see cref="ImageSource"/>
/// </summary>
public class BitmapToImageSourceConverter : BaseValueConverter<BitmapToImageSourceConverter, Bitmap, ImageSource>
{
    public override ImageSource ConvertValue(Bitmap value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToImageSource();
    }
}