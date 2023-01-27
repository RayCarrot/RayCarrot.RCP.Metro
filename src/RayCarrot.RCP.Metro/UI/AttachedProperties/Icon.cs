using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro;

public static class Icon
{
    #region Icon Kind

    public static PackIconMaterialKind GetKind(DependencyObject obj) => (PackIconMaterialKind)obj.GetValue(KindProperty);

    public static void SetKind(DependencyObject obj, PackIconMaterialKind value) => obj.SetValue(KindProperty, value);

    public static readonly DependencyProperty KindProperty = DependencyProperty.RegisterAttached(
        name: "Kind",
        propertyType: typeof(PackIconMaterialKind),
        ownerType: typeof(Icon));

    #endregion

    #region Icon Size

    public static double GetSize(DependencyObject obj) => (double)obj.GetValue(SizeProperty);

    public static void SetSize(DependencyObject obj, double value) => obj.SetValue(SizeProperty, value);

    public static readonly DependencyProperty SizeProperty = DependencyProperty.RegisterAttached(
        name: "Size",
        propertyType: typeof(double),
        ownerType: typeof(Icon));

    #endregion

    #region Icon Brush

    public static Brush GetBrush(DependencyObject obj) => (Brush)obj.GetValue(BrushProperty);

    public static void SetBrush(DependencyObject obj, Brush value) => obj.SetValue(BrushProperty, value);

    public static readonly DependencyProperty BrushProperty = DependencyProperty.RegisterAttached(
        name: "Brush",
        propertyType: typeof(Brush),
        ownerType: typeof(Icon));

    #endregion
}