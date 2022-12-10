using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

// TODO-14: Rename to Icon. Then you do local:Icon.Kind, local:Icon.Brush etc.
public static class IconAssist
{
    #region Icon Kind

    public static PackIconMaterialKind GetIconKind(DependencyObject obj) => (PackIconMaterialKind)obj.GetValue(IconKindProperty);

    public static void SetIconKind(DependencyObject obj, PackIconMaterialKind value) => obj.SetValue(IconKindProperty, value);

    public static readonly DependencyProperty IconKindProperty = DependencyProperty.RegisterAttached(
        name: "IconKind",
        propertyType: typeof(PackIconMaterialKind),
        ownerType: typeof(IconAssist));

    #endregion

    #region Icon Size

    public static double GetIconSize(DependencyObject obj) => (double)obj.GetValue(IconSizeProperty);

    public static void SetIconSize(DependencyObject obj, double value) => obj.SetValue(IconSizeProperty, value);

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.RegisterAttached(
        name: "IconSize",
        propertyType: typeof(double),
        ownerType: typeof(IconAssist));

    #endregion

    #region Icon Brush

    public static Brush GetIconBrush(DependencyObject obj) => (Brush)obj.GetValue(IconBrushProperty);

    public static void SetIconBrush(DependencyObject obj, Brush value) => obj.SetValue(IconBrushProperty, value);

    public static readonly DependencyProperty IconBrushProperty = DependencyProperty.RegisterAttached(
        name: "IconBrush",
        propertyType: typeof(Brush),
        ownerType: typeof(IconAssist));

    #endregion
}