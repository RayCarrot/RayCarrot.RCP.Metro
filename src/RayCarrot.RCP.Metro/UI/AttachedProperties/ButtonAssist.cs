using System.Windows;
using System.Windows.Controls;
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
}