using System.Globalization;
using System.Windows.Media.Effects;

namespace RayCarrot.RCP.Metro;

// Implemented from MaterialDesignInXamlToolkit

public class ShadowConverter : BaseValueConverter<ShadowConverter, ShadowDepth, DropShadowEffect?>
{
    public override DropShadowEffect? ConvertValue(ShadowDepth value, Type targetType, object parameter, CultureInfo culture)
    {
        return Clone(ShadowInfo.GetDropShadow(value));
    }

    private static DropShadowEffect? Clone(DropShadowEffect? dropShadowEffect)
    {
        if (dropShadowEffect is null) 
            return null;

        return new DropShadowEffect()
        {
            BlurRadius = dropShadowEffect.BlurRadius,
            Color = dropShadowEffect.Color,
            Direction = dropShadowEffect.Direction,
            Opacity = dropShadowEffect.Opacity,
            RenderingBias = dropShadowEffect.RenderingBias,
            ShadowDepth = dropShadowEffect.ShadowDepth
        };
    }
}