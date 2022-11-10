using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public static class ButtonAssist
{
    #region Icon Kind

    public static PackIconMaterialKind GetIconKind(Button obj) => (PackIconMaterialKind)obj.GetValue(IconKindProperty);

    public static void SetIconKind(Button obj, PackIconMaterialKind value) => obj.SetValue(IconKindProperty, value);

    public static readonly DependencyProperty IconKindProperty = DependencyProperty.RegisterAttached(
        name: "IconKind",
        propertyType: typeof(PackIconMaterialKind),
        ownerType: typeof(ButtonAssist));

    #endregion

    #region Icon Size

    public static double GetIconSize(Button obj) => (double)obj.GetValue(IconSizeProperty);

    public static void SetIconSize(Button obj, double value) => obj.SetValue(IconSizeProperty, value);

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.RegisterAttached(
        name: "IconSize",
        propertyType: typeof(double),
        ownerType: typeof(ButtonAssist));

    #endregion

    #region Icon Brush

    public static Brush GetIconBrush(Button obj) => (Brush)obj.GetValue(IconBrushProperty);

    public static void SetIconBrush(Button obj, Brush value) => obj.SetValue(IconBrushProperty, value);

    public static readonly DependencyProperty IconBrushProperty = DependencyProperty.RegisterAttached(
        name: "IconBrush",
        propertyType: typeof(Brush),
        ownerType: typeof(ButtonAssist));

    #endregion
}