using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    public static class TabItemProperties
    {
        #region Icon Kind

        public static PackIconMaterialKind GetIconKind(TabItem obj) => (PackIconMaterialKind)obj.GetValue(IconKindProperty);

        public static void SetIconKind(TabItem obj, PackIconMaterialKind value) => obj.SetValue(IconKindProperty, value);

        public static readonly DependencyProperty IconKindProperty = DependencyProperty.RegisterAttached(
            name: "IconKind",
            propertyType: typeof(PackIconMaterialKind),
            ownerType: typeof(TabItemProperties));

        #endregion

        #region Header Font Size

        public static double GetHeaderFontSize(TabItem obj) => (double)obj.GetValue(HeaderFontSizeProperty);

        public static void SetHeaderFontSize(TabItem obj, double value) => obj.SetValue(HeaderFontSizeProperty, value);

        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.RegisterAttached(
            name: "HeaderFontSize",
            propertyType: typeof(double),
            ownerType: typeof(TabItemProperties));

        #endregion

        #region Icon Visibility

        public static Visibility GetIconVisibility(TabItem obj) => (Visibility)obj.GetValue(IconVisibilityProperty);

        public static void SetIconVisibility(TabItem obj, Visibility value) => obj.SetValue(IconVisibilityProperty, value);

        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.RegisterAttached(
            name: "IconVisibility",
            propertyType: typeof(Visibility),
            ownerType: typeof(TabItemProperties));

        #endregion

        #region Icon Foreground

        public static Brush GetIconForeground(TabItem obj) => (Brush)obj.GetValue(IconForegroundProperty);

        public static void SetIconForeground(TabItem obj, Brush value) => obj.SetValue(IconForegroundProperty, value);

        public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.RegisterAttached(
            name: "IconForeground",
            propertyType: typeof(Brush),
            ownerType: typeof(TabItemProperties));

        #endregion
    }
}