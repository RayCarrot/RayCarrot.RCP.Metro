using System.Windows.Markup;
using System.Windows.Media;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Markup extension for declaring an <see cref="ImageSource"/> using an assets enum value
/// </summary>
public class AssetImageSource : MarkupExtension
{
    public Enum? Asset { get; set; }

    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (Asset == null)
            return null;
        else
            return new ImageSourceConverter().ConvertFrom(Asset.GetAssetPath());
    }
}