using System.Globalization;
using System.Windows.Media;
using RayCarrot.RCP.Metro.Pages.Games;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Converts an <see cref="AppNewsIcon"/> to an <see cref="ImageSource"/>
/// </summary>
public class AppNewsIconToImageSourceConverter : BaseValueConverter<AppNewsIconToImageSourceConverter, AppNewsIcon, ImageSource?>
{
    public override ImageSource? ConvertValue(AppNewsIcon value, Type targetType, object parameter, CultureInfo culture)
    {
        AppNewsIconAsset asset = value switch
        {
            AppNewsIcon.Main => AppNewsIconAsset.Main,
            AppNewsIcon.Rayman => AppNewsIconAsset.Rayman,
            AppNewsIcon.Patcher => AppNewsIconAsset.Patcher,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };

        return new ImageSourceConverter().ConvertFrom(asset.GetAssetPath()) as ImageSource;
    }
}